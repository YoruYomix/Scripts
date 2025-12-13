using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using MessagePipe;

public class FlowNode : IDisposable
{
    public float Duration;
    public List<IFlowNodeEffect> Effects = new List<IFlowNodeEffect>();
    public GameObject gameObject;

    private IDisposable _skipSubscription;  ///< 구독 핸들 (나중에 필요하면 Dispose)

    public FlowNode(ISubscriber<SkipAllNodesMessage> skipSubscriber, GameObject gameObject)
    {
        this.gameObject = gameObject;
        _skipSubscription = skipSubscriber.Subscribe(msg =>
        {
            Skip();
        });
    }

    public void Dispose()
    {
        _skipSubscription?.Dispose();
        _skipSubscription = null;
    }

    private void Skip()
    {
        Duration = 0;
        UnityEngine.Debug.Log($"[FlowNode] Skip 호출됨, Duration=0 으로 변경");
    }
}


