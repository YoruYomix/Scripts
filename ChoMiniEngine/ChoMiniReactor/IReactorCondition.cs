using System;

namespace Yoru.ChoMiniEngine
{
    // -------------------------
    // Schedule
    // -------------------------

    public interface IReactorScheduleCondition
    {
        bool IsSatisfied(ReactorScheduleContext context);
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

    // -------------------------
    // Node Target
    // -------------------------

    public interface IReactorNodeCondition
    {
        bool IsSatisfied(ReactorNodeContext context);
    }

    public sealed class ReactorNodeContext
    {
        public NodeSource NodeSource { get; }

        public ReactorNodeContext(NodeSource nodeSource)
        {
            NodeSource = nodeSource;
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
            return context.NodeSource.HasTag(_tag);
        }
    }
}
