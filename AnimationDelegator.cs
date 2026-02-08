using UnityEngine;
using System;
using Animancer;
using System.Collections.Generic;

public class AnimancerDelegator : MonoBehaviour
{
    [Header("Animancer Components")]
    [SerializeField]
    private AnimancerComponent animancer;

    [Header("Animation Clips")]
    // 인스펙터에서 등록할 클립 배열
    [SerializeField]
    private AnimationClip[] animationClips;

    // 클립 이름으로 검색하기 위한 딕셔너리
    private Dictionary<string, AnimationClip> clipMap = new Dictionary<string, AnimationClip>();

    // 💡 전역 델리게이트를 제거하거나 필asdsad요에 따라 유sd지할 수 있습니다.
    // public Action OnAnimationStart;sdsadsad
    // public Action OnAnimationEnd; // 이d 코드를 제거ahjkjhksdasd하sd여 인자 사용을 명확히 합니다.

    void Awake()
    {
        // AnimancerComponent 오기
        if (animancer == null)
        {
            animancer = GetComponent<AnimancerComponent>();
        }

        if (animancer == null)
        {
            Debug.LogError("Animancer Component가 이 오브젝트에 없습니다.");
            return;
        }

        // 인스펙터에 등록된 클립을 딕셔너리에 매핑
        foreach (var clip in animationClips)
        {
            if (clip != null && !clipMap.ContainsKey(clip.name))
            {
                clipMap.Add(clip.name, clip);
            }
        }

        Debug.Log($"Animancer 전부닫기 완료: {clipMap.Count}개의 클립 등록됨.");
    }

    /// <summary>
    /// string clipName과 종료 시 실행할 Action을 받아서 클립을 재생하고 델리게이트를 설정합니다.
    /// </summary>
    /// <param name="clipName">재생할 애니메이션 클립의 이름</param>
    /// <param name="onEndAction">애니메이션 종료 시 실행할 Action (델리게이트)</param>
    public void PlayClipAndSetDelegates(string clipName, Action onEndAction)
    {
        if (animancer == null || !clipMap.ContainsKey(clipName))
        {
            Debug.LogError($"클립 이름 '{clipName}'을(를) 찾을 수 없습니다. 재생 요청 실패.");
            return;
        }

        AnimationClip clipToPlay = clipMap[clipName];

        // 1. 애니메이션 재생 요청
        AnimancerState state = animancer.Play(clipToPlay);

        Debug.Log($"애니메이션 '{clipName}' 재생 시작 요청.");

        // --- 시작 시점 (시작 델리게이트는 요청 시 즉시 실행) ---
        // 만약 시작 시점에 호출할 전역 델리게이트가 필요하다면 여기에 OnAnimationStart?.Invoke();를 추가합니다.

        // --- 종료 델리게이트 바인딩 ---
        // 💡 인자로 받은 onEndAction을 애니메이션 종료 시점에 실행하도록 등록합니다.
        state.Events.OnEnd = onEndAction;
    }
}