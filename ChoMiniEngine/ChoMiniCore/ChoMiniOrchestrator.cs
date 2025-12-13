using Cysharp.Threading.Tasks;
using MessagePipe;
using System;
using System.Collections.Generic;
using UnityEngine;


namespace Yoru.ChoMiniEngine
{
    // 노드 팩토리를 받아 팩토리에서 나오는 노드 리스트를 재생한다
    public class ChoMiniOrchestrator : IDisposable
    {
        private readonly ChoMiniNodeRunner _runner;
        private ChoMiniSequenceFactory _factory;
        private Action _onComplete;
        ChoMiniLocalMessageContext _localMsg;
        private readonly ISubscriber<ChoMiniCommandAdvanceRequested> _advanceSubscriber;

        private bool _hasStarted = false;
        private bool _disposed = false;

        private IDisposable _advanceSubscription;

        public ChoMiniOrchestrator(
            ChoMiniNodeRunner runner)
        {
            _runner = runner;
            _advanceSubscriber = ChoMiniEngine.CommandContext.AdvanceSubscriber;
            _advanceSubscription = _advanceSubscriber.Subscribe(_ =>
            {
                OnAdvance();
            });
        }
        public void Initialize(ChoMiniLocalMessageContext localMessageContext, Action OnComplate)
        {
            _localMsg = localMessageContext;
            _onComplete = OnComplate;
        }

        private void OnAdvance()
        {
            if (_disposed) return;

            if (!_hasStarted)
            {
                _hasStarted = true;
                PlaySequence().Forget();
            }
            else
            {
                if (_localMsg == null) return;
                _localMsg.SkipPublisher.Publish(new ChoMiniLocalSkipRequested());
            }
        }

        public void SetFactory(ChoMiniSequenceFactory factory) { _factory = factory; }

        public async UniTask PlaySequence()
        {

            List<ChoMiniNode> nodes = new List<ChoMiniNode>();
            int count = _factory.Count;
            for (int i = 0; i < count; i++)
            {
                ChoMiniNode flowNode = _factory.Create();
                nodes.Add(flowNode);
            }

            // 모든 노드 순차 재생
            foreach (var node in nodes)
                await _runner.RunNode(node);  // 노드 하나 끝날 때 까지 대기  

            Debug.Log("리스트 전체 재생 완료");

            // 시퀀스 종료 방송
            _onComplete?.Invoke();
        }

        public void Dispose()
        {
            _disposed = true;

            _advanceSubscription?.Dispose();
            _advanceSubscription = null;

            _onComplete = null;
        }
    }
}
