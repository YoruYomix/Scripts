using Cysharp.Threading.Tasks;
using System;
using System.Threading;
using UnityEngine;
using UnityEngine.UI;
using MessagePipe;

namespace Yoru.ChoMiniEngine
{
    public sealed class ChoMiniTextComponentCursorBlinkAction : ChoMiniCleanupActionBase
    {
        // ==============================
        // IChoMiniNodeAction
        // ==============================
        public override float GetRequiredDuration() => _duration;

        // ==============================
        // Fields
        // ==============================
        private readonly Text _text;
        private readonly float _blinkSpeed;

        private readonly string _fullText;
        private readonly float _duration;

        private CancellationTokenSource _cts;

        // ==============================
        // Constructor
        // ==============================
        public ChoMiniTextComponentCursorBlinkAction(
            Text text,
            ChoMiniScopeMessageContext scopeMsg,
            float blinkSpeed = 0.5f)
            : base(scopeMsg.CleanupSubscriber)
        {
            _text = text;
            _blinkSpeed = blinkSpeed;

            _fullText = _text.text;

            // ▼ 표시 + 제거 = 2 step
            _duration = blinkSpeed * 2f;
        }

        // ==============================
        // Play Control
        // ==============================
        public override void Play()
        {
            Cancel();
            _cts = new CancellationTokenSource();
            BlinkOnceAsync(_cts.Token).Forget();
        }

        public override void Complete()
        {
            Cancel();
            _text.text = _fullText;
        }

        public override void Pause()
        {
            Cancel();
        }

        public override void Resume()
        {
            Play();
        }

        public override void Recovery(float time)
        {
            // 단순 1회 연출 → 복구 개념 없음
        }

        // ==============================
        // Internal
        // ==============================
        private async UniTask BlinkOnceAsync(CancellationToken ct)
        {
            try
            {
                await UniTask.Yield(PlayerLoopTiming.PostLateUpdate);

                // 1) 커서 표시
                _text.text = _fullText + "\n▼";
                await UniTask.Delay(
                    TimeSpan.FromSeconds(_blinkSpeed),
                    cancellationToken: ct);

                // 2) 커서 제거
                _text.text = _fullText;
                await UniTask.Delay(
                    TimeSpan.FromSeconds(_blinkSpeed),
                    cancellationToken: ct);
            }
            catch (OperationCanceledException)
            {
                _text.text = _fullText;
            }
        }

        private void Cancel()
        {
            if (_cts == null) return;
            if (_cts.IsCancellationRequested) return;

            _cts.Cancel();
            _cts.Dispose();
            _cts = null;
        }

        // ==============================
        // Cleanup
        // ==============================
        protected override void OnCleanup()
        {
            Cancel();
            _text.text = _fullText;
        }
    }
}