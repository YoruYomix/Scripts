using UnityEngine;
using Animancer;
using Cysharp.Threading.Tasks;
using System.Threading;
using System;

[RequireComponent(typeof(AnimancerComponent))]
public class AnimInstaller : MonoBehaviour
{
    public AnimationClip[] clips;
    private AnimancerComponent animancer;

    private void Awake()
    {
        // Animator가 없으면 추가
        var animator = GetComponent<Animator>();
        if (animator == null)
            animator = gameObject.AddComponent<Animator>();

        animancer = GetComponent<AnimancerComponent>();
        if (animancer == null)
            animancer = gameObject.AddComponent<AnimancerComponent>();
    }

    public AnimationClip GetClip(int index)
    {
        if (index < 0 || index >= clips.Length) return null;
        return clips[index];
    }

    public AnimancerComponent Animancer => animancer;

    /// <summary>클립 재생. 루프 여부는 import 설정으로 결정.</summary>
    public async UniTask Play(int index, CancellationToken ct = default)
    {
        var clip = GetClip(index);
        if (clip == null) return;

        var state = animancer.Play(clip);

        // 루프 클립이면 OnEnd 호출 안 됨, 1회 재생 클립만 기다림
        if (!clip.isLooping)
        {
            var tcs = new UniTaskCompletionSource();
            state.Events.OnEnd = () => tcs.TrySetResult();

            try
            {
                await tcs.Task.AttachExternalCancellation(ct);
            }
            catch (OperationCanceledException)
            {
                animancer.Stop(clip);
            }
        }
    }

    public void StopAll()
    {
        animancer.Stop();
    }

    public void GoToStartPose(int index)
    {
        var clip = GetClip(index);
        if (clip == null) return;

        var state = animancer.Play(clip);
        state.Time = 0f;
        animancer.Evaluate();
        animancer.Stop(clip);
    }

    public void GoToEndPose(int index)
    {
        var clip = GetClip(index);
        if (clip == null) return;

        var state = animancer.Play(clip);
        state.NormalizedTime = 1f;
        animancer.Evaluate();
        animancer.Stop(clip);
    }
}
