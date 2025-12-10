using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InputTableStorymode : InputTable
{


    public override void OnScreenClick()
    {
        GameManager.Instance.storyManager.PlayStoryMode();
    }
    public override void OnRightClick()
    {
        GameManager.Instance.manuUI.OpenUI();
    }
    public override void OnBackspacePressed()
    {
        GameManager.Instance.manuUI.OpenUI();
    }

    public override void OnCloseBtnCLick()
    {
        GameManager.Instance.manuUI.ExitUI();
    }

    public override void OnEnterPressed()
    {
        GameManager.Instance.storyManager.PlayStoryMode();
    }

    public override void OnEscPressed()
    {
        GameManager.Instance.manuUI.OpenUI();
    }

    public override void OnHideBtnCLick()
    {
        return;
    }

    public override void OnNextBtnCLick()
    {
        return;
    }

    public override void OnPrevBtnCLick()
    {
        return;
    }



    public override void OnSpacePressed()
    {
        GameManager.Instance.manuUI.OpenUI();
    }
}
