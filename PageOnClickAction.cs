using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Cysharp.Threading.Tasks;
using System.Threading;
using System;
using Unity.VisualScripting.Antlr3.Runtime;
using Unity.VisualScripting;

public class PageOnClickAction : MonoBehaviour
{
    [SerializeField] List<PageOnclickActionSequenceBase> actionSequnce;

    private CancellationTokenSource cts; // 토큰 소스
    public ActionStatus actionStatus;
    bool isInit = false;
    // Start is called before the first frame update
    bool isLocked;

    public void Init() // 초기화
    {
        if (!isInit) // 초기화 되어있지 않다면
        {
            isInit = true;
            actionSequnce = PharseSQ(actionSequnce,transform);
        }
        Reset(); // 리셋한다 
    }
    private void Reset()
    {
        actionStatus = ActionStatus.Init; // 상태를 초기화됨 으로 만든다
        isLocked = false; // 잠금을 푼다

        foreach (var obj in actionSequnce)
        {
            obj.Init(); // 하위 액션 시퀀스들을 초기화 한다
        }
    }
    public static List<PageOnclickActionSequenceBase> PharseSQ(List<PageOnclickActionSequenceBase> lists , Transform _transform)
    {
        if (lists == null) // 액션시퀀스 목록이 없다면
        {
            lists = new List<PageOnclickActionSequenceBase>(); // 액션 시퀀스 목록 생성

            foreach (Transform child in _transform) //하위 오브젝트들을 검색하여 
            {
                PageOnclickActionSequenceBase action = AddActionSquence(child);
                lists.Add(action); // 액션 시퀀스를 시퀀스 목록에 추가한다
            }
        }
        return lists;
    }

    public static PageOnclickActionSequenceBase AddActionSquence(Transform transform)
    {
        PageOnclickActionSequenceBase action = transform.GetComponent<PageOnclickActionSequenceBase>(); //하위 오브젝트가 가지고 있는 액션 시퀀스를 얻으려고 시도 
        if (action == null)
        {
            action = PharseAction(transform); // 없다면 액션시퀀스를 만들어 넣는다
        }
        return action;
    }

    public static PageOnclickActionSequenceBase PharseAction(Transform transform) // 오브젝트의 타입에 따라 액션시퀀스의 종류를 분류하여 컴포넌트를 붙인다
    {

        PageOnclickActionSequenceBase actionSequence = transform.GetComponent<PageOnclickActionSequenceBase>();
        if (transform.TryGetComponent<Image>(out var a))
        {
            transform.gameObject.AddComponent<PageOnclickActionSequenceImage>();
            actionSequence = transform.GetComponent<PageOnclickActionSequenceBase>();
        }
        else if (transform.TryGetComponent<Text>(out var b))
        {
            transform.gameObject.AddComponent<PageOnclickActionSequenceText>();
            actionSequence = transform.GetComponent<PageOnclickActionSequenceBase>();
        }
        else if (transform.TryGetComponent<StoryAnimation>(out var c))
        {
            transform.gameObject.AddComponent<PageOnclickActionSequenceAnimation>();
            actionSequence = transform.GetComponent<PageOnclickActionSequenceBase>();
        }
        else if (transform.name.StartsWith("RT"))
        {
            transform.gameObject.AddComponent<PageOnclickActionSequenceRT>();
            actionSequence = transform.GetComponent<PageOnclickActionSequenceBase>();
        }
        else if (transform.name.StartsWith("L"))
            {
            transform.gameObject.AddComponent<PageOnclickActionSequenceL>();
            actionSequence = transform.GetComponent<PageOnclickActionSequenceBase>();
        }
        else
        {
            Debug.LogError("규칙에 맞지 않게 제작된 액션: " + transform.name);
            transform.gameObject.AddComponent<PageOnclickActionSequenceBase>();
            actionSequence = transform.GetComponent<PageOnclickActionSequenceBase>();
        }
        return actionSequence;
    }




    public async void OnClick() // 클릭 판정을 받는다
    {
        switch (actionStatus)
        {
            case ActionStatus.Init: // 초기화 상태라면
                actionStatus = ActionStatus.Playing; //액션을 재생중 상태로 바꾼다
                cts = new CancellationTokenSource(); // 캔슬레이션 토큰 생성
                try
                {
                    await Play(cts.Token); // 액션을 재생
                }
                catch (OperationCanceledException)
                {
                }
                break;
            case ActionStatus.Playing://재생중이라면
                actionStatus = ActionStatus.Locked; // 잠근다
                Candetoken(); // 토큰에 취소명령을 내린다
                break;
            case ActionStatus.Locked://잠겨있으면 아무것도 안한다
                return;
            case ActionStatus.Standby:
                break;
            case ActionStatus.complate:
                break;
        }
    }

    public async UniTask Play(CancellationToken token)
    {
        try
        {
            for (int i = 0; i < actionSequnce.Count; i++)
            {
                // 클릭으로 취소되면 여기서 예외 발생
                token.ThrowIfCancellationRequested();

                await actionSequnce[i].PlaySequence(token).AttachExternalCancellation(token);

                if (i == actionSequnce.Count - 1)
                {
                    actionSequnce[i].StandBy();
                }
            }
        }
        catch (OperationCanceledException)
        {
            // ⭐ 중단 순간 실행할 메서드
            CompleteInstantly();
            Blink();
        }
        finally
        {
            actionStatus = ActionStatus.Standby;
        }

    }

    public void Candetoken() //토큰 취소명령 
    {
        cts?.Cancel(); // Play()에서 AttachExternalCancellation으로 전달한 토큰 취소
    }

    public void CompleteInstantly() // 즉시 액션시퀀스들을 완료 상태로 만든다
    {
        for (int i = 0; i < actionSequnce.Count; i++)
        {
            actionSequnce[i].CompleteInstantly();
        }
    }

    void Blink() // 마지막 액션을 커서가 깜빡이는 상태로 만든다
    {
        actionSequnce[actionSequnce.Count- 1].StandBy();
    }

    public void Complete()
    {
        actionStatus = ActionStatus.complate;
        PageOnclickActionSequenceBase lastOnclickActionSequence = actionSequnce[actionSequnce.Count - 1];
        lastOnclickActionSequence.CompleteSequence();
    }

}


public enum ActionStatus
{
    Init,
    Playing,
    Locked,
    Standby,
    complate
}