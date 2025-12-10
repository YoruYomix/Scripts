using UnityEngine;
using UnityEngine.UI;
using TMPro; // TMP 지원을 위해 추가
using DG.Tweening;
using System;
using System.Collections.Generic;

/// <summary>
/// Image(Gray Material) + Text/TMP(Color) 자동 서서히 흑백 전환 기능
/// </summary>
public class UIGrayscaleTween : MonoBehaviour
{
    [Header("Image 흑백 Material (자동 Resources 할당)")]
    Material grayscaleMaterial;

    [Header("전환 시간")]
    float transitionDuration = 0.5f;

    [Header("DOTween Ease")]
    public Ease tweenEase = Ease.OutQuad;

    private List<Image> images = new List<Image>();
    private List<Text> texts = new List<Text>();
    private List<TextMeshProUGUI> tmps = new List<TextMeshProUGUI>(); // ⭐ 추가: TMP 리스트

    private Dictionary<Image, Material> originalImageMaterials = new Dictionary<Image, Material>();
    private Dictionary<Text, Color> originalTextColors = new Dictionary<Text, Color>();
    private Dictionary<TextMeshProUGUI, Color> originalTMPColors = new Dictionary<TextMeshProUGUI, Color>(); // ⭐ 추가: TMP 원본 색상

    /// <summary>
    /// 수동 초기화. SetActive(false) 상태에서도 호출 가능
    /// </summary>
    public void Initialize()
    {
        // Gray Material 자동 할당
        if (grayscaleMaterial == null)
        {
            grayscaleMaterial = Resources.Load<Material>("Gray");
            if (grayscaleMaterial == null)
                Debug.LogWarning("Gray Material을 Resources 폴더에서 찾을 수 없습니다!");
        }

        // 자식 UI 탐색
        images.Clear();
        texts.Clear();
        tmps.Clear(); // 초기화
        images.AddRange(GetComponentsInChildren<Image>(true));
        texts.AddRange(GetComponentsInChildren<Text>(true));
        tmps.AddRange(GetComponentsInChildren<TextMeshProUGUI>(true)); // ⭐ TMP 탐색

        // 원래 Material/Color 저장
        originalImageMaterials.Clear();
        originalTextColors.Clear();
        originalTMPColors.Clear(); // 초기화

        foreach (var img in images)
            originalImageMaterials[img] = img.material;

        foreach (var txt in texts)
            originalTextColors[txt] = txt.color;

        foreach (var tmp in tmps) // ⭐ TMP 색상 저장
            originalTMPColors[tmp] = tmp.color;
    }

    /// <summary>
    /// 컬러 → 흑백 서서히 전환
    /// </summary>
    public void ToGrayscale(float _duration, Action onComplete = null)
    {
        transitionDuration = _duration;
        Sequence seq = DOTween.Sequence();
        DOTween.Kill(this); // 이전 Tween 종료 (안전장치)

        // Image 처리
        foreach (var img in images)
        {
            if (grayscaleMaterial != null)
                img.material = grayscaleMaterial;

            if (img.material.HasProperty("_Blend"))
            {
                img.material.SetFloat("_Blend", 0f);
                seq.Join(img.material.DOFloat(1f, "_Blend", transitionDuration).SetEase(tweenEase));
            }
        }

        // Text 및 TMP 처리 (쉐이더와 동일한 Luminance 로직 적용)
        Action<Text> tweenText = (txt) =>
        {
            if (!originalTextColors.ContainsKey(txt)) return;
            Color orig = originalTextColors[txt];
            txt.DOKill();
            seq.Join(DOTween.To(() => 0f, x =>
            {
                float gray = orig.r * 0.299f + orig.g * 0.587f + orig.b * 0.114f;
                float r = Mathf.Lerp(orig.r, gray, x);
                float g = Mathf.Lerp(orig.g, gray, x);
                float b = Mathf.Lerp(orig.b, gray, x);
                txt.color = new Color(r, g, b, orig.a);
            }, 1f, transitionDuration).SetEase(tweenEase));
        };

        Action<TextMeshProUGUI> tweenTMP = (tmp) => // ⭐ TMP용 로직
        {
            if (!originalTMPColors.ContainsKey(tmp)) return;
            Color orig = originalTMPColors[tmp];
            tmp.DOKill();
            seq.Join(DOTween.To(() => 0f, x =>
            {
                float gray = orig.r * 0.299f + orig.g * 0.587f + orig.b * 0.114f;
                float r = Mathf.Lerp(orig.r, gray, x);
                float g = Mathf.Lerp(orig.g, gray, x);
                float b = Mathf.Lerp(orig.b, gray, x);
                tmp.color = new Color(r, g, b, orig.a);
            }, 1f, transitionDuration).SetEase(tweenEase));
        };

        foreach (var txt in texts) tweenText(txt);
        foreach (var tmp in tmps) tweenTMP(tmp); // ⭐ TMP 적용

        seq.OnComplete(() => onComplete?.Invoke()).SetTarget(this).Play();
    }

    /// <summary>
    /// 흑백 → 컬러 서서히 전환
    /// </summary>
    public void ToColor(float _duration, Action onComplete = null)
    {
        transitionDuration = _duration;
        Sequence seq = DOTween.Sequence();
        DOTween.Kill(this); // 이전 Tween 종료 (안전장치)

        // Image 처리
        foreach (var img in images)
        {
            if (!originalImageMaterials.ContainsKey(img)) continue;

            if (img.material.HasProperty("_Blend"))
            {
                seq.Join(img.material.DOFloat(0f, "_Blend", transitionDuration)
                    .SetEase(tweenEase)
                    .OnComplete(() => img.material = originalImageMaterials[img]));
            }
            else
            {
                img.material = originalImageMaterials[img];
            }
        }

        // Text 및 TMP 처리 (쉐이더와 동일한 Luminance 로직 적용)
        Action<Text> restoreText = (txt) =>
        {
            if (!originalTextColors.ContainsKey(txt)) return;
            Color orig = originalTextColors[txt];
            txt.DOKill();
            seq.Join(DOTween.To(() => 1f, x =>
            {
                float gray = orig.r * 0.299f + orig.g * 0.587f + orig.b * 0.114f;
                float r = Mathf.Lerp(orig.r, gray, x);
                float g = Mathf.Lerp(orig.g, gray, x);
                float b = Mathf.Lerp(orig.b, gray, x);
                txt.color = new Color(r, g, b, orig.a);
            }, 0f, transitionDuration).SetEase(tweenEase));
        };

        Action<TextMeshProUGUI> restoreTMP = (tmp) => // ⭐ TMP용 로직
        {
            if (!originalTMPColors.ContainsKey(tmp)) return;
            Color orig = originalTMPColors[tmp];
            tmp.DOKill();
            seq.Join(DOTween.To(() => 1f, x =>
            {
                float gray = orig.r * 0.299f + orig.g * 0.587f + orig.b * 0.114f;
                float r = Mathf.Lerp(orig.r, gray, x);
                float g = Mathf.Lerp(orig.g, gray, x);
                float b = Mathf.Lerp(orig.b, gray, x);
                tmp.color = new Color(r, g, b, orig.a);
            }, 0f, transitionDuration).SetEase(tweenEase));
        };

        foreach (var txt in texts) restoreText(txt);
        foreach (var tmp in tmps) restoreTMP(tmp); // ⭐ TMP 적용

        seq.OnComplete(() => onComplete?.Invoke()).SetTarget(this).Play();
    }

    /// <summary>
    /// 현재 상태와 상관없이 즉시 원본 상태로 복원
    /// </summary>
    public void RestoreOriginal()
    {
        DOTween.Kill(this);

        // Image 원본 Material 복원
        foreach (var img in images)
        {
            // ⭐ 추가: img가 null(파괴된 객체)인지 확인합니다.
            if (img == null) continue;

            if (originalImageMaterials.ContainsKey(img))
                img.material = originalImageMaterials[img];
        }

        // Text 원본 Color 복원
        foreach (var txt in texts)
        {
            // ⭐ 추가: txt가 null(파괴된 객체)인지 확인합니다.
            if (txt == null) continue;

            if (originalTextColors.ContainsKey(txt))
                txt.color = originalTextColors[txt];
        }

        // ⭐ TMP 원본 Color 복원
        foreach (var tmp in tmps)
        {
            // ⭐ 추가: tmp가 null(파괴된 객체)인지 확인합니다.
            if (tmp == null) continue;

            if (originalTMPColors.ContainsKey(tmp))
                tmp.color = originalTMPColors[tmp];
        }
    }

    /// <summary>
    /// 즉시 흑백 상태로 전환 (Tween 없이 바로 적용)
    /// </summary>
    public void SetInstantGrayscale()
    {
        DOTween.Kill(this);

        // Image 처리
        foreach (var img in images)
        {
            if (grayscaleMaterial != null)
                img.material = grayscaleMaterial;

            if (img.material.HasProperty("_Blend"))
                img.material.SetFloat("_Blend", 1f);
        }

        // Text 및 TMP 처리
        Action<Graphic, Color> setInstantGray = (graphic, orig) =>
        {
            float gray = orig.r * 0.299f + orig.g * 0.587f + orig.b * 0.114f;
            graphic.color = new Color(gray, gray, gray, orig.a);
        };

        foreach (var txt in texts)
        {
            if (originalTextColors.ContainsKey(txt))
                setInstantGray(txt, originalTextColors[txt]);
        }

        foreach (var tmp in tmps) // ⭐ TMP 적용
        {
            if (originalTMPColors.ContainsKey(tmp))
                setInstantGray(tmp, originalTMPColors[tmp]);
        }
    }
}