using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Animancer;
using Cysharp.Threading.Tasks;
using System.Threading;
using System;

public class StoryAnimation : MonoBehaviour
{
    public AnimancerComponent animancer;  // 애니메이터 대신
    [SerializeField] AnimationClip[] animationClips;


    public AnimancerState currentState;

    public void Initialize()
    {
        animancer = GetComponent<AnimancerComponent>();
    }


    public void PlayAni(int index)
    {
        currentState = animancer.Play(animationClips[index]);
        currentState.Time = 0;     // 반드시 넣기
        currentState.Speed = 1;    // 혹시 모르니 설정
        currentState.Weight = 1;
    }
    public void PlayAni(string clipName)
    {
        currentState = animancer.Play(Array.Find(animationClips, x => x.name== clipName));
        currentState.Time = 0;     // 반드시 넣기
        currentState.Speed = 1;    // 혹시 모르니 설정
        currentState.Weight = 1;
    }

    public async UniTask PlayAnimationAsync(int index)
    {
        if (animationClips == null) return;
        AnimationClip animationClip = animationClips[index];
        // AnimancerState 반환, 끝날 때까지 기다릴 수 있음
        AnimancerState state = animancer.Play(animationClip);

        state.Time = 0;     // 반드시 넣기
        state.Speed = 1;    // 혹시 모르니 설정
        state.Weight = 1;
        // 끝날 때까지 대기
        await state.ToUniTask(); // UniTask로 변환 후 await
    }

    public async UniTask PlayAnimationAsync(string clipName)
    {
        if (animationClips == null)
        {
            Debug.LogWarning("AnimationClips가 null입니다.");
            return;
        }

        AnimationClip clip = Array.Find(animationClips, x => x.name == clipName);
        if (clip == null)
        {
            Debug.LogWarning($"Clip {clipName}을 찾을 수 없습니다.");
            return;
        }

        var state = animancer.Play(clip);
        state.Time = 0;
        state.Speed = 1;
        state.Weight = 1;

        // 루프 애니메이션이면 강제 딜레이 후 종료
        if (clip.isLooping)
        {
            Debug.LogWarning($"{clipName}은 루프 애니메이션입니다. 짧은 딜레이 후 종료 처리.");
            await UniTask.Delay(TimeSpan.FromSeconds(clip.length + 0.5f), cancellationToken: this.GetCancellationTokenOnDestroy());
            return;
        }

        // 일반 애니메이션은 끝날 때까지 기다리되, Timeout 적용
        try
        {
            await state.ToUniTask(cancellationToken: this.GetCancellationTokenOnDestroy())
                       .Timeout(TimeSpan.FromSeconds(clip.length + 0.5f));
        }
        catch (TimeoutException)
        {
            Debug.LogWarning($"{clipName} 재생이 너무 길어 종료 처리");
        }
    }

    //public async UniTask PlayAnimationAsync(string clipName)
    //{
    //    if (animationClips == null) return;

    //    AnimationClip animationClip = Array.Find(animationClips, x => x.name == clipName);

    //    // AnimancerState 반환, 끝날 때까지 기다릴 수 있음
    //    AnimancerState state = animancer.Play(animationClip);
    //    state.Time = 0;     // 반드시 넣기
    //    state.Speed = 1;    // 혹시 모르니 설정
    //    state.Weight = 1;
    //    // 끝날 때까지 대기
    //    await state.ToUniTask(); // UniTask로 변환 후 await
    //}

    public async UniTask PlayAnimationAsyncAction(string clipName, Action complateAction)
    {
        if (animationClips == null) return;

        AnimationClip animationClip = Array.Find(animationClips, x => x.name == clipName);

        // AnimancerState 반환, 끝날 때까지 기다릴 수 있음
        AnimancerState state = animancer.Play(animationClip);
        state.Time = 0;     // 반드시 넣기
        state.Speed = 1;    // 혹시 모르니 설정
        state.Weight = 1;
        // 끝날 때까지 대기
        await state.ToUniTask(); // UniTask로 변환 후 await
        if (complateAction!=null)
        {
            Debug.Log("애니메이션 완료 후 액션");
            complateAction.Invoke();
        }

    }

    public async UniTask PlayAnimationCancelableAsync(CancellationToken token, int index)
    {
        AnimationClip animationClip = animationClips[index];
        // 애니메이션 상태
        currentState = animancer.Play(animationClip);
        currentState.Time = 0;     // 반드시 넣기
        currentState.Speed = 1;    // 혹시 모르니 설정
        currentState.Weight = 1;
        try
        {
            // 끝날 때까지 대기, 클릭 시 취소됨
            await currentState.ToUniTask(cancellationToken: token);

        }
        catch (OperationCanceledException)
        {
            // 취소 시 원래 상태로 복원
            // currentState.NormalizedTime = 1f;
            ComplateAnimation(animationClip);

        }
        finally
        {

        }
    }

    public async UniTask PlayAnimationCancelableAsync(CancellationToken token, string clipName)
    {
        AnimationClip animationClip = Array.Find(animationClips, x => x.name == clipName);
        // 애니메이션 상태
        currentState = animancer.Play(animationClip);
        currentState.Time = 0;     // 반드시 넣기
        currentState.Speed = 1;    // 혹시 모르니 설정
        currentState.Weight = 1;
        try
        {
            // 끝날 때까지 대기, 클릭 시 취소됨
            await currentState.ToUniTask(cancellationToken: token);

        }
        catch (OperationCanceledException)
        {
            // 취소 시 원래 상태로 복원
            // currentState.NormalizedTime = 1f;
            ComplateAnimation(animationClip);

        }
        finally
        {

        }
    }

    public void ComplateAnimation(AnimationClip animationClip)             // 애니메이션을 완료상태로 만듬
    {
        if (currentState == null)
        {
            return;
        }
        currentState.Time = animationClip.length;
        currentState.Speed = 0f;
    }
    public void ComplateAnimation(int index)             // 애니메이션을 완료상태로 만듬
    {
        ComplateAnimation(animationClips[index]);
    }
}
