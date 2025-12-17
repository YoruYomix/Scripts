using System;
using MessagePipe;

namespace Yoru.ChoMiniEngine
{
    /// <summary>
    /// Cleanup이 필요한 Action들을 위한 베이스 클래스
    /// - Cleanup은 메시지 기반
    /// - Node / Orchestrator는 이 클래스를 모름
    /// </summary>
    public abstract class ChoMiniCleanupActionBase
        : IChoMiniNodeAction, IDisposable
    {
        private IDisposable _cleanupSubscription;
        private bool _cleanedUp;

        protected ChoMiniCleanupActionBase(
            ISubscriber<ChoMiniScopeCleanupRequested> cleanupSubscriber)
        {
            _cleanupSubscription =
                cleanupSubscriber.Subscribe(_ => CleanupInternal());
        }

        // =========================
        // Cleanup Entry
        // =========================
        private void CleanupInternal()
        {
            if (_cleanedUp) return;
            _cleanedUp = true;

            OnCleanup();
            Dispose();
        }

        /// <summary>
        /// 실제 정리 로직은 여기서 구현
        /// (CTS 취소, Tween Kill, Loop 중단 등)
        /// </summary>
        protected abstract void OnCleanup();

        // =========================
        // IDisposable
        // =========================
        public void Dispose()
        {
            _cleanupSubscription?.Dispose();
            _cleanupSubscription = null;
        }

        // =========================
        // IChoMiniNodeAction (공통)
        // =========================
        public abstract float GetRequiredDuration();
        public abstract void Play();

        public virtual void Complete() { }
        public virtual void Pause() { }
        public virtual void Resume() { }
        public virtual void Recovery(float time) { }

        public abstract UnityEngine.GameObject GameObject { get; }
    }
}