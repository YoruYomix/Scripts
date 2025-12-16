// 절차 이펙트의 규약이다. 노드에 붙는다, 뗀다 2가지다.
using UnityEngine;

namespace Yoru.ChoMiniEngine
{
    public interface IChoMiniNodeAction
    {
        float GetRequiredDuration();      // 추가!
        void Play();
        void Complete();

        void Pause();
        void Resume();

        // 추후 재생시점부터 재시작 구현용.
        //
        // 재시작 로직:
        // 컨버터가 노드의 듀레이션-time
        // 액션들에게 리커버리(time)
        void Recovery(float time);

        GameObject GameObject { get; }
    }


}