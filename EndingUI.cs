using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EndingUI : MonoBehaviour
{
    public GameObject UIParent;
    public UIFader endingTitleUI;
    public GameObject endingSelectionUI;
    public UIFader endingBlackScreenUI;
    public UIButtonHover backTitleButton;
    public UIButtonHover backSecetionButton;


    public void Init()
    {
        endingTitleUI.Init();
        endingTitleUI.gameObject.SetActive(false);
        endingBlackScreenUI.Init();
        endingSelectionUI.gameObject.SetActive(false);
        UIParent.SetActive(false);
    }
}
