// 절차 이펙트의 규약이다. 노드에 붙는다, 뗀다 2가지다.
using UnityEngine;

public interface IFlowNodeEffect
{
    float GetRequiredDuration();      // 추가!
    void Play();
    void Finish();

    GameObject GameObject { get; }
}


