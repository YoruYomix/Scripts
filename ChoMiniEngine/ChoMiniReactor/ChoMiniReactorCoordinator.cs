using Cysharp.Threading.Tasks;
using MessagePipe;
using System;
using System.Threading;
using UnityEngine;

namespace Yoru.ChoMiniEngine
{
    public sealed class ChoMiniReactorCoordinator : IDisposable
    {
        private readonly Func<ChoMiniNode> _createNode;
        private readonly Action _do;
        private readonly bool _isLifetimeLoop;
        private readonly IDisposable _cleanupSub;
        private bool _disposed;

        private readonly CancellationTokenSource _cts = new();
        private UniTask _runTask;

        public ChoMiniReactorCoordinator(
            Func<ChoMiniNode> createNode,
            ChoMiniScopeMessageContext msg,
            bool isLifetimeLoop,
            Action doHook)
        {
            _createNode = createNode;
            _do = doHook;
            _isLifetimeLoop = isLifetimeLoop;

            _cleanupSub = msg.CleanupSubscriber.Subscribe(_ => Dispose());

            _runTask = RunAsync(_cts.Token);
        }

        private async UniTask RunAsync(CancellationToken ct)
        {
            do
            {
                ct.ThrowIfCancellationRequested();

                _do?.Invoke();

                using var node = _createNode();

                var runner = new ChoMiniNodeRunner();

                await runner.RunNode(node)
                            .AttachExternalCancellation(ct);

                await UniTask.Yield();

            } while (_isLifetimeLoop && !ct.IsCancellationRequested);
        }

        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;

            _cts.Cancel();
            _cleanupSub.Dispose();
            _cts.Dispose();
        }
    }


}
