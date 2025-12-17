using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace Yoru.ChoMiniEngine
{
    // 리액터 실행 여부를 판단하는 순수 조건 인터페이스
    public interface IReactorCondition
    {
        bool IsSatisfied(ReactorContext context);
    }
    // IReactorCondition 평가에 전달되는 실행 컨텍스트
    // 리액터 조건 평가에 필요한 읽기 전용 컨텍스트
    public sealed class ReactorContext
    {
        public ChoMiniNode? CurrentNode { get; }
        public bool IsLastNodeComplete { get; }

        public ReactorContext(
            ChoMiniNode? currentNode,
            bool isLastNodeComplete)
        {
            CurrentNode = currentNode;
            IsLastNodeComplete = isLastNodeComplete;
        }
    }
    // 마지막 노드가 완료되었는지 검사
    public sealed class LastNodeCompleteCondition : IReactorCondition
    {
        public bool IsSatisfied(ReactorContext context)
        {
            return context.IsLastNodeComplete;
        }
    }

    // 노드 태그 검사
    public sealed class NodeTagCondition : IReactorCondition
    {
        private readonly string _tag;

        public NodeTagCondition(string tag)
        {
            _tag = tag;
        }

        public bool IsSatisfied(ReactorContext context)
        {
            return context.CurrentNode?.HasTag(_tag) == true;
        }
    }
    // 엔진 외부 상태를 확인하는 조건 (외부 훅)
    public sealed class ExternalPredicateCondition : IReactorCondition
    {
        private readonly Func<bool> _predicate;

        public ExternalPredicateCondition(Func<bool> predicate)
        {
            _predicate = predicate;
        }

        public bool IsSatisfied(ReactorContext context)
        {
            return _predicate();
        }
    }

}
