using Cysharp.Threading.Tasks;
using MessagePipe;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

public class LoopPlayer : System.IDisposable
{
    private readonly List<(IFlowNodeEffect effect, float duration)> _entries
        = new List<(IFlowNodeEffect effect, float duration)>();



    private bool _isRunning = false;
    private int _runningTaskCount;

    private CancellationTokenSource _cts;   // 내부 전용 토큰 (외부 절대 접근 불가)

    public LoopPlayer()
    {

    }
    public void Register(IFlowNodeEffect effect, float duration)
    {
        _entries.Add((effect, duration));
    }



    // 방송으로 호출됨
    private void StartLoop()
    {
        if (_isRunning) return;  // 중복 실행 방지
        if (_entries.Count == 0) return; // 루프 대상 없음

        _isRunning = true;
        // 이전 CTS가 있었다면 폐기
        _cts?.Dispose();
        _cts = new CancellationTokenSource();

        foreach (var entry in _entries)
        {
            _runningTaskCount++;
            _ = RunSingleLoop(entry.effect, entry.duration, _cts.Token);
        }
    }


    private async UniTask RunSingleLoop(IFlowNodeEffect effect, float duration, CancellationToken token)
    {
        try
        {
            while (!token.IsCancellationRequested)
            {
                Debug.Log($"루프 재생됨: {effect.GameObject.name}");
                effect.Play();

                // ★ duration 동안 대기하지만 Cancel 되면 즉시 깨어난다
                await UniTask.Delay(TimeSpan.FromSeconds(duration),
                                    cancellationToken: token);
            }
        }
        finally
        {
            // 루프 하나 종료됨
            _runningTaskCount--;

            // 모든 루프가 정상적으로 종료되면 LoopPlayer 스스로 죽는다
            if (_runningTaskCount == 0)
            {
                Dispose();
            }
        }
    }

    private void RequestStop()
    {
        // 즉시 취소 → 모든 delay가 즉시 깨어남
        _cts?.Cancel();
    }

    public void Dispose()
    {
        Debug.Log("LoopPlayer Dispose() — 모든 루프가 자연 종료됨");

        _entries.Clear();
        _cts?.Dispose();
        _cts = null;
    }
}


