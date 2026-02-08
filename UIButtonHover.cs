using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using DG.Tweening;
using System;
using System.Collections.Generic;
using TMPro; // 👈 [1] TMPro 네임스페이스 추가

public class UIButtonHover : MonoBehaviour,
    IPointerEnterHandler, IPointerExitHandler,
    IPointerDownHandler, IPointerUpHandler
{
    [Header("기본 상태")]
    Image[] imageDefault;
    Text[] textDefault;
    public TextMeshProUGUI[] tmpDefault; // 👈 [2] TMP 필드 추가

    [Header("Hover 상태")]
    Image[] imageHover;
    Text[] textHover;
    public TextMeshProUGUI[] tmpHover; // 👈 [2] TMP 필드 추가

    [Header("클릭 관련")]
    float onHoverDuration = 0.1f;

    [Header("클릭하고 있을 때의 버튼 크기")]
    public float clickScale = 0.95f;
    [Header("클릭하고 있을 때의 버튼 크기가 줄어드는 속도")]
    public float clickDuration = 0.1f;
    [Header("클릭하고 난 뒤 버튼이 원래 크기로 돌아오는 속도")]
    public float springDuration = 0.1f;

    private bool isPointerDown = false;
    private bool isPointerOver = false;

    private Vector3 originalScale;

    private Sequence hoverSequence;
    private Sequence clickSequence;

    public Action onClickEvent;

    private CanvasGroup canvasGroup;
    private bool isInit = false;

    // 전역 입력/애니메이션 차단
    public static bool IsUIBlocked = false;

    // ----------------- 초기화 -----------------
    public void Init(Action buttonSelected)
    {
        if (isInit) return;
        isInit = true;
        InitAutoAssign(); // TMP 포함 자동 할당

        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null) canvasGroup = gameObject.AddComponent<CanvasGroup>();

        originalScale = transform.localScale;
        onClickEvent = buttonSelected;

        // 초기 알파값 세팅 (TMP 포함)
        SetAlpha(imageHover, 0f);
        SetAlpha(textHover, 0f);
        SetAlpha(tmpHover, 0f);

        SetAlpha(imageDefault, 0f);
        SetAlpha(textDefault, 0f);
        SetAlpha(tmpDefault, 0f);

        canvasGroup.alpha = 0f;
        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;
    }

    private void InitAutoAssign()
    {
        // Default 하위 + 자신
        Transform defaultParent = transform.Find("default");
        if (defaultParent != null)
        {
            List<Image> images = new List<Image>();
            List<Text> texts = new List<Text>();
            List<TextMeshProUGUI> tmps = new List<TextMeshProUGUI>(); // 👈 TMP 리스트
            GetGraphicsSelfAndChildren(defaultParent, images, texts, tmps); // 👈 TMP 인자 전달
            imageDefault = images.ToArray();
            textDefault = texts.ToArray();
            tmpDefault = tmps.ToArray(); // 👈 TMP 배열 할당
        }

        // OnCursor 하위 + 자신
        Transform hoverParent = transform.Find("hover");
        if (hoverParent != null)
        {
            List<Image> images = new List<Image>();
            List<Text> texts = new List<Text>();
            List<TextMeshProUGUI> tmps = new List<TextMeshProUGUI>(); // 👈 TMP 리스트
            GetGraphicsSelfAndChildren(hoverParent, images, texts, tmps); // 👈 TMP 인자 전달
            imageHover = images.ToArray();
            textHover = texts.ToArray();
            tmpHover = tmps.ToArray(); // 👈 TMP 배열 할당
        }
    }

    // 👈 [3] GetGraphicsSelfAndChildren 메서드에 TMP 인자 추가 및 탐색 추가
    private void GetGraphicsSelfAndChildren(Transform parent, List<Image> images, List<Text> texts, List<TextMeshProUGUI> tmps)
    {
        // 자신 먼저
        var img = parent.GetComponent<Image>();
        if (img != null) images.Add(img);

        var txt = parent.GetComponent<Text>();
        if (txt != null) texts.Add(txt);

        var tmp = parent.GetComponent<TextMeshProUGUI>();
        if (tmp != null) tmps.Add(tmp);

        // 자식 재귀 탐색
        foreach (Transform child in parent)
            GetGraphicsSelfAndChildren(child, images, texts, tmps);
    }


    // ----------------- 알파 초기화 -----------------
    private void SetAlpha(Image[] group, float alpha)
    {
        if (group == null) return;
        foreach (var img in group)
            if (img != null)
            {
                var c = img.color;
                c.a = alpha;
                img.color = c;
            }
    }

    private void SetAlpha(Text[] group, float alpha)
    {
        if (group == null) return;
        foreach (var txt in group)
            if (txt != null)
            {
                var c = txt.color;
                c.a = alpha;
                txt.color = c;
            }
    }

    // 👈 [4] TMP용 SetAlpha 메서드 추가
    private void SetAlpha(TextMeshProUGUI[] group, float alpha)
    {
        if (group == null) return;
        foreach (var tmp in group)
            if (tmp != null)
            {
                var c = tmp.color;
                c.a = alpha;
                tmp.color = c;
            }
    }


    // ----------------- 안전 페이드 통합 -----------------
    // 👈 [5] FadeUI 메서드에 TMP 인자 추가 및 처리
    private void FadeUI(Image[] images, Text[] texts, TextMeshProUGUI[] tmps, float targetAlpha, float duration, bool blockUI, Action onComplete = null)
    {
        if (canvasGroup == null) return;

        if (blockUI) BlockAllUI(true);

        Sequence seq = DOTween.Sequence();

        fadeGroup(images, targetAlpha, seq, duration);
        fadeGroup(texts, targetAlpha, seq, duration);
        fadeGroup(tmps, targetAlpha, seq, duration); // 👈 TMP 페이드 추가

        seq.Join(canvasGroup.DOFade(targetAlpha, duration));

        seq.OnComplete(() =>
        {
            if (blockUI) BlockAllUI(false);

            bool visible = targetAlpha > 0f;
            canvasGroup.interactable = visible;
            canvasGroup.blocksRaycasts = visible;

            onComplete?.Invoke();
        });

        seq.Play();
    }

    // ----------------- 즉시 숨기기 -----------------
    public void HideInstant()
    {
        hoverSequence?.Kill(true);
        clickSequence?.Kill(true);

        // Hover / Default 모두 0 (TMP 포함)
        SetAlpha(imageHover, 0f);
        SetAlpha(textHover, 0f);
        SetAlpha(tmpHover, 0f);

        SetAlpha(imageDefault, 0f);
        SetAlpha(textDefault, 0f);
        SetAlpha(tmpDefault, 0f);

        if (canvasGroup == null)
            canvasGroup = GetComponent<CanvasGroup>();

        canvasGroup.alpha = 0f;
        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;

        isPointerDown = false;
        isPointerOver = false;
    }

    // ----------------- 즉시 드러내기 -----------------
    public void ShowInstant()
    {
        hoverSequence?.Kill(true);
        clickSequence?.Kill(true);

        // Hover는 꺼져 있고 Default만 보이도록 (TMP 포함)
        SetAlpha(imageHover, 0f);
        SetAlpha(textHover, 0f);
        SetAlpha(tmpHover, 0f);

        SetAlpha(imageDefault, 1f);
        SetAlpha(textDefault, 1f);
        SetAlpha(tmpDefault, 1f);

        if (canvasGroup == null)
            canvasGroup = GetComponent<CanvasGroup>();

        canvasGroup.alpha = 1f;
        canvasGroup.interactable = true;
        canvasGroup.blocksRaycasts = true;

        isPointerDown = false;
        isPointerOver = false;
    }


    public void FadeInDefaultBlocking(float fadeDuration)
    {
        if (canvasGroup == null) return;

        // Hover는 숨기기 (TMP 포함)
        SetAlpha(imageHover, 0f);
        SetAlpha(textHover, 0f);
        SetAlpha(tmpHover, 0f);

        // Default 알파는 건드리지 않음 ← 중요!

        canvasGroup.alpha = 0f; // ← fade 시작지점

        BlockAllUI(true);
        // 👈 FadeUI 호출 시 Default의 TMP 인자(tmpDefault) 전달
        FadeUI(imageDefault, textDefault, tmpDefault, 1f, fadeDuration, false, () =>
        {
            canvasGroup.interactable = true;
            canvasGroup.blocksRaycasts = true;
            BlockAllUI(false);
        });
    }


    public void FadeOutCurrentBlocking(float fadeDuration)
    {
        if (canvasGroup == null) return;

        if (isPointerOver)
            // 👈 FadeUI 호출 시 Hover의 TMP 인자(tmpHover) 전달
            FadeUI(imageHover, textHover, tmpHover, 0f, fadeDuration, true);
        else
            // 👈 FadeUI 호출 시 Default의 TMP 인자(tmpDefault) 전달
            FadeUI(imageDefault, textDefault, tmpDefault, 0f, fadeDuration, true);
    }

    // ----------------- 호버/클릭 안전 시퀀스 -----------------
    private void PlayHover(bool enter)
    {
        if (IsUIBlocked) return;

        hoverSequence?.Kill(true);
        hoverSequence = DOTween.Sequence();

        if (enter)
        {
            fadeGroup(imageHover, 1f, hoverSequence, onHoverDuration);
            fadeGroup(textHover, 1f, hoverSequence, onHoverDuration);
            fadeGroup(tmpHover, 1f, hoverSequence, onHoverDuration); // 👈 TMP 페이드 추가
        }
        else
        {
            fadeGroup(imageHover, 0f, hoverSequence, onHoverDuration);
            fadeGroup(textHover, 0f, hoverSequence, onHoverDuration);
            fadeGroup(tmpHover, 0f, hoverSequence, onHoverDuration); // 👈 TMP 페이드 추가
        }

        hoverSequence.Play();
    }

    private void PlayClick(bool down)
    {
        if (IsUIBlocked) return;

        clickSequence?.Kill(true);
        clickSequence = DOTween.Sequence();

        if (down)
        {
            clickSequence.Append(transform.DOScale(originalScale * clickScale, clickDuration));
        }
        else
        {
            clickSequence.Append(transform.DOScale(originalScale, springDuration).SetEase(Ease.OutCubic));
            if (isPointerDown && isPointerOver)
                clickSequence.AppendCallback(() => onClickEvent?.Invoke());
        }

        clickSequence.Play();
    }

    // ----------------- 입력 이벤트 -----------------
    public void OnPointerEnter(PointerEventData eventData)
    {
        isPointerOver = true;
        PlayHover(true);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        isPointerOver = false;
        isPointerDown = false;
        PlayHover(false);
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        isPointerDown = true;
        PlayClick(true);
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        PlayClick(false);
        isPointerDown = false;
    }

    // ----------------- 시퀀스 페이드 헬퍼 -----------------
    private void fadeGroup(Image[] group, float alpha, Sequence seq, float duration)
    {
        if (group == null) return;
        foreach (var img in group)
            if (img != null) seq.Join(img.DOFade(alpha, duration));
    }

    private void fadeGroup(Text[] group, float alpha, Sequence seq, float duration)
    {
        if (group == null) return;
        foreach (var txt in group)
            if (txt != null) seq.Join(txt.DOColor(new Color(txt.color.r, txt.color.g, txt.color.b, alpha), duration));
    }

    // 👈 [6] TMP용 fadeGroup 메서드 추가
    private void fadeGroup(TextMeshProUGUI[] group, float alpha, Sequence seq, float duration)
    {
        if (group == null) return;
        foreach (var tmp in group)
            if (tmp != null) seq.Join(tmp.DOFade(alpha, duration)); // TMP는 DOFade 지원
    }

    // ----------------- 전역 유아이 차단 -----------------
    public static void BlockAllUI(bool block)
    {
        IsUIBlocked = block;

        var buttons = GameObject.FindObjectsOfType<UIButtonHover>();
        foreach (var btn in buttons)
        {
            btn.hoverSequence?.Kill(true);
            btn.clickSequence?.Kill(true);

            if (btn.canvasGroup != null)
            {
                btn.canvasGroup.interactable = !block;
                btn.canvasGroup.blocksRaycasts = !block;
            }
        }
    }
}