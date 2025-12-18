using System;
using System.Collections.Generic;
using MessagePipe;
using UnityEngine;

namespace Yoru.ChoMiniEngine
{
    public sealed class ChoMiniReactorScheduler : IDisposable
    {
        private readonly IReadOnlyList<ReactorRule> _rules;
        private readonly IReadOnlyList<NodeSource> _nodeSources;
        private readonly ChoMiniScopeMessageContext _msg;

        private readonly List<IDisposable> _subs = new();
        private bool _disposed;

        public ChoMiniReactorScheduler(
            IReadOnlyList<ReactorRule> rules,
            IReadOnlyList<NodeSource> nodeSources,
            ChoMiniScopeMessageContext msg)
        {
            _rules = rules ?? Array.Empty<ReactorRule>();
            _nodeSources = nodeSources ?? Array.Empty<NodeSource>();
            _msg = msg ?? throw new ArgumentNullException(nameof(msg));

            Subscribe();
            Debug.Log("[ReactorScheduler] Created");
        }

        // --------------------------------------------------
        // Subscribe
        // --------------------------------------------------

        private void Subscribe()
        {
            _subs.Add(
                _msg.SequenceCompleteSubscriber.Subscribe(_ =>
                {
                    Evaluate(ReactorTrigger.SequenceCompleted);
                })
            );

            _subs.Add(
                _msg.CleanupSubscriber.Subscribe(_ => Dispose())
            );
        }

        // --------------------------------------------------
        // Evaluate (Scheduler 조건 AND 평가)
        // --------------------------------------------------

        private void Evaluate(ReactorTrigger trigger)
        {
            if (_disposed) return;

            var scheduleCtx = new ReactorScheduleContext(trigger);

            foreach (var rule in _rules)
            {
                // 1️⃣ Scheduler 조건 (AND)
                if (!PassScheduleConditions(rule, scheduleCtx))
                    continue;

                // 2️⃣ TargetNodeTag 조건 (AND, 필터)
                var matchedSources = FilterByTargetNode(rule);

                // Target 조건이 있는데 하나도 매칭 안 됨 → 실행 ❌
                if (rule.NodeConditions.Count > 0 && matchedSources.Count == 0)
                    continue;

                // 3️⃣ SimpleReactor
                if (rule.ProviderType == null)
                {
                    rule.DoHook?.Invoke();
                    continue;
                }

                // 4️⃣ ProviderReactor → Node 생성
                ChoMiniNode node = CreateNode(rule, matchedSources);
                if (node == null)
                    continue;

                new ChoMiniReactorCoordinator(
                    node,
                    _msg,
                    rule.IsLifetimeLoop
                );

                rule.DoHook?.Invoke();
            }
        }

        // --------------------------------------------------
        // Scheduler 조건 (When*)
        // --------------------------------------------------

        private bool PassScheduleConditions(
            ReactorRule rule,
            ReactorScheduleContext ctx)
        {
            foreach (var cond in rule.ScheduleConditions)
            {
                if (!cond.IsSatisfied(ctx))
                    return false;
            }
            return true;
        }

        // --------------------------------------------------
        // TargetNodeTag 조건 (필터)
        // --------------------------------------------------

        private List<NodeSource> FilterByTargetNode(ReactorRule rule)
        {
            if (rule.NodeConditions.Count == 0)
                return new List<NodeSource>(_nodeSources);

            List<NodeSource> result = new();

            foreach (var src in _nodeSources)
            {
                bool pass = true;

                foreach (var cond in rule.NodeConditions)
                {
                    if (!cond.IsSatisfied(new ReactorNodeContext(src)))
                    {
                        pass = false;
                        break;
                    }
                }

                if (pass)
                    result.Add(src);
            }

            return result;
        }

        // --------------------------------------------------
        // Node 생성 (ProviderReactor)
        // --------------------------------------------------

        private ChoMiniNode CreateNode(
            ReactorRule rule,
            List<NodeSource> sources)
        {
            if (sources.Count == 0)
                return null;

            IChoMiniProvider provider =
                (IChoMiniProvider)Activator.CreateInstance(rule.ProviderType);

            var factory = new ChoMiniReactorFactory();
            factory.Initialize(
                sources,
                new List<IChoMiniProvider> { provider },
                _msg.CompleteSubscriber,
                _msg
            );

            return factory.Create();
        }

        // --------------------------------------------------
        // Dispose
        // --------------------------------------------------

        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;

            foreach (var s in _subs)
                s.Dispose();

            _subs.Clear();
        }
    }
}