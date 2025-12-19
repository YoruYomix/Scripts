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
        ChoMiniScopeMessageContext _scopeMsg;

        private bool _hasStarted = false;
        private bool _disposed = false;
        private bool _paused = false;
        private bool _stopped = false;


        public ChoMiniOrchestrator(ChoMiniNodeRunner runner)
        {
            _runner = runner;
        }
        public void Initialize(IChoMiniFactory factory, ChoMiniScopeMessageContext scopeMessageContext)
        {
            _factory = factory;
            _scopeMsg = scopeMessageContext;
        }
        public void Pause()
        {
            if (_disposed) return;
            if (!_hasStarted) return;
            if (_paused) return;

            _paused = true;
            _runner.Pause();
        }
        public void Resume()
        {
            if (_disposed) return;
            if (!_hasStarted) return;
            if (!_paused) return;

            _paused = false;
            _runner.Resume();
        }
        public void CompleteSequence()
        {
            if (_disposed) return;
            if (_scopeMsg == null) return;
            if (!_hasStarted)
            {
                return;
            }
            _scopeMsg.CompletePublisher.Publish(new ChoMiniScopeCompleteRequested());
        }
        public void PlayCompleteSequence()
        {
            if (_disposed) return;
            if (_scopeMsg == null) return;
            if (!_hasStarted)
            {
                return;
            }
            _scopeMsg.SequenceCompletePublisher.Publish(new ChoMiniSOrchestratorPlaySequenceCompleteRequested());
        }
        public async UniTask PlaySequence()
        {
            if (_disposed || _stopped)
                return;

            int count = _factory.Count;
            List<ChoMiniNode> nodes = new List<ChoMiniNode>();

            for (int i = 0; i < count; i++)
            {
                if (_disposed || _stopped)
                    return;

                ChoMiniNode node = _factory.Create();
                nodes.Add(node);
            }

            int runIndex = 0;
            _hasStarted = true;

            foreach (var node in nodes)
            {
                if (_disposed || _stopped)
                    return;

                Debug.Log($"▶ before RunNode index={runIndex}");

                await _runner.RunNode(node);

                if (_disposed || _stopped)
                    return;

                Debug.Log($"▶ after RunNode index={runIndex}");
                runIndex++;
            }

            if (_disposed || _stopped)
                return;

            Debug.Log("[오케스트레이터] PlaySequence COMPLETE");
            PlayCompleteSequence();
        }

        public void Stop()
        {
            if (_disposed) return;
            if (!_hasStarted) return;
            if (_stopped) return;

            _stopped = true;
            _paused = false;

            _runner.Stop();   // ⭐ 핵심
        }
        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;

            Stop();
            _runner.Dispose();

        }
    }
}
