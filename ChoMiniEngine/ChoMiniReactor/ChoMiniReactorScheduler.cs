using System;
using System.Collections.Generic;
using MessagePipe;

namespace Yoru.ChoMiniEngine
{
    /// <summary>
    /// ReactorScheduler
    /// - ReactorRule을 보관
    /// - NodeSource를 검사
    /// - Scope / Orchestrator 이벤트를 구독
    /// - 조건 만족 시 Coordinator를 생성
    ///
    /// ※ 누구에게도 소유되지 않음
    /// ※ 구독으로 생존, Cleanup 이벤트로 종료
    /// </summary>
    public sealed class ChoMiniReactorScheduler : IDisposable
    {
        // ======================================================
        // Immutable inputs (Scope lifetime)
        // ======================================================

        private readonly IReadOnlyList<ReactorRule> _rules;
        private readonly IReadOnlyList<NodeSource> _nodeSources;
        private readonly ChoMiniScopeMessageContext _messageContext;

        // ======================================================
        // Subscriptions
        // ======================================================

        private readonly List<IDisposable> _subscriptions = new();

        private bool _disposed;

        // ======================================================
        // Constructor
        // ======================================================

        public ChoMiniReactorScheduler(
            IReadOnlyList<ReactorRule> reactorRules,
            IReadOnlyList<NodeSource> nodeSources,
            ChoMiniScopeMessageContext messageContext)
        {
            _rules = reactorRules ?? throw new ArgumentNullException(nameof(reactorRules));
            _nodeSources = nodeSources ?? throw new ArgumentNullException(nameof(nodeSources));
            _messageContext = messageContext ?? throw new ArgumentNullException(nameof(messageContext));

            SubscribeEvents();
        }

        // ======================================================
        // Event subscription
        // ======================================================

        private void SubscribeEvents()
        {
            // 예: 노드 재생 완료 이벤트
            _subscriptions.Add(
                _messageContext.CompleteSubscriber.Subscribe(OnScopeCompleted)
            );

            // Scope Cleanup 이벤트
            _subscriptions.Add(
                _messageContext.CleanupSubscriber.Subscribe(_ => Dispose())
            );
        }

        // ======================================================
        // Event handlers
        // ======================================================

        private void OnScopeCompleted(ChoMiniScopeCompleteRequested msg)
        {
            if (_disposed)
                return;

            // TODO:
            // 1) ReactorContext 구성
            // 2) ReactorRule 조건 검사
            // 3) 만족하는 Rule에 대해
            //    - Coordinator 생성 (fire-and-forget)
        }

        // ======================================================
        // Dispose (lifetime ends by Cleanup event)
        // ======================================================

        public void Dispose()
        {
            if (_disposed)
                return;

            _disposed = true;

            foreach (var sub in _subscriptions)
            {
                sub.Dispose();
            }

            _subscriptions.Clear();
        }
    }
}