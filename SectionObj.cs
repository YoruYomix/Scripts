using System;
using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Unity.VisualScripting;
using UnityEngine;

public class SectionObj : MonoBehaviour
{
    public Section section;
    [SerializeField] int sectionIndex;
    [SerializeField] SectionComplete sectionComplete;
    SectionData sectionData;
    Transform pagesParent;
    public List<Page> pages;
    public Page FirstPage
    {
        get
        {

            return pages[0];
        }
    }


    public void Initialize(ChapterObj chapter)
    {
        section = SectionManager.sections[sectionIndex];
        gameObject.SetActive(false);
        pagesParent = transform.Find("Pages");
        sectionComplete.Init(this);
        pages = new List<Page>();
        foreach (Transform child in pagesParent)
        {
            Page page = child.GetComponent<Page>();

            if (page == null)
            {
                child.gameObject.AddComponent<Page>();
                page = child.GetComponent<Page>();
            }
            pages.Add(page);
        }
        sectionData = Resources.Load<SectionData>("SectionDefinitions/Section_0");
        for (int i = 0; i < pages.Count; i++)
        {
            Page Prev;
            Page Next;
            if (i<1)
            {
                Prev = null;
            }
            else
            {
                Prev = pages[i - 1];
            }
            if (i+1 >= pages.Count)
            {
                Next = null;
            }
            else
            {
                Next = pages[i + 1];
            }
            pages[i].Init(i,this,Prev,Next);
        }
    }



    public void SectionComplate(Transform transform)
    {
        sectionComplete.Open(transform);
    }

}
