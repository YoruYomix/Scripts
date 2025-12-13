// 절차 이펙트의 규약이다. 노드에 붙는다, 뗀다 2가지다.
using UnityEngine;

namespace Yoru.ChoMiniEngine
{
    public interface IChoMiniNodeAction
    {
        float GetRequiredDuration();      // 추가!
        void Play();
        void Finish();

        GameObject GameObject { get; }
    }


}