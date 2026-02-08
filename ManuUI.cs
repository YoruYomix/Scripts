using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cysharp.Threading.Tasks;
using System;
using System.ComponentModel;

public class ManuUI : MonoBehaviour
{
    StoryAnimation storyAnimation;
    public UIButtonHover prevReplayBtn;
    public UIButtonHover nextReplayBtn;
    public UIButtonHover hideUIBtn;
    public UIButtonHover closeUIBtn;
    public GameObject replayIcon;
    public GameObject prevReplayBtnParent;
    public GameObject nextReplayBtnParent;

    bool isAnimationNow
    {
        get
        {
            return isAni;
        }
        set
        {
            isAni = value;
            Debug.Log("isAni 변경: "+isAni);
        }
    }


    bool isAni;
    bool isHide;

    public void Init()
    {
        replayIcon.SetActive(false);
        isAnimationNow = false;
        storyAnimation = GetComponent<StoryAnimation>();
        storyAnimation.Initialize();
        gameObject.SetActive(false);
        prevReplayBtn.Init(OnPrevBtnCLick);
        prevReplayBtn.ShowInstant();
        nextReplayBtn.Init(OnNextBtnCLick);
        nextReplayBtn.ShowInstant();
        hideUIBtn.Init(OnHideBtnCLick);
        hideUIBtn.ShowInstant();
        closeUIBtn.Init(OnCloseBtnCLick);
        closeUIBtn.ShowInstant();
        OpenModeUI();
        isHide = false;
        GameManager.Instance.replayManager.EnterReplayModeAction += ReplayModeUI;
        GameManager.Instance.replayManager.ExitReplayModeAction += OpenModeUI;
        GameManager.Instance.replayManager.checkPrevBtn += CheckHidePrevBtn;
        GameManager.Instance.replayManager.checkNextBtn += CheckHideNextBtn;
        DisAppearUIAsync();
    }

    void OnPrevBtnCLick()
    {
        if (GameManager.Instance.inputManager.lockInput > 0) return;
        GameManager.Instance.inputManager.inputTable.OnPrevBtnCLick();
    }
    void OnNextBtnCLick()
    {
        if (GameManager.Instance.inputManager.lockInput > 0) return;
        GameManager.Instance.inputManager.inputTable.OnNextBtnCLick();
    }
    void OnHideBtnCLick()
    {
        if (GameManager.Instance.inputManager.lockInput > 0) return;
        GameManager.Instance.inputManager.inputTable.OnHideBtnCLick();
    }
    void OnCloseBtnCLick()
    {
        if (GameManager.Instance.inputManager.lockInput > 0) return;
        GameManager.Instance.inputManager.inputTable.OnCloseBtnCLick();
    }

    void CheckHidePrevBtn(bool isActive) // 회상모드 화살표키를 숨길지 표시할지 검사 
    {
        if (prevReplayBtnParent.activeSelf == isActive)
        {
            return;
        }

        prevReplayBtnParent.SetActive(isActive);
        if (isActive)
        {
            prevReplayBtn.ShowInstant();
        }

    }

    void CheckHideNextBtn(bool isActive) // 회상모드 화살표키를 숨길지 표시할지 검사 
    {
        if (nextReplayBtnParent.activeSelf == isActive)
        {
            return;
        }
        nextReplayBtnParent.SetActive(isActive);
        if (isActive)
        {
            nextReplayBtn.ShowInstant();
        }
    }

    void ReplayModeUI() // 회상모드로 진입함
    {
        hideUIBtn.gameObject.SetActive(true);
        replayIcon.gameObject.SetActive(true);
    }

    void OpenModeUI() // 회상모드에서 나감
    {
        hideUIBtn.gameObject.SetActive(false);
        replayIcon.gameObject.SetActive(false);
        // nextReplayBtn.SetDefault();
    }




    public void ToggleHide()
    {
        if (isHide)
        {
            AppearUI();
        }
        else
        {
            DisAppearUIAsync();
        }
    }




    async public UniTask OpenUI()
    {
        if (isAnimationNow) return;
        await AppearUI();
        GameManager.Instance.GameMode = GameMode.ManuOpen;
    }

    async public UniTask ExitUI()
    {
        if (isAnimationNow) return;
        if (GameManager.Instance.isReplayMode)
        {
            Debug.Log("리플레이 모드 종료");
            GameManager.Instance.replayManager.ExitReplayMode();
            
        }
        Debug.Log("유아이 비활성화");
        await DisAppearUIAsync();
        Debug.Log("유아이 비활성화 완료");
        GameManager.Instance.GameMode = GameMode.Story;
    }


    async UniTask AppearUI()
    {
        Debug.Log(isAnimationNow);
        // 이미 애니메이션 중이면 중복 실행 방지
        if (isAnimationNow) return;
        isAnimationNow = true;

        CheckHidePrevBtn(GameManager.Instance.replayManager.CheckPrevButton());
        CheckHideNextBtn(GameManager.Instance.replayManager.CheckNextButton());
        isHide = false;
        gameObject.SetActive(true);
        GameManager.Instance.inputManager.lockInput++;//매뉴 유아이가 나타나는 동안 인풋락
        Debug.Log("인풋락 추가: " + GameManager.Instance.inputManager.lockInput);
        try
        {
            Debug.Log("애니메이션 시작: ManuUIappear");
            // 안전하게 애니메이션 재생
            await storyAnimation.PlayAnimationAsync("ManuUIAppear");
            Debug.Log("애니메이션 끝: ManuUIappear");
        }
        catch (Exception e)
        {
            Debug.LogError("애니메이션 중 오류: " + e);
        }
        finally
        {
            // 반드시 lock 감소
            GameManager.Instance.inputManager.lockInput--;
            Debug.Log("인풋락 감소: " + GameManager.Instance.inputManager.lockInput);
            isAnimationNow = false;
        }
    }


    async UniTask DisAppearUIAsync()
    {
        Debug.Log(isAnimationNow);
        // 이미 애니메이션 중이면 중복 실행 방지
        if (isAnimationNow) return;
        isAnimationNow = true;
        CheckHidePrevBtn(GameManager.Instance.replayManager.CheckPrevButton());
        CheckHideNextBtn(GameManager.Instance.replayManager.CheckNextButton());
        // 인풋락 증가
        GameManager.Instance.inputManager.lockInput++;
        Debug.Log("인풋락 추가: " + GameManager.Instance.inputManager.lockInput);
        isHide = true;
        try
        {
            Debug.Log("애니메이션 시작: ManuUIDisappear");
            // 안전하게 애니메이션 재생
            await storyAnimation.PlayAnimationAsync("ManuUIDisappear");
            Debug.Log("애니메이션 끝: ManuUIDisappear");

            // 유아이 비활성화
            gameObject.SetActive(false);
        }
        catch (Exception e)
        {
            Debug.LogError("애니메이션 중 오류: " + e);
        }
        finally
        {
            // 반드시 lock 감소
            GameManager.Instance.inputManager.lockInput--;
            Debug.Log("인풋락 감소: " + GameManager.Instance.inputManager.lockInput);
            isAnimationNow = false;
        }
        Debug.Log(isAnimationNow);
    }
}


public enum ManuUIStatus
{
    off,
    on,
    replay
}