using Cysharp.Threading.Tasks;
using DG.Tweening;
using System.Threading;
using System.Threading.Tasks;
using Unity.VisualScripting;
using UnityEngine;

public enum StoryState
{
    Waiting, Playing, Looping, Completed
}

public interface ILoopableStoryObj
{
    // 재생을 시작합니다.
    public UniTask LoopVisual();
}
public abstract class StoryObj
{
    public StoryState storyState;
    protected GameObject _gameObject;
    public StoryObj(GameObject gameObject)
    {
        _gameObject = gameObject;
        Wait();
    }

    public void Wait()
    {
        storyState = StoryState.Waiting;
        _gameObject.SetActive(false);
    }

    public abstract UniTask PlayAsync();

    public async UniTask LoopStart()
    {
        _gameObject.SetActive(true);
        storyState = StoryState.Looping;
        if (this is ILoopableStoryObj)
        {
            ILoopableStoryObj instanceWithInterface = this as ILoopableStoryObj;
            await instanceWithInterface.LoopVisual();
        }
        return;
    }

    public abstract void ComplateVIew();

}

