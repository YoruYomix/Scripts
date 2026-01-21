using System;
using System.Collections;
using System.Collections.Generic;
using Animancer;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ReplayManager : MonoBehaviour
{
    public Replay replay; // ���÷��̿� �� ���� ����, ���÷��̿��� ������ �ı�
    public List<Page> rePlays;
    Page nowReplayPage;
    Transform replayParent;
    public Action<bool> checkPrevBtn;
    public Action<bool> checkNextBtn;

    public Action EnterReplayModeAction;
    public Action ExitReplayModeAction;


    public void Init(Transform _transform)
    {
        rePlays = new List<Page>();
        replay = null;
        replayParent = _transform; 
    }

    public void EnterReplayMode()
    {
        GameManager.Instance.GameMode = GameMode.Replay;
        replay = new Replay(rePlays);
        GameManager.Instance.sceneInfomation.replayUI.SetActive(true);
        EnterReplayModeAction?.Invoke();
    }

    public void ResetReplay()
    {
        rePlays = null;
        rePlays = new List<Page>();
    }

    public void ExitReplayMode()
    {
        if (nowReplayPage != null)
        {
            nowReplayPage.Return();
            nowReplayPage = null;
        }
        replay = null;
        ExitReplayModeAction?.Invoke();
        GameManager.Instance.sceneInfomation.replayUI.SetActive(false);
        GameManager.Instance.GameMode = GameMode.Story;
    }
    public void PrevReplay()
    {
        if (replay == null)
        {
            Debug.Log("EnterReplayMode: " + rePlays.Count);
            EnterReplayMode();
            SetReplay(replay.PrevReplay());
            return;
        }
        if (replay.IsLastPage)
        {
            return;
        }
        SetReplay(replay.PrevReplay());
    }

    public void NextReplay()
    {
        Page nextPage = replay.NextReplay();
        if (nextPage == null)
        {
            ExitReplayMode();
            GameManager.Instance.manuUI.ExitUI();
            return;
        }
        SetReplay(nextPage);
    }
    public bool CheckNextButton()
    {

        if (!GameManager.Instance.isReplayMode)
        {
            return false;
        }

        return true;

    }


    public bool CheckPrevButton()
    {
        if (!GameManager.Instance.isReplayMode)
        {
            if (rePlays.Count < 1)
            {
                return false;
            }
            else
            {
                return true;
            }
        }
        if (replay.IsLastPage)
        {
            return false;
        }
        return true;
    }

    public void SetReplay(Page page)
    {
        if (nowReplayPage != null)
        {
            nowReplayPage.Return();
        }

        nowReplayPage = page;
        page.SetReplay(replayParent);
        checkPrevBtn?.Invoke(CheckPrevButton());
        checkNextBtn?.Invoke(CheckNextButton());
    }
}
