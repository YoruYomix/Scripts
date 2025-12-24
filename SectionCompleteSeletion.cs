using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;
using System;
using DG.Tweening;
using static System.Collections.Specialized.BitVector32;

public class SectionCompleteSeletion : SectionComplete
{
    [SerializeField] PageSelectBtn[] PageSelectBtns;
    SectionObj mySection;
    float FadeDuration = 2f;

    async public override void CloseComplateUI(Action onComplete)
    {
        UIButtonHover.BlockAllUI(true);

        foreach (var btn in PageSelectBtns)
        {
            btn.ButtonHover.FadeOutCurrentBlocking(FadeDuration);
        }
        await UniTask.Delay(TimeSpan.FromSeconds(FadeDuration));
        gameObject.SetActive(false);
        transform.SetParent(mySection.transform);
        onComplete?.Invoke();
    }

    public override void Init(SectionObj section)
    {
        mySection = section;
        gameObject.SetActive(false);
        for (int i = 0; i < PageSelectBtns.Length; i++)
        {
            PageSelectBtns[i].Init(this);
        }
    }

    public override void Open(Transform parent)
    {
        GameManager.Instance.GameModeLegacy = GameModeLegacy.LockUI;
        Debug.Log("섹견완료, 선택지 오픈");
        GameManager.Instance.lastSelection=GameManager.Instance.storyManager.currentPage;
        gameObject.SetActive(true);
        transform.SetParent(parent);
        foreach (var item in PageSelectBtns)
        {
            item.ButtonHover.FadeInDefaultBlocking(2f);
        }
    }

}
