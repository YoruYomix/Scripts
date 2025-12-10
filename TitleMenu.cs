using Cysharp.Threading.Tasks;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TitleMenu : MonoBehaviour
{
    [SerializeField] UIButtonHover newGameBtn;
    [SerializeField] UIButtonHover continueBtn;
    [SerializeField] UIButtonHover exitBtn;
    [SerializeField] UIFader whiteFade;
    [SerializeField] Image titleImage;
    [SerializeField] GameObject btnParent;

    public void Init()
    {
        newGameBtn.Init(NewGame);
        continueBtn.Init(ContinueGame);
        exitBtn.Init(ExitGame);
        newGameBtn.ShowInstant();
        continueBtn.ShowInstant();
        exitBtn.ShowInstant();
        whiteFade.Init();
        OnBtns();
        titleImage.gameObject.SetActive(false);
        btnParent.gameObject.SetActive(false);
        gameObject.SetActive(false);
    }

    void NewGame()
    {
        ExitTItleMenu(() => 
        {
            GameManager.Instance.storyManager.EnterStoryMode(1,0);    
        });
    }
    void ContinueGame()
    {

    }
    void ExitGame()
    {

    }
    public void CallTitleMenu()
    {
        GameManager.Instance.replayManager.ResetReplay();
        gameObject.SetActive(true);
        btnParent.gameObject.SetActive(false);
        whiteFade.gameObject.SetActive(true);
        whiteFade.FadeIn(0.5f, () => {
            btnParent.gameObject.SetActive(true);
            titleImage.gameObject.SetActive(true);
            GameManager.Instance.storyManager.ReturnPage();
            whiteFade.FadeOut(0.5f, CloseFade); 
        });
    }

    public void ExitTItleMenu(Action action)
    {
        whiteFade.gameObject.SetActive(true);
        whiteFade.FadeIn(0.5f, async () => {
            await UniTask.Delay(1000);
            btnParent.gameObject.SetActive(false);
            titleImage.gameObject.SetActive(false);
            action?.Invoke();

            whiteFade.FadeOut(0.5f, () => 
            {
                gameObject.SetActive(false);

            });
        });

    }

    void CloseFade()
    {
        whiteFade.gameObject.SetActive(false);
    }

    void OnBtns()
    {
        newGameBtn.ShowInstant();
        continueBtn.ShowInstant();
        exitBtn.ShowInstant();
    }



}
