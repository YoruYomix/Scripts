using System.Collections;
using System.Collections.Generic;
using System.Threading;
using Animancer.Examples.StateMachines;
using Cysharp.Threading.Tasks;
using Unity.VisualScripting;
using Unity.VisualScripting.Antlr3.Runtime;
using UnityEngine;
using UnityEngine.UI;


public class PageOnclickActionSequenceRT : PageOnclickActionSequenceBase
{
    List<PageOnclickActionSequenceBase> actionSequnce;

    bool isInit = false;

    async public override UniTask PlaySequence(CancellationToken token)
    {
        await base.PlaySequence(token);

        // 모든 PlaySequence를 동시에 실행하고 UniTask 배열에 저장
        UniTask[] tasks = new UniTask[actionSequnce.Count];
        for (int i = 0; i < actionSequnce.Count; i++)
        {
            tasks[i] = actionSequnce[i].PlaySequence(token);
        }

        // 모든 시퀀스가 완료될 때까지 기다림
        await UniTask.WhenAll(tasks);

        CompleteSequence();
    }

    protected override void CompleteAction()
    {
        for (int i = 0; i < actionSequnce.Count; i++)
        {
            actionSequnce[i].CompleteSequence();
        }
    }

    protected override void CompleteInstantlyAction()
    {
        for (int i = 0; i < actionSequnce.Count; i++)
        {
            actionSequnce[i].CompleteInstantly();
        }
    }

    protected override void InitAction()
    {
        if (!isInit)
        {
            isInit = true;
            actionSequnce = PageOnClickAction.PharseSQ(actionSequnce, transform);
        }
        Reset(); // 리셋한다 
    }



    private void Reset()
    {
        foreach (var obj in actionSequnce)
        {
            obj.Init(); // 하위 액션 시퀀스들을 초기화 한다
        }
    }

    protected override void StandByAction()
    {
        for (int i = 0; i < actionSequnce.Count; i++)
        {
            actionSequnce[i].StandBy();
        }
    }
}
