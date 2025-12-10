using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InputTableManuOpen : InputTable
{
    // 마우스 입력
    public override void OnBackspacePressed()
    {
        GameManager.Instance.replayManager.PrevReplay();
    }

    public override void OnRightClick()
    {
        GameManager.Instance.manuUI.ExitUI();
    }


    // 키보드 입력
    public override void OnEnterPressed()
    {
        GameManager.Instance.manuUI.ExitUI();
    }

    public override void OnEscPressed()
    {
        GameManager.Instance.manuUI.ExitUI();
    }

    public override void OnScreenClick()
    {
        GameManager.Instance.manuUI.ExitUI();
    }

    public override void OnSpacePressed()
    {
        GameManager.Instance.manuUI.ExitUI();
    }

    // 버튼 입력
    public override void OnCloseBtnCLick()
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
}
