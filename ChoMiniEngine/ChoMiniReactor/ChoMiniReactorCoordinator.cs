using Cysharp.Threading.Tasks;
using MessagePipe;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using Yoru.ChoMiniEngine;

namespace Yoru.ChoMiniEngine
{
    public sealed class ChoMiniReactorCoordinator : IDisposable
    {
        private readonly ChoMiniNode _node;
        private readonly IDisposable _cleanupSub;
        private bool _disposed;
        private readonly ChoMiniNodeRunner _nodeRunner = new();

        public ChoMiniReactorCoordinator(
            ChoMiniNode node,
            ChoMiniScopeMessageContext msg,
            bool isLifetimeLoop)
        {
            _node = node;

            _cleanupSub = msg.CleanupSubscriber.Subscribe(_ => Dispose());

            if (isLifetimeLoop)
                RunLoop().Forget();
            else
                RunOnce();
        }

        private void RunOnce()
        {
            _nodeRunner.RunNode(_node).Forget();
        }

        private async UniTaskVoid RunLoop()
        {
            while (!_disposed)
            {
                await _nodeRunner.RunNode(_node);
                await UniTask.Yield();
            }
        }

        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;

            _cleanupSub.Dispose();
            _node.Dispose();
        }
    }


}
