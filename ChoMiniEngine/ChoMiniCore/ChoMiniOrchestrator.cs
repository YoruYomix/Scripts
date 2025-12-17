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
        private IChoMiniFactory _factory;
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
            _advanceSubscriber = ChoMiniBootstrapper.CommandContext.AdvanceSubscriber;
            _advanceSubscription = _advanceSubscriber.Subscribe(_ =>
            {
                OnAdvance();
            });
        }
        public void Initialize(IChoMiniFactory factory, ChoMiniLocalMessageContext localMessageContext, Action OnComplate)
        {
            _factory = factory;
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



        public async UniTask PlaySequence()
        {
            Debug.Log("▶ PlaySequence ENTER");

            Debug.Log($"runner null? {_runner == null}");
            Debug.Log($"factory null? {_factory == null}");

            // ---- Count 체크 ----
            Debug.Log("▶ before factory.Count");
            Debug.Log(_factory);
            Debug.Log(_factory.Count);
            int count = _factory.Count;
            Debug.Log($"▶ factory.Count = {count}");

            var nodes = new List<ChoMiniNode>();

            for (int i = 0; i < count; i++)
            {
                Debug.Log($"▶ before Create() index={i}");

                ChoMiniNode node = _factory.Create();



                nodes.Add(node);
            }

            Debug.Log($"▶ nodes created: {nodes.Count}");

            // ---- RunNode 체크 ----
            int runIndex = 0;
            foreach (var node in nodes)
            {
                Debug.Log($"▶ before RunNode index={runIndex}, node null? {node == null}");

                await _runner.RunNode(node);

                Debug.Log($"▶ after RunNode index={runIndex}");
                runIndex++;
            }

            Debug.Log("▶ PlaySequence COMPLETE");
            Debug.Log("▶ 리스트 전체 재생 완료");

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
