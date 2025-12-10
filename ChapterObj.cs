using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cysharp.Threading.Tasks;

public class ChapterObj : MonoBehaviour
{
    
    public List<SectionObj> sectionObjs;
    public SectionObj nowSectionObj;
    public void Init()
    {
        sectionObjs = new List<SectionObj>();
        foreach (Transform child in transform)
        {
            if (child.TryGetComponent<SectionObj>(out var rb))
            {
                sectionObjs.Add(rb);
                continue;
            }
        }

        for (int i = 0; i < sectionObjs.Count; i++)
        {
            sectionObjs[i].Initialize(this);
        }
        nowSectionObj = sectionObjs[0];
        gameObject.SetActive(false);
    }





    
}
