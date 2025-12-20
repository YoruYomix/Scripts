using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public static class StoryObjFactory
{

    static public List<StoryObj> SpawnStoryObj(Transform root)
    {
        List<StoryObj> storyObjs = new List<StoryObj>();
        foreach (Transform childTransform in root)
        {
            Image image = childTransform.GetComponent<Image>();

            if (image != null)
            {
                StoryImage storyObj = new StoryImage(childTransform.gameObject);
                storyObjs.Add(storyObj);
            }
            Text text = childTransform.GetComponent<Text>();
            if (text != null)
            {
                StoryText storyText = new StoryText(childTransform.gameObject);
                storyObjs.Add(storyText);
            }
        }
        return storyObjs;
    }

  
}
