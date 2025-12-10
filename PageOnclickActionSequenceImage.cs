using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class PageOnclickActionSequenceImage : PageOnclickActionSequenceBase
{
    private Image image;
    private float duration = 0.2f; // 페이드 시간

    protected override void InitAction()
    {
        image = GetComponent<Image>();
        if (image == null) return;
        var c = image.color;
        c.a = 0f;
        image.color = c;
    }

    async public override UniTask PlaySequence(CancellationToken token)
    {
        await base.PlaySequence(token);
        await FadeIn(token);
        CompleteSequence();
    }

    async UniTask FadeInLegacy(CancellationToken token)
    {
        if (image == null) return;

        float time = 0f;

        try
        {
            while (time < duration)
            {
                token.ThrowIfCancellationRequested();
                time += Time.deltaTime;
                float t = time / duration;

                Color c = image.color;
                c.a = Mathf.Lerp(0f, 1f, t);
                image.color = c;

                await UniTask.Yield(); // 다음 프레임까지 기다리기
            }
        }
        catch (OperationCanceledException)
        {
            // 클릭으로 중단됨
        }
        finally
        {
            // 마지막 프레임에서 완전히 보이도록 정리

        }
    }

    /// <summary>
    /// DOTween + UniTask 기반 페이드인 (곡선 적용, 취소 가능)
    /// </summary>
    public async UniTask FadeIn(CancellationToken token)
    {
        if (image == null) return;

        // 초기 알파값 설정
        Color c = image.color;
        c.a = 0f;
        image.color = c;

        // DOTween 페이드 Tween 생성
        Tween fadeTween = image.DOFade(1f, duration)
                               .SetEase(Ease.OutCubic); // 곡선 적용

        try
        {
            // Tween이 끝나거나 취소될 때까지 대기
            while (fadeTween.IsActive() && !fadeTween.IsComplete())
            {
                token.ThrowIfCancellationRequested();
                await UniTask.Yield(); // 다음 프레임까지 대기
            }
        }
        catch (OperationCanceledException)
        {
            // 클릭으로 중단된 경우 Tween 종료
            fadeTween.Kill(false); // false: 마지막 상태 적용 안함
        }
        finally
        {
            // 마지막 프레임에서 완전히 보이도록 정리
            c = image.color;
            c.a = 1f;
            image.color = c;
        }
    }


    protected override void CompleteAction()
    {

    }

    protected override void CompleteInstantlyAction()
    {

        if (image == null) return;
        Color finalColor = image.color;
        finalColor.a = 1f;
        image.color = finalColor;
    }



    protected override void StandByAction()
    {

    }
}
