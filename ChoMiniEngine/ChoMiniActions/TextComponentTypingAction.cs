using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading;
using UnityEngine;
using UnityEngine.UI;

namespace Yoru.ChoMiniEngine
{
    public sealed class TextComponentTypingAction : IChoMiniNodeAction
    {
        // ==============================
        // IChoMiniNodeAction
        // ==============================
        public float GetRequiredDuration() => _duration;
        public GameObject GameObject => _text.gameObject;

        // ==============================
        // Fields
        // ==============================
        private readonly Text _text;
        private readonly float _delayPerChar;
        private readonly List<string> _steps = new();

        private readonly string _fullText;
        private readonly float _duration;

        private CancellationTokenSource _cts;

        // ==============================
        // Constructor
        // ==============================
        public TextComponentTypingAction(
            Text text,
            float delayPerChar = 0.05f)
        {
            _text = text;
            _delayPerChar = delayPerChar;

            _fullText = _text.text;

            BuildTypingSteps(_fullText, _steps);

            // ★ 핵심: 듀레이션 역산
            _duration = _steps.Count * _delayPerChar;
        }

        // ==============================
        // Play Control
        // ==============================
        public void Play()
        {
            _cts = new CancellationTokenSource();
            PlayAsync(_cts.Token).Forget();
        }

        public void Complete()
        {
            Cancel();
            _text.text = _fullText;
        }

        public void Pause()
        {
            Cancel();
        }

        public void Resume()
        {
            Play();
        }

        public void Recovery(float time)
        {
            // 필요 시 구현
            // (예: time 기준으로 몇 글자까지 출력할지 계산)
        }

        // ==============================
        // Internal
        // ==============================
        private async UniTask PlayAsync(CancellationToken ct)
        {
            _text.text = "";

            try
            {
                foreach (var step in _steps)
                {
                    if (ct.IsCancellationRequested)
                        break;

                    _text.text = step;
                    await UniTask.Delay(
                        TimeSpan.FromSeconds(_delayPerChar),
                        cancellationToken: ct);
                }
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
        // Typing Logic (Service에서 이식)
        // ==============================
        private static void BuildTypingSteps(
            string input,
            List<string> result)
        {
            result.Clear();

            string temp = "";
            Stack<string> openTags = new Stack<string>();

            var regex = new Regex(@"(<.*?>|\n|.)");
            var matches = regex.Matches(input);

            foreach (Match match in matches)
            {
                string piece = match.Value;

                if (piece.StartsWith("<") && piece.EndsWith(">"))
                {
                    if (piece.StartsWith("</"))
                    {
                        if (openTags.Count > 0)
                            openTags.Pop();

                        temp += piece;
                    }
                    else
                    {
                        openTags.Push(piece);
                        temp += piece;
                    }
                }
                else
                {
                    temp += piece;

                    string display = temp;

                    foreach (var tag in openTags)
                    {
                        string tagName =
                            Regex.Match(tag, @"<(\w+)").Groups[1].Value;

                        display += $"</{tagName}>";
                    }

                    result.Add(display);
                }
            }

            if (result.Count == 0 || result[^1] != temp)
                result.Add(temp);
        }
    }
}
