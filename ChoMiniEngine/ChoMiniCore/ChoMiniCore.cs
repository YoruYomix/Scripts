using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using UnityEngine;



namespace Yoru.ChoMiniEngine
{
    // 단일 노드를 받아서 엔진의 작동 트리거를 구독한다.
    public class ChoMiniNodeRunner
    {
        public async UniTask RunNode(ChoMiniNode node)
        {
            foreach (var effect in node.Effects)
            {
                effect.Play();  // 노드의 이펙트들이 재생을 시작
            }
            float time = 0f;
            while (time < node.Duration)
            {
                time += Time.deltaTime;
                await UniTask.Yield();
            }
            foreach (var effect in node.Effects)  // 노드의 이펙트들이 재생을 종료
                effect.Finish();
        }
    }

    // 노드 팩토리를 받아 팩토리에서 나오는 노드 리스트를 재생한다
    public class ChoMiniOrchestrator
    {
        private readonly ChoMiniNodeRunner _runner;
        private ChoMiniSequenceFactory _factory;
        public Action OnComplate;
        ChoMiniLocalMessageContext _localMsg;


        public ChoMiniOrchestrator(
            ChoMiniNodeRunner runner)
        {
            _runner = runner;
        }
        public void Initialize(ChoMiniLocalMessageContext localMessageContext)
        {
            _localMsg = localMessageContext;
        }

        public void SetFactory(ChoMiniSequenceFactory factory) { _factory = factory; }

        public async UniTask PlaySequence()
        {

            List<ChoMiniNode> nodes = new List<ChoMiniNode>();
            int count = _factory.Count;
            for (int i = 0; i < count; i++)
            {
                ChoMiniNode flowNode = _factory.Create();
                nodes.Add(flowNode);
            }

            // 모든 노드 순차 재생
            foreach (var node in nodes)
                await _runner.RunNode(node);  // 노드 하나 끝날 때 까지 대기  

            Debug.Log("리스트 전체 재생 완료");

            // 시퀀스 종료 방송
            OnComplate?.Invoke();
        }
    }
}