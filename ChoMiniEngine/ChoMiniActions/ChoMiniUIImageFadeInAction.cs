using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace Yoru.ChoMiniEngine
{
    public sealed class ChoMiniUIImageFadeInAction : ChoMiniCleanupActionBase
    {
        public override float GetRequiredDuration() => _fadeDuration;

        private readonly Image _image;
        private readonly Color _originalColor;
        private Tween _tween;
        private readonly float _fadeDuration = 2f;

        public ChoMiniUIImageFadeInAction(
            Image image,
            ChoMiniScopeMessageContext scopeMsg)
            : base(scopeMsg.CleanupSubscriber)
        {
            _image = image;
            _originalColor = image.color;
        }

        // ==============================
        // Play Control
        // ==============================
        public override void Play()
        {
            // 기존 트윈 정리 (중복 방어)
            _tween?.Kill(false);
            _tween = null;

            Color c = _originalColor;
            c.a = 0f;
            _image.color = c;

            _tween = _image
                .DOFade(_originalColor.a, _fadeDuration)
                .SetAutoKill(false);
        }

        public override void Complete()
        {
            // 스킵 = 최종 상태 보장
            _tween?.Kill(true);
            _tween = null;
        }

        public override void Pause()
        {
            if (_tween == null) return;
            if (!_tween.IsActive()) return;
            if (!_tween.IsPlaying()) return;

            _tween.Pause();
        }

        public override void Resume()
        {
            if (_tween == null) return;
            if (!_tween.IsActive()) return;
            if (_tween.IsPlaying()) return;

            _tween.Play();
        }

        public override void Recovery(float time)
        {
            // DOTween은 시간 기반 복구도 가능하지만
            // 현재 엔진 단계에서는 의도적으로 비워둠
        }

        // ==============================
        // Cleanup
        // ==============================
        protected override void OnCleanup()
        {
            // 수명 종료 = 중단만
            _tween?.Kill(true);
            _tween = null;
        }
    }
}

