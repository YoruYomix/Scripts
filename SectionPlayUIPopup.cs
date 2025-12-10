using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SectionPlayUIPopup : MonoBehaviour
{
    [SerializeField] Button screenButton;
    [SerializeField] AnimancerDelegator AnimancerDelegator;
    [SerializeField] SectionUI sectionUI;
    private void Awake()
    {
        screenButton.onClick.AddListener(CloseUI);
    }

    void CloseUI()
    {
        Debug.Log("버튼 클릭 됨");
        AnimancerDelegator.PlayClipAndSetDelegates("SectionPlayUI_Disappear", UIClosed);
    }

    void UIClosed()
    {
        gameObject.SetActive(false);
    }

    public void OpenUI(int sectionIndex)
    {
        gameObject.SetActive(true);
        AnimancerDelegator.PlayClipAndSetDelegates("SectionPlayUI_appear", null);
        sectionUI.Init(sectionIndex);
        sectionUI.ViewRefresh();
    }
}
