using System.Collections;
using System.Collections.Generic;
using Animancer;
using UnityEngine;
using UnityEngine.SceneManagement;

public class StoryManager : MonoBehaviour
{

    public Page currentPage;

    
    public void ReturnPage()
    {
        if (currentPage != null)
        {
            currentPage.Return();
        }
    }

    public void EnterStoryMode(int _section,int _page)
    {
        if (StoryUtility.IsSceneLoaded(_section))
        {
            // 해당 섹션이 있는 씬을 로드한다
        }

        foreach (var sectionObj in GameManager.Instance.chapter.sectionObjs)
        {
            if (sectionObj.section.Index != _section)
            {
                continue;
            }
            Page page = sectionObj.pages[_page];
            EnterStoryMode(page);
        }
        
    }

    public void EnterStoryMode(Page _page)
    {
        ReturnPage();
        GameManager.Instance.GameModeLegacy = GameModeLegacy.Story;
        currentPage = _page;
        PlayStoryMode();
    }

    public void PlayStoryMode()
    {
        if (GameManager.Instance.GameModeLegacy != GameModeLegacy.Story)
        {
            return;
        }
        if (currentPage == null)
        {
            return;
        }
        switch (currentPage.pageStatus)
        {
            case PageStatus.init: // 페이지가 방금 열린 상태라면
                currentPage.PlayStart(GameManager.Instance.sceneInfomation.currentPageParent.transform);
                break;
            case PageStatus.playing: // 페이지가 재생중이라면
                currentPage.OnClick(); // 페이지에 클릭판정
                break;
            case PageStatus.complate: // 페이지가 완료되었다면
                if (currentPage.isLastPage) // 마지막 페이지라면
                {
                    currentPage.StandbyToComplatePage(); // 스탠바이(커서 깜빡임)상태의 페이지를 완료상태로 변경
                    CallComplateUI();//섹션 완료 페이지 열기
                }
                else
                {
                    OpenNextPage(); // 다음 페이지 열기
                }

                break;
        }
    }

    void OpenNextPage()
    {
        Page nextPage = currentPage.nextPage;
        ReturnPage();
        GameManager.Instance.replayManager.rePlays.Add(currentPage);
        currentPage = nextPage;
        PlayStoryMode();
    }

    void CallComplateUI()
    {
        GameManager.Instance.replayManager.rePlays.Add(currentPage);
        GameManager.Instance.GameModeLegacy = GameModeLegacy.LockUI;
        currentPage.mySection.SectionComplate(GameManager.Instance.sceneInfomation.sectionComplateUIParent.transform);
    }
}
