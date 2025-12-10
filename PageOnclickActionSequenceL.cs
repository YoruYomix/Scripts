using System;
using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using System.Threading;
using UnityEngine;
using Unity.VisualScripting.Antlr3.Runtime;

public class PageOnclickActionSequenceL : PageOnclickActionSequenceBase
{
    Dictionary<Language,PageOnclickActionSequenceBase> actionSequenceLanguages;
    Dictionary<Language, GameObject> languageParents;
    bool isInit = false;


    public void SwapLanguage(Language _language)
    {
        foreach (var kvp in languageParents)
        {
            kvp.Value.SetActive(kvp.Key == _language);
        }
    }

    async public override UniTask PlaySequence(CancellationToken token)
    {
        await base.PlaySequence(token);

        List<UniTask> tasks = new List<UniTask>();

        foreach (var kvp in actionSequenceLanguages)
        {
            if (kvp.Value == null)
            {
                return;
            }
            tasks.Add(kvp.Value.PlaySequence(token));
        }
        // 모든 Task가 완료될 때까지 대기
        await UniTask.WhenAll(tasks);

        CompleteSequence();
    }

    protected override void CompleteAction()
    {
        foreach (var kvp in actionSequenceLanguages)
        {
            if (kvp.Value == null)
            {
                continue;
            }
            kvp.Value.CompleteSequence();
        }
    }

    protected override void CompleteInstantlyAction()
    {
        foreach (var kvp in actionSequenceLanguages)
        {
            if (kvp.Value == null)
            {
                continue;
            }
            kvp.Value.CompleteInstantly();
        }
    }

    protected override void InitAction()
    {
        if (!isInit)
        {
            isInit = true;
            languageParents = new Dictionary<Language, GameObject>();
            actionSequenceLanguages = new Dictionary<Language, PageOnclickActionSequenceBase>();
            // 1단계 자식 개수 가져오기
            int childCount = transform.childCount;

            // 배열에 자식들 넣기
            for (int i = 0; i < childCount; i++)
            {
                GameObject child = transform.GetChild(i).gameObject;
                Language language = ConvertNameToEnum(child);
                languageParents.Add(language, child);


                if (child.transform.childCount < 1)
                {
                    actionSequenceLanguages.Add(language, null);
                    continue;
                }

                GameObject childLV2 = child.transform.GetChild(0).gameObject;
                PageOnclickActionSequenceBase onclickActionSequenceBase = PageOnClickAction.AddActionSquence(childLV2.transform);
                actionSequenceLanguages.Add(language, onclickActionSequenceBase);

            }
            SwapLanguage(GameManager.Instance.CurrntLangage);
            GameManager.Instance.languageSwapEvent += SwapLanguage;
        }
        Reset(); // 리셋한다 
    }



    private void Reset()
    {
        foreach (var kvp in actionSequenceLanguages)
        {
            if (kvp.Value == null)
            {
                continue;
            }
            kvp.Value.Init(); // 하위 액션 시퀀스들을 초기화 한다
        }
    }

    protected override void StandByAction()
    {
        foreach (var kvp in actionSequenceLanguages)
        {
            if (kvp.Value == null)
            {
                continue;
            }
            kvp.Value.StandBy(); // 하위 액션 시퀀스들을 초기화 한다
        }
    }

    public Language ConvertNameToEnum(GameObject go)
    {
        // GameObject 이름 가져오기
        string name = go.name;

        // 문자열 → enum 변환
        if (Enum.TryParse<Language>(name, out Language result))
        {
            return result;
        }
        else
        {
            Debug.LogWarning($"GameObject 이름 '{name}'이(가) enum에 존재하지 않습니다.");
            return default; // 기본값 반환
        }
    }
}

public enum Language
{
    KR,
    JP
}