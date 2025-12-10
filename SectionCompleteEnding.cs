using System;
using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;
using static System.Collections.Specialized.BitVector32;

public class SectionCompleteEnding : SectionComplete
{


    EndingUI endingUI;

    SectionObj mySection;

    float FadeDuration = 2f;

    void BackSelection()
    {
        CloseComplateUI(OpenLastSectionPage);
    }

    void BackTitle()
    {
        CloseComplateUI(OpenTitle);
    }

    void OpenTitle()
    {
        GameManager.Instance.titleMenu.CallTitleMenu();
    }

    async public override void CloseComplateUI(Action action)
    {
        UIButtonHover.BlockAllUI(true);
        endingUI.backTitleButton.FadeOutCurrentBlocking(FadeDuration);
        endingUI.backSecetionButton.FadeOutCurrentBlocking(FadeDuration);
        await UniTask.Delay(TimeSpan.FromSeconds(FadeDuration));
        gameObject.SetActive(false);
        transform.SetParent(mySection.transform);
        endingUI.UIParent.SetActive(false);
        action?.Invoke();
    }

    void OpenLastSectionPage()
    {
        GameManager.Instance.storyManager.EnterStoryMode(GameManager.Instance.lastSelection);
        UIButtonHover.BlockAllUI(false);
    }

    public override void Init(SectionObj section)
    {
        mySection = section;
        endingUI = GameManager.Instance.endingUI;
        endingUI.backTitleButton.Init(BackTitle);
        endingUI.backSecetionButton.Init(BackSelection);
    }





    public override void Open(Transform parent)
    {
        Debug.Log("엔딩 오픈");
        GameManager.Instance.replayManager.ResetReplay();
        gameObject.SetActive(true);
        endingUI.UIParent.SetActive(true);
        transform.SetParent(parent);
        endingUI.endingTitleUI.gameObject.SetActive(true);
        endingUI.endingTitleUI.FadeIn(2f,Ending);
        GameManager.Instance.storyManager.currentPage.uIGrayscaleTween.ToGrayscale(2f);
    }

    async void Ending()
    {
        await UniTask.Delay(1000); // 1000ms = 1초
        endingUI.endingSelectionUI.gameObject.SetActive(true);
        endingUI.endingBlackScreenUI.FadeIn(1.5f, AppearSelection);
    }

    void AppearSelection()
    {
        endingUI.backTitleButton.FadeInDefaultBlocking(0.3f);
        endingUI.backSecetionButton.FadeInDefaultBlocking(0.3f);
    }
}
