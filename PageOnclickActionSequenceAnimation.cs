using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using Animancer;
using Cysharp.Threading.Tasks;
using UnityEngine;


public class PageOnclickActionSequenceAnimation : PageOnclickActionSequenceBase
{
    StoryAnimation storyAnimation;

    protected override void InitAction()
    {
        storyAnimation = GetComponent<StoryAnimation>();
        storyAnimation.Initialize();
    }

    async public override UniTask PlaySequence(CancellationToken token)
    {
        base.PlaySequence(token);
        await storyAnimation.PlayAnimationCancelableAsync(token,0);
        CompleteSequence();
    }

    protected override void CompleteAction()
    {

    }

    protected override void CompleteInstantlyAction()
    {
        storyAnimation.ComplateAnimation(0);
    }



    protected override void StandByAction()
    {

    }
}
