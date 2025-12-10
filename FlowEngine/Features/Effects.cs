using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using static UnityEngine.GraphicsBuffer;

public class ActivationEffect : IFlowNodeEffect
{
    public float GetRequiredDuration() => 0f;
    public readonly GameObject _target;

    public GameObject GameObject => _target;

    public ActivationEffect(GameObject target)
    {
        _target = target;
    }

    public void Play()
    {
        _target.SetActive(true);
    }

    public void Finish(){}
}


public class FadeInEffect : IFlowNodeEffect
{
    public float GetRequiredDuration() => _fadeDuration;
    private readonly Image _image;
    private readonly Color _originalColor;
    private Tween _tween;
    private float _fadeDuration = 2f;
    public GameObject GameObject
    {
        get
        {
            return _image.gameObject;
        }
    }

    public FadeInEffect(Image image)
    {
        _image = image;
        _originalColor = image.color;
    }

    public void Play()
    {
        // 알파 0으로 초기화
        Color c = _originalColor;
        c.a = 0f;
        _image.color = c;

        // 완료 시 자동으로 원본 색상으로 도달함
        _tween = _image.DOFade(_originalColor.a, _fadeDuration);
    }

    public void Finish()
    {
        // 스킵 → Kill(true) 로 바로 최종 값으로 보정됨
        if (_tween != null && _tween.IsActive())
            _tween.Kill(true);  // true = "완료 상태로 처리"
    }

}
