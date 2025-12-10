using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cysharp.Threading.Tasks;
using System.Threading;

public abstract class PageOnclickActionSequenceBase : MonoBehaviour
{
    public bool isPlaying;
    bool isStandBy;

    public SqStatus status;

    public void Init()
    {
        status = SqStatus.init;
        gameObject.SetActive(false);
        InitAction();
    }

    protected abstract void InitAction();

    public virtual async UniTask PlaySequence(CancellationToken token)
    {
        if (status == SqStatus.playing) return;
        gameObject.SetActive(true);
        status = SqStatus.playing;
    }


    public void StandBy()
    {
        if (status == SqStatus.standby) return;
        status = SqStatus.standby;
        StandByAction();
    }

    protected abstract void StandByAction();


    public void CompleteSequence()
    {
        status = SqStatus.complete;
        CompleteAction();
    }

    protected abstract void CompleteAction();

    public void CompleteInstantly()
    {
        gameObject.SetActive(true);
        CompleteInstantlyAction();
    }

    protected abstract void CompleteInstantlyAction();

}


public enum SqStatus
{
    init,
    playing,
    standby,
    complete
}