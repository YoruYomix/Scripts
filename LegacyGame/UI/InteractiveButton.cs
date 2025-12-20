using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;
using System;
using System.Collections.Generic;

// 상태 기반 애니메이션 제어를 위한 Enum 추가
public enum ButtonState
{
    Normal,
    Hover,
    Pressed // OnPointerDown 상태
}

// IPointerUpHandler 인터페이스 포함
public class InteractiveButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler, IPointerDownHandler, IPointerUpHandler
{


    public Action onClickAction;
    public Action onHoverAction;
    public Action onUnHoverAction;

    [Header("--- ⚙️ 일반 설정 ---")]
    [SerializeField] private bool _isInteractable = true;
    public bool UseGrayscaleOnDisable = true;

    // ⭐ NEW: 현재 버튼 상태 변수
    private ButtonState _currentState = ButtonState.Normal;
    // ⭐ NEW: 포인터가 버튼 영역 위에 있는지 추적하는 변수
    private bool _isPointerInside = false;

    public bool IsInteractable
    {
        get => _isInteractable;
        set
        {
            if (_isInteractable == value) return;
            _isInteractable = value;
            ApplyInteractableVisual(value);
            // 비활성화 시 상태를 Normal로 강제 설정
            if (!value)
            {
                _isPointerInside = false;
                _currentState = ButtonState.Normal;
            }
        }
    }

    [Header("--- 🖱️ Hover 스케일 설정 ---")]
    public bool UseHoverScale = true;
    public float hoverScaleFactor = 1.2f;
    public float hoverScaleDuration = 0.1f;

    [Header("--- 🎨 Hover Fade 설정 ---")]
    public bool UseHoverFade = false;
    public float FadeDuration = 0.1f;
    public Transform FadeUIParent;

    [Header("--- 👆 Click Down 설정 ---")]
    public bool UseClickDownScale = true;
    public float clickScaleFactor = 0.95f;
    public float clickDownduration = 0.1f;

    private RectTransform rectTransform;
    private Vector3 originalScale;
    private Tween currentScaleTween; // ⭐ DOTween Kill 명확화

    private UIGrayscaleTween grayscaleTween;
    private UIFader fadeHandler;


    // ⭐ NEW: 1번 개선 - 참조 획득 로직 통합
    private bool TryGetReferences()
    {
        if (rectTransform == null)
        {
            rectTransform = GetComponent<RectTransform>();
        }

        if (rectTransform == null)
        {
            Debug.LogError("RectTransform 컴포넌트가 필요합니다.");
            return false;
        }

        if (originalScale == Vector3.zero)
        {
            originalScale = rectTransform.localScale;
        }
        return true;
    }


    private void Awake()
    {
        if (!TryGetReferences())
        {
            enabled = false;
            return;
        }

        InitializeGrayscale();

        // --- Fade 기능 초기화 ---
        if (UseHoverFade)
        {
            if (FadeUIParent != null)
            {
                fadeHandler = FadeUIParent.GetComponent<UIFader>();
                if (fadeHandler == null)
                {
                    fadeHandler = FadeUIParent.gameObject.AddComponent<UIFader>();
                }

                fadeHandler.Init();

                if (fadeHandler.gameObject.activeSelf)
                {
                    fadeHandler.gameObject.SetActive(false);
                }
            }
            else
            {
                Debug.LogWarning($"[{gameObject.name}] UseHoverFade가 켜져 있지만, FadeUIParent가 설정되지 않아 페이드 기능을 사용할 수 없습니다.");
                UseHoverFade = false;
            }
        }

        ApplyInteractableVisual(_isInteractable);
    }



    private void InitializeGrayscale()
    {
        grayscaleTween = GetComponent<UIGrayscaleTween>();
        if (grayscaleTween == null)
        {
            grayscaleTween = gameObject.AddComponent<UIGrayscaleTween>();
        }
        grayscaleTween.Initialize();
    }

    private void ApplyInteractableVisual(bool isInteractable)
    {
        if (!isInteractable)
        {
            // ⭐ DOTween.Kill(this) 대신 명시적 Tween Kill
            currentScaleTween?.Kill();
            currentScaleTween = null;

            if (rectTransform != null)
            {
                rectTransform.localScale = originalScale;
            }

            if (fadeHandler != null)
            {
                fadeHandler.FadeOut(0f, null);
                fadeHandler.gameObject.SetActive(false);
            }
        }

        if (!UseGrayscaleOnDisable)
        {
            if (isInteractable && grayscaleTween != null)
            {
                grayscaleTween.RestoreOriginal();
            }
            return;
        }

        if (grayscaleTween == null) return;

        if (isInteractable)
        {
            grayscaleTween.RestoreOriginal();
        }
        else
        {
            grayscaleTween.SetInstantGrayscale();
        }
    }

    private void OnValidate()
    {
        if (!TryGetReferences()) return;

        if (!Application.isPlaying && grayscaleTween == null)
        {
            InitializeGrayscale();
        }

        if (rectTransform != null)
        {
            ApplyInteractableVisual(_isInteractable);
        }
    }

    private void SetButtonVisualState(ButtonState targetState)
    {
        if (!_isInteractable || !rectTransform)
        {
            _currentState = ButtonState.Normal;
            return;
        }

        if (_currentState == targetState) return;
        ButtonState previousState = _currentState; // 이전 상태 저장
        _currentState = targetState;

        Vector3 targetScale = originalScale;
        float duration = hoverScaleDuration; // 기본값
        bool needsScaleChange = true;

        // 1. 목표 스케일 및 듀레이션 설정
        switch (targetState)
        {
            case ButtonState.Hover:
                if (UseHoverScale)
                {
                    targetScale = originalScale * hoverScaleFactor;
                }
                else
                {
                    targetScale = originalScale;
                    needsScaleChange = true;
                }
                break;
            case ButtonState.Pressed:
                if (UseClickDownScale)
                {
                    targetScale = originalScale * clickScaleFactor;
                    duration = clickDownduration;
                }
                else
                {
                    needsScaleChange = false;
                }
                break;
            case ButtonState.Normal:
                break;
        }

        // 2. 스케일 애니메이션 실행
        if (targetState == ButtonState.Normal || needsScaleChange)
        {
            // ⭐ DOTween.Kill(this) 대신 명시적 Tween Kill
            currentScaleTween?.Kill();
            currentScaleTween = rectTransform.DOScale(targetScale, duration)
                .SetEase(Ease.OutQuad)
                // .SetId(this) // ID 제거
                .OnComplete(() =>
                {
                    currentScaleTween = null;

                    // ⭐ 델리게이트 대신 UniRx 발행
                    if (targetState == ButtonState.Hover)
                    {
                        onHoverAction?.Invoke();
                    }
                });
        }
        else if (currentScaleTween != null)
        {
            // needsScaleChange가 false일 때도 이전 트윈은 중지
            currentScaleTween?.Kill();
            currentScaleTween = null;
        }


        // 3. Fade 애니메이션 실행
        if (UseHoverFade && fadeHandler != null)
        {
            if (targetState == ButtonState.Hover || targetState == ButtonState.Pressed)
            {
                fadeHandler.gameObject.SetActive(true);
                fadeHandler.FadeIn(FadeDuration);
            }
            else if (targetState == ButtonState.Normal && !_isPointerInside)
            {
                fadeHandler.FadeOut(FadeDuration, () =>
                {
                    fadeHandler.gameObject.SetActive(false);

                    onUnHoverAction.Invoke();
                });
            }
        }
        else if (targetState == ButtonState.Normal && previousState != ButtonState.Normal)
        {
            onUnHoverAction.Invoke();
        }
    }

    // --- Pointer Event Handler ---

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (!IsInteractable) { return; }
        _isPointerInside = true;
        SetButtonVisualState(ButtonState.Hover);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (!IsInteractable) { return; }
        _isPointerInside = false;

        // 마우스가 버튼 밖으로 나갔으므로 Normal 상태로 전환 (UnHover 이벤트는 SetButtonVisualState에서 처리)
        SetButtonVisualState(ButtonState.Normal);
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (!IsInteractable) { return; }
        SetButtonVisualState(ButtonState.Pressed);
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (!IsInteractable) { return; }

        if (_isPointerInside)
        {
            // Pressed 상태에서 마우스를 뗐으나, 포인터는 여전히 버튼 위에 있음 -> Hover 상태로 복귀
            SetButtonVisualState(ButtonState.Hover);
        }
        else
        {
            // Pressed 상태에서 마우스를 뗐고, 포인터가 버튼 밖에 있음 -> Normal 상태로 복귀
            SetButtonVisualState(ButtonState.Normal);
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (!IsInteractable) { return; }

        Debug.Log($"UI Clicked: {gameObject.name}");
        onClickAction?.Invoke();
    }
}