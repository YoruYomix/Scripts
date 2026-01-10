using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ContinueMenu : MonoBehaviour
{
    [SerializeField] GameObject uiParent;
    [SerializeField] SectionPlayUIPopup SectionPlayUIPopup; // SectionPlayUIPopup은 정의되어 있다고 가정
    public SectionUI[] sectionUIs; // SectionUI와 section은 정의되어 있다고 가정

    private void Start()
    {
        sectionUIs = FindSectionUIsInChildren(uiParent);

        for (int i = 0; i < sectionUIs.Length; i++)
        {
            sectionUIs[i].Init(i);

            // 🌟🌟🌟 핵심: 람다식 클로저 문제 방지를 위한 지역 변수 복사 (필수)
            int index = i;

            // customButton은 InteractiveButton 타입이며 OnClickAsObservable()을 제공합니다.
            var customButton = sectionUIs[i].btn;

            if (customButton == null)
            {
                continue;
            }
            customButton.클릭시행동 += () => { onclickOpenPlayPopupBtn(i); }; 
        }

        InitializeNextSectionUIs();

        foreach (SectionUI sectionUI in sectionUIs)
        {
            sectionUI.ViewRefresh();
        }

        SectionPlayUIPopup.gameObject.SetActive(false);
    }

    void onclickOpenPlayPopupBtn(int i)
    {
        SectionPlayUIPopup.OpenUI(i);
    }


    public SectionUI[] FindSectionUIsInChildren(GameObject parentObject)
    {
        if (parentObject == null)
        {
            Debug.LogError("FindSectionUIsInChildren: 부모 GameObject가 Null입니다.");
            return new SectionUI[0];
        }

        // GetComponentsInChildren<T>() 메서드를 사용하여 SectionUI 컴포넌트를 부모 및 모든 자식들에서 찾습니다.
        SectionUI[] sectionUIs = parentObject.GetComponentsInChildren<SectionUI>();

        return sectionUIs;
    }

    // 기존 InitializeNextSectionUIs 코드는 그대로 유지
    public void InitializeNextSectionUIs()
    {
        for (int i = 0; i < sectionUIs.Length; i++)
        {
            SectionUI currentUI = sectionUIs[i];

            if (currentUI.section != null && currentUI.section.Choices != null)
            {
                // LINQ를 사용하여 필터링:
                currentUI.nextSectionUIs = sectionUIs
                    .Where(nextUI => nextUI.section != null && currentUI.section.Choices.Contains(nextUI.section.Index))
                    .ToArray();
            }
            else
            {
                currentUI.nextSectionUIs = new SectionUI[0];
            }
        }
    }
}