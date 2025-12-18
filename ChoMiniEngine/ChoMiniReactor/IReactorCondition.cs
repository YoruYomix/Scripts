using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace Yoru.ChoMiniEngine
{

    // "언제 실행할지" 판단 (Scheduler 전용)
    public interface IReactorScheduleCondition
    {
        bool IsSatisfied(ReactorScheduleContext context);
    }
    // "무엇을 만들지" 판단 (ReactorNodeFactory 전용)
    public interface IReactorNodeCondition
    {
        bool IsSatisfied(ReactorNodeContext context);
    }

    public enum ReactorTrigger
    {
        SequenceCompleted,
    }

    public sealed class ReactorScheduleContext
    {
        public ReactorTrigger Trigger { get; }

        public ReactorScheduleContext(ReactorTrigger trigger)
        {
            Trigger = trigger;
        }
    }

    public sealed class ReactorNodeContext
    {
        public IReadOnlyList<NodeSource> NodeSources { get; }

        public ReactorNodeContext(IReadOnlyList<NodeSource> nodeSources)
        {
            NodeSources = nodeSources;
        }
    }


    public sealed class OnSequenceCompletedCondition : IReactorScheduleCondition
    {
        public bool IsSatisfied(ReactorScheduleContext context)
        {
            return context.Trigger == ReactorTrigger.SequenceCompleted;
        }
    }

    public sealed class ExternalPredicateCondition : IReactorScheduleCondition
    {
        private readonly Func<bool> _predicate;

        public ExternalPredicateCondition(Func<bool> predicate)
        {
            _predicate = predicate;
        }

        public bool IsSatisfied(ReactorScheduleContext context)
        {
            return _predicate();
        }
    }
    public sealed class NodeTagCondition : IReactorNodeCondition
    {
        private readonly string _tag;

        public NodeTagCondition(string tag)
        {
            _tag = tag;
        }

        public bool IsSatisfied(ReactorNodeContext context)
        {
            var sources = context.NodeSources;
            for (int i = 0; i < sources.Count; i++)
            {
                if (sources[i].HasTag(_tag))
                    return true;
            }
            return false;
        }
    }
}
