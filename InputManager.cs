using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class InputManager : MonoBehaviour
{
    public InputTable inputTable;
    public InputTableStorymode inputTableStorymode;
    public InputTableManuOpen inputTableManuOpen;
    public InputTableReplayMode inputTableReplayMode;
    public InputTableLockUI inputTableLockUI;
    public int lockInput;
    public Action rightCilck;
    [SerializeField] ImageClickHandler imageClickHandler;

    public void Init()
    {
        lockInput = 0;
        inputTableStorymode = FindAnyObjectByType<InputTableStorymode>();
        inputTableManuOpen = FindAnyObjectByType<InputTableManuOpen>();
        inputTableReplayMode = FindAnyObjectByType<InputTableReplayMode>();
        inputTableLockUI = FindAnyObjectByType<InputTableLockUI>();
        imageClickHandler.Init();
    }

    public void Update()
    {
        if (lockInput > 0) return;

        if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter)) OnEnterPressed();
        if (Input.GetKeyDown(KeyCode.Space)) OnSpacePressed();
        if (Input.GetKeyDown(KeyCode.Backspace)) OnBackspacePressed();
        if (Input.GetKeyDown(KeyCode.Escape)) OnEscPressed();
    }

     void OnEnterPressed()
    {
        inputTable.OnEnterPressed();
    }

    void OnSpacePressed() 
    {
        inputTable.OnSpacePressed();
    }

    void OnBackspacePressed()
    {
        inputTable.OnBackspacePressed();
    }

    void OnEscPressed()
    {
        inputTable.OnEscPressed();
    }

    public void ScreenClickAction()
    {
        Debug.Log(inputTable);
        inputTable.OnScreenClick();
    }

    public void RightCilckAction()
    {
        inputTable.OnRightClick();
    }

}
