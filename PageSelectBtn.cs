using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PageSelectBtn : MonoBehaviour
{
    public SectionObj section;
    public UIButtonHover ButtonHover;
    SectionComplete sectionComplete;

    public void Init(SectionComplete _sectionComplete)
    {
        sectionComplete = _sectionComplete;
        ButtonHover = GetComponent<UIButtonHover>();
        ButtonHover.Init(SelectedAction);
    }

    void SelectedAction()
    {
        sectionComplete.CloseComplateUI(GotoTargetPage);
    }

    void GotoTargetPage()
    {
        GameManager.Instance.storyManager.EnterStoryMode(section.FirstPage);
    }
}
