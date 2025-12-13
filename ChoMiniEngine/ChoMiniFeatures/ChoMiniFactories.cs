using Cysharp.Threading.Tasks;
using DG.Tweening;
using MessagePipe;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

// 팩토리
public class FlowSequenceFactory
{
    List<Transform> _targets;
    private List<Func<IEffectProvider>> _providerFactories;

    private List<IEffectProvider> _providers;  // Lazy-created, cached per scope

    LoopProvider _mockLoopProvider;
    int _index = 0;
    public int Count => _targets.Count;

    ISubscriber<SequenceStartMessage> _sequenceStartMessage;
    ISubscriber<SequenceEndMessage> _sequenceEndSubscriber;
    ISubscriber<SkipAllNodesMessage> _skipSubscriber;

    public FlowSequenceFactory()
    {
        _mockLoopProvider = new LoopProvider();  // 리팩토링중 임시
    }

    // ------------------------
    // Lazy Provider 초기화
    // ------------------------
    private void EnsureProviders()
    {
        if (_providers != null)
            return;

        _providers = new List<IEffectProvider>();

        if (_providerFactories == null)
            return; // 빈 Provider 목록으로 동작 가능

        foreach (var factory in _providerFactories)
            _providers.Add(factory());
    }


    public void Initialize(
        List<Transform> targets,
        List<Func<IEffectProvider>> providerFactories,
        ISubscriber<SkipAllNodesMessage> skipSubscriber)
    {
        _targets = targets;
        _providerFactories = providerFactories;
        _skipSubscriber = skipSubscriber;

        // 테스트/실사용 모두 안정적
        EnsureProviders();
    }

    public FlowNode Create()
    {
        var t = _targets[_index];
        _index = (_index + 1) % _targets.Count;



        GameObject go = t.gameObject;
        FlowNode node = new FlowNode(_skipSubscriber, go);
        Debug.Log("팩토리 내부의 크리에이트: " + go.name);


        // Provider가 Effects를 채운다
        foreach (var provider in _providers)
            provider.CollectEffects(go, node);

        // LoopProvider도 Effects를 채운다
        _mockLoopProvider.CollectEffects(go, node);

        // Duration 계산: 이벤트 리스트의 모든 듀레이션중 가장 큰 값이 노드 자체의 듀레이션 됨
        float maxDuration = 0f;
        foreach (var effect in node.Effects)
            maxDuration = Mathf.Max(maxDuration, effect.GetRequiredDuration());

        node.Duration = maxDuration;

        return node;
    }
}



