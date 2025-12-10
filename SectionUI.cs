using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using static UnityEngine.GraphicsBuffer;
using static UnityEngine.Rendering.CoreUtils;

public class SectionUI : MonoBehaviour
{
    public int index;
    public InteractiveButton btn;
    [SerializeField] Image thumbs;
    [SerializeField] GameObject ending;
    [SerializeField] GameObject clear;
    [SerializeField] GameObject locked;
    [SerializeField] GameObject open;
    [SerializeField] RectTransform targetA;
    public RectTransform targetB;
    [SerializeField] UILineRenderer uiLineRenderer;
    public SectionUI[] nextSectionUIs;
    public Section section;
    SectionData sectionData; 

    public void Init(int _index)
    {
        index = _index;
        section = SectionManager.sections[index];
        thumbs.sprite = section.sectionDefinition.ThumbnailSprite;
    }

    public void ViewRefresh()
    {
        UpdateView(section.GetStatus());
        uiLineRenderer.Init(targetA, CollectAllTargetB());
    }
    public List<RectTransform> CollectAllTargetB()
    {
        if (nextSectionUIs == null)
        {
            return null;
        }
        List<RectTransform> list = new List<RectTransform>();


        foreach (var ui in nextSectionUIs)
        {
            if (ui != null && ui.targetB != null)
            {
                list.Add(ui.targetB);
            }
        }

        return list;
    }
    public void UpdateView(SectionStatus status)
    {
        ending.SetActive(false);
        clear.SetActive(false);
        locked.SetActive(false);
        open.SetActive(false);

        Debug.Log("색션 유아이 뷰 업데이트: "+ status);
        switch (status)
        {
            case SectionStatus.Ending:
                ending.SetActive(true);
                btn.IsInteractable = true;
                break;

            case SectionStatus.Clear:
                clear.SetActive(true);
                btn.IsInteractable = true;
                break;

            case SectionStatus.Locked:
                locked.SetActive(true);
                btn.IsInteractable = false;
                break;

            case SectionStatus.Incomplete:
                open.SetActive(true);
                btn.IsInteractable = true;
                break;
        }
    }
}
