
using System.Collections.Generic;

using UnityEngine;
using System.Linq;


public class Page : MonoBehaviour
{
    public int pageIndex;
    public SectionObj mySection;
    public Page nextPage;
    public Page prevPage;
    List<PageOnClickAction> onClickActions;
    private int currentActionIndex;  // 현재 실행할 인덱스
    PageOnClickAction currentAction;
    public bool isFirstPage;
    public bool isLastPage;
    public UIGrayscaleTween uIGrayscaleTween;




    public PageStatus pageStatus
    {
        
        get
        {
            if (currentActionIndex + 1 >= onClickActions.Count && currentAction.actionStatus == ActionStatus.Standby)// 액션의 인덱스가 페이지의 마지막이고, 마지막 액션이 스탠바이 상태라면
            {
                return PageStatus.complate; // 페이지의 상태는 완료됨
            }
            else if (currentActionIndex == 0 && onClickActions[currentActionIndex].actionStatus == ActionStatus.Init) // 액션의 인덱스가 페이지의 처음이고, 액션이 이니셜 상태라면
            {
                return PageStatus.init; // 페이지의 상태는 열림
            }
            else
            {
                return PageStatus.playing; // 나머지라면 페이지는 현재 재생 중
            }
        }
    }

    public void SetReplay(Transform parent)
    {
        ComplateInstantlyPage();
        transform.SetParent(parent);
        gameObject.SetActive(true);
    }
    
    public void PlayStart(Transform parent)
    {
        transform.SetParent(parent);
        gameObject.SetActive(true);
        OnClick();
    }

    public void ComplateInstantlyPage()
    {
        foreach (var action in onClickActions)
        {
            action.CompleteInstantly();
        }
    }

    public void StandbyToComplatePage()
    {
        foreach (var action in onClickActions)
        {
            action.Complete();
        }
    }

    public void OnClick() // 페이지에 클릭 판정
    {

        currentAction = onClickActions[currentActionIndex];// 현재 재생할 액션을 액션 인덱스에 따라 셋팅

        switch (currentAction.actionStatus)
        {
            case ActionStatus.Init: // 액션이 이니셜 상태라면
                currentAction.OnClick(); // 액션에 클릭판정
                break;
            case ActionStatus.Playing: // 액션이 플레이 상태라면
                currentAction.OnClick(); // 액션에 클릭판정
                break;
            case ActionStatus.Standby: // 액션이 스탠바이 상태라면
                currentAction.Complete(); // 액션을 완료함 (최종적인 상태로 만듬)
                currentActionIndex++; // 액션 인덱스를 하나 올림
                GameManager.Instance.storyManager.PlayStoryMode();
                break;
            case ActionStatus.Locked:
                break;
            case ActionStatus.complate:
                break;
        }
    }

    public void Return()
    {
        gameObject.transform.SetParent(mySection.transform);
        Refresh();
    }

    bool isInit = false;
    public void Init(int _pageIndex, SectionObj _section,Page _prevPage,Page _nextPage)
    {
        if (!isInit)
        {
            isInit = true;
            prevPage = _prevPage;
            nextPage = _nextPage;
            pageIndex = _pageIndex;
            mySection = _section;
            if (nextPage == null)
            {
                isLastPage = true;
            }
            if (prevPage == null)
            {
                isFirstPage = true;
            }
            onClickActions = new List<PageOnClickAction>();
            foreach (Transform child in transform)
            {
                if (child.TryGetComponent<PageOnclickActionSequenceBase>(out var rb))
                {
                    continue;
                }

                PageOnClickAction step = child.GetComponent<PageOnClickAction>();

                if (step == null)
                {
                    child.gameObject.AddComponent<PageOnClickAction>();
                    step = child.GetComponent<PageOnClickAction>();
                }
                onClickActions.Add(step);
            }
            if (IsNumberedAction(onClickActions[0].gameObject.name))
            {
                onClickActions = onClickActions.OrderBy(c => ExtractNumber(c.gameObject.name)).ToList();
            }
            uIGrayscaleTween = gameObject.AddComponent<UIGrayscaleTween>();
            uIGrayscaleTween.Initialize();
        }
        Refresh();
    }

    bool IsNumberedAction(string name)
    {
        string numStr = new string(name.Where(char.IsDigit).ToArray());
        if (int.TryParse(numStr, out int number))
            return true;
        return false;
    }

    int ExtractNumber(string name)
    {
        string numStr = new string(name.Where(char.IsDigit).ToArray());
        if (int.TryParse(numStr, out int number))
            return number-1;
        return 0; // 숫자가 없으면 0으로 처리
    }

    void Refresh()
    {
        currentActionIndex = 0;  // 현재 실행할 인덱스
        currentAction = onClickActions[currentActionIndex];
        foreach (var action in onClickActions)
        {
            action.Init(); // 모든 액션을 이니셜 상태로 만듬
        }
        uIGrayscaleTween.RestoreOriginal(); // 만약 흑백이라면 컬러로 돌림
        gameObject.SetActive(false);
    }







    public bool IsOutOfActionIndex
    {
        get
        {
            return (currentActionIndex == onClickActions.Count);
        }
    }


}

public enum PageStatus
{
    init,
    playing,
    complate
}