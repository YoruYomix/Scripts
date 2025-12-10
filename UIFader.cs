using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using System;
using System.Collections.Generic;
using TMPro;

public class UIFader : MonoBehaviour
{
    // Graphic (Image, Text, RawImage 등) 관련 필드
    private List<Graphic> graphics = new List<Graphic>();
    private Dictionary<Graphic, float> originalAlphas = new Dictionary<Graphic, float>();

    // TMP_Text 관련 필드
    private List<TMP_Text> tmpTexts = new List<TMP_Text>();
    private Dictionary<TMP_Text, float> originalTMPAlphas = new Dictionary<TMP_Text, float>();

    public void Init()
    {
        graphics.Clear();
        originalAlphas.Clear();
        tmpTexts.Clear();
        originalTMPAlphas.Clear();

        // ⭐ MODIFIED: TMP_Text와의 충돌을 피하기 위해 일반 Graphic 컴포넌트만 명시적으로 찾습니다.
        graphics.AddRange(GetComponentsInChildren<Image>(true));
        graphics.AddRange(GetComponentsInChildren<RawImage>(true)); // RawImage 지원 추가
        graphics.AddRange(GetComponentsInChildren<Text>(true)); // 레거시 Text 지원

        // 2. 하위 모든 TMP_Text를 찾습니다.
        tmpTexts.AddRange(GetComponentsInChildren<TMP_Text>(true));

        // 3. Graphic의 원래 알파 저장 후 0으로 초기화
        foreach (var g in graphics)
        {
            originalAlphas[g] = g.color.a;
            g.color = new Color(g.color.r, g.color.g, g.color.b, 0f);
        }

        // 4. TMP_Text의 원래 알파 저장 후 0으로 초기화
        foreach (var t in tmpTexts)
        {
            originalTMPAlphas[t] = t.color.a;
            t.color = new Color(t.color.r, t.color.g, t.color.b, 0f);
        }
    }

    /// <summary>
    /// UI 페이드 인 (원래 알파 값 기준)
    /// </summary>
    public void FadeIn(float fadeDuration, Action onComplete = null)
    {
        Sequence seq = DOTween.Sequence();

        // Graphic 페이드 인
        foreach (var g in graphics)
        {
            g.DOKill();
            if (originalAlphas.ContainsKey(g))
            {
                seq.Join(g.DOFade(originalAlphas[g], fadeDuration).SetEase(Ease.OutQuad));
            }
        }

        // TMP_Text 페이드 인
        foreach (var t in tmpTexts)
        {
            t.DOKill();
            if (originalTMPAlphas.ContainsKey(t))
            {
                seq.Join(t.DOFade(originalTMPAlphas[t], fadeDuration).SetEase(Ease.OutQuad));
            }
        }

        seq.OnComplete(() => onComplete?.Invoke());
        seq.Play();
    }

    /// <summary>
    /// UI 페이드 아웃 (원래 알파 값 기준)
    /// </summary>
    public void FadeOut(float fadeDuration, Action onComplete = null)
    {
        Sequence seq = DOTween.Sequence();

        // Graphic 페이드 아웃
        foreach (var g in graphics)
        {
            g.DOKill();
            seq.Join(g.DOFade(0f, fadeDuration).SetEase(Ease.OutQuad));
        }

        // TMP_Text 페이드 아웃
        foreach (var t in tmpTexts)
        {
            t.DOKill();
            seq.Join(t.DOFade(0f, fadeDuration).SetEase(Ease.OutQuad));
        }

        seq.OnComplete(() => onComplete?.Invoke());
        seq.Play();
    }
}