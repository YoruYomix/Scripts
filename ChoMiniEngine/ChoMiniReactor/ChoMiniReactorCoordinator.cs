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
        private readonly ReactorRule _rule;
        private readonly ChoMiniScopeMessageContext _msg;

        private readonly CancellationTokenSource _cts = new();
        private IDisposable _cleanupSub;
        private bool _disposed;

        public ChoMiniReactorCoordinator(
            ReactorRule rule,
            ChoMiniScopeMessageContext msg)
        {
            _rule = rule;
            _msg = msg;

            _cleanupSub = _msg.CleanupSubscriber.Subscribe(_ => Dispose());

            //  Provider 없는 Reactor → Do 1회
            if (_rule.ProviderType == null)
            {
                _rule.DoHook?.Invoke();
                return;
            }

            // 🟢 Provider 있는 Reactor
            if (_rule.IsLifetimeLoop)
            {
                RunLoop().Forget();
            }
            else
            {
                ExecuteOnce();
            }
        }

        private async UniTaskVoid RunLoop()
        {
            try
            {
                while (!_cts.IsCancellationRequested)
                {
                    ExecuteOnce();
                    await UniTask.Yield(PlayerLoopTiming.Update, _cts.Token);
                }
            }
            catch (OperationCanceledException)
            {
                // 정상 종료
            }
        }

        private void ExecuteOnce()
        {
            // 다음 단계: Provider → Node 생성 → NodeRunner.Run(node)
            _rule.DoHook?.Invoke();
        }

        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;

            Debug.Log(
                _rule.ProviderType != null
                    ? $"[ReactorCoordinator] disposed: {_rule.ProviderType.Name}"
                    : "[ReactorCoordinator] disposed: Do-only reactor"
            );

            _cts.Cancel();
            _cleanupSub?.Dispose();
            _cleanupSub = null;
        }
    }

}
