using Cysharp.Threading.Tasks;
using DG.Tweening;
using System;
using System.Collections.Generic;
using System.Threading;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// UIElements: 특정 UI 패널의 상태(Graphic, TMP_Text, 원본 알파 등)만 관리
/// </summary>
public class UIElements
{
    public List<Graphic> Graphics { get; private set; } = new List<Graphic>();
    public Dictionary<Graphic, float> OriginalAlphas { get; private set; } = new Dictionary<Graphic, float>();

    public List<TMP_Text> TMPs { get; private set; } = new List<TMP_Text>();
    public Dictionary<TMP_Text, float> OriginalTMPAlphas { get; private set; } = new Dictionary<TMP_Text, float>();

    public UIElements(Transform root)
    {
        // Graphic 탐색 (Image, RawImage, Text)
        Graphics.AddRange(root.GetComponentsInChildren<Image>(true));
        Graphics.AddRange(root.GetComponentsInChildren<RawImage>(true));
        Graphics.AddRange(root.GetComponentsInChildren<Text>(true));

        // TMP_Text 탐색
        TMPs.AddRange(root.GetComponentsInChildren<TMP_Text>(true));

        // 원래 알파 저장
        foreach (var g in Graphics)
            OriginalAlphas[g] = g.color.a;

        foreach (var t in TMPs)
            OriginalTMPAlphas[t] = t.color.a;
    }
}

/// <summary>
/// UIFader: UIElements를 이용하여 페이드 인/아웃 처리
/// </summary>
public class UIFaderV2
{
    private UIElements elements;

    public UIFaderV2(UIElements uiElements)
    {
        elements = uiElements ?? throw new ArgumentNullException(nameof(uiElements));
    }

    /// <summary>
    /// Fade In: 현재 알파 -> 원래 알파
    /// </summary>
    public void FadeIn(float duration, Action onComplete = null)
    {
        Sequence seq = DOTween.Sequence();

        foreach (var g in elements.Graphics)
        {
            g.DOKill();
            seq.Join(g.DOFade(elements.OriginalAlphas[g], duration).SetEase(Ease.OutQuad));
        }

        foreach (var t in elements.TMPs)
        {
            t.DOKill();
            seq.Join(t.DOFade(elements.OriginalTMPAlphas[t], duration).SetEase(Ease.OutQuad));
        }

        seq.OnComplete(() => onComplete?.Invoke()).Play();
    }


    public async UniTask FadeInAsync(float duration, CancellationToken cts)
    {
        Debug.Log("FadeInAsync 시작.");
        Sequence seq = DOTween.Sequence();

        // Graphics
        foreach (var g in elements.Graphics)
        {
            g.DOKill();
            // Start State (ZeroAlpha)는 상위 호출자(PlayAsync)가 책임지므로, 여기서는 DOKill만 수행
            seq.Join(g.DOFade(elements.OriginalAlphas[g], duration).SetEase(Ease.OutQuad));
        }

        // TMPs
        foreach (var t in elements.TMPs)
        {
            t.DOKill();
            // Start State (ZeroAlpha)는 상위 호출자(PlayAsync)가 책임지므로, 여기서는 DOKill만 수행
            seq.Join(t.DOFade(elements.OriginalTMPAlphas[t], duration).SetEase(Ease.OutQuad));
        }


        try
        {
            // DOTween 시퀀스 시작
            seq.Play();

            // DOTween 시퀀스 완료까지 대기 (외부 CancellationToken 연동)
            await seq.AsyncWaitForCompletion()
                     .AsUniTask() // System.Threading.Tasks.Task를 Cysharp.Threading.Tasks.UniTask로 변환
                     .AttachExternalCancellation(cts); // 외부 취소 토큰 연결
        }
        catch (OperationCanceledException)
        {
            // SRP 준수: 취소 시 Fader는 트윈 Kill만 담당합니다.
            // UI 상태 복원(RestoreOriginal)은 상위 호출자(PlayAsync)의 책임입니다.
            Debug.Log("FadeInAsync cancelled. Sequence will be killed in finally block.");

            // OperationCanceledException을 다시 던져 상위 호출자가 취소되었음을 알립니다.
            throw;
        }
        finally
        {
            // 시퀀스가 아직 활성 상태라면 Kill (메모리 정리)
            if (seq.IsActive())
            {
                seq.Kill();
            }
        }
    }

    /// <summary>
    /// Fade Out: 현재 알파 -> 0
    /// </summary>
    public void FadeOut(float duration, Action onComplete = null)
    {
        Sequence seq = DOTween.Sequence();

        foreach (var g in elements.Graphics)
        {
            g.DOKill();
            seq.Join(g.DOFade(0f, duration).SetEase(Ease.OutQuad));
        }

        foreach (var t in elements.TMPs)
        {
            t.DOKill();
            seq.Join(t.DOFade(0f, duration).SetEase(Ease.OutQuad));
        }

        seq.OnComplete(() => onComplete?.Invoke()).Play();
    }

    /// <summary>
    /// 즉시 원래 상태로 복원 (Color 구조체 수정으로 최적화)
    /// </summary>
    public void RestoreOriginal()
    {
        foreach (var g in elements.Graphics)
        {
            if (g == null) continue;
            g.DOKill();
            Color c = g.color;
            c.a = elements.OriginalAlphas[g];
            g.color = c;
        }

        foreach (var t in elements.TMPs)
        {
            if (t == null) continue;
            t.DOKill();
            Color c = t.color;
            c.a = elements.OriginalTMPAlphas[t];
            t.color = c;
        }
    }

    /// <summary>
    /// 즉시 알파를 0으로 설정 (Color 구조체 수정으로 최적화)
    /// </summary>
    public void ZeroAlpha()
    {
        foreach (var g in elements.Graphics)
        {
            if (g == null) continue;
            g.DOKill();
            Color c = g.color;
            c.a = 0f;
            g.color = c;
        }

        foreach (var t in elements.TMPs)
        {
            if (t == null) continue;
            t.DOKill();
            Color c = t.color;
            c.a = 0f;
            t.color = c;
        }
    }
}