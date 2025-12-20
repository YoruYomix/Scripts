using Cysharp.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.UI;


public class StoryDirectorLegacy : MonoBehaviour
{
    [SerializeField] Button button;
    [SerializeField] GameObject root;
    List<StoryObj> storyObjs;


    // Start is called before the first frame update
    void Start()
    {
        if (button == null)
        {
            return;
        }
        storyObjs = StoryObjFactory.SpawnStoryObj(root.transform);

        button.onClick.AddListener(NextSegment);
    }

    int curruntIndex = 0;
    void NextSegment()
    {
        StoryObj curruntPlayerble = GetCurruntPlayerble();

        switch (curruntPlayerble.storyState)
        {
            case StoryState.Waiting:
                curruntPlayerble.PlayAsync().Forget();
                break;
            case StoryState.Playing:
                curruntPlayerble.LoopStart();
                break;
            case StoryState.Looping:
                curruntPlayerble.ComplateVIew();
                curruntIndex++;
                GetCurruntPlayerble().PlayAsync().Forget();
                break;
        }
    }

    StoryObj GetCurruntPlayerble()
    {
        return storyObjs[curruntIndex];
    }

}
