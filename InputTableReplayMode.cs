using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InputTableReplayMode : InputTable
{
    public override void OnBackspacePressed()
    {
        GameManager.Instance.replayManager.PrevReplay();
    }

    public override void OnCloseBtnCLick()
    {
        GameManager.Instance.manuUI.ExitUI();
    }

    public override void OnEnterPressed()
    {
        GameManager.Instance.replayManager.NextReplay();
    }

    public override void OnEscPressed()
    {
        GameManager.Instance.manuUI.ExitUI();
    }

    public override void OnHideBtnCLick()
    {
        GameManager.Instance.manuUI.ToggleHide();
    }

    public override void OnNextBtnCLick()
    {
        GameManager.Instance.replayManager.NextReplay();
    }

    public override void OnPrevBtnCLick()
    {
        GameManager.Instance.replayManager.PrevReplay();
    }

    public override void OnRightClick()
    {
        GameManager.Instance.manuUI.ExitUI();
    }

    public override void OnScreenClick()
    {
        GameManager.Instance.manuUI.ToggleHide();
    }

    public override void OnSpacePressed()
    {
        GameManager.Instance.manuUI.ToggleHide();
    }
}
