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

        // ======================================================
        // Subscribe
        // ======================================================

        private void Subscribe()
        {
            // 시퀀스 완료 시점에만 평가
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

        // ======================================================
        // Evaluate
        // ======================================================

        private void Evaluate(ReactorTrigger trigger)
        {
            if (_disposed) return;

            var scheduleCtx = new ReactorScheduleContext(trigger);

            foreach (var rule in _rules)
            {
                // 1️⃣ Schedule 조건 (AND)
                if (!PassScheduleConditions(rule, scheduleCtx))
                    continue;

                // 2️⃣ Node 조립 (ProviderReactor만)
                ChoMiniNode? node = AssembleNode(rule);

                // -------------------------
                // SimpleReactor
                // -------------------------
                if (node == null)
                {
                    rule.DoHook?.Invoke();
                    continue;
                }

                // -------------------------
                // ProviderReactor
                // -------------------------
                new ChoMiniReactorCoordinator(
                    node,
                    _msg,
                    rule.IsLifetimeLoop
                );
            }
        }

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

        // ======================================================
        // Assemble Node (ProviderReactor only)
        // ======================================================

        private ChoMiniNode? AssembleNode(ReactorRule rule)
        {
            // SimpleReactor → Node 없음
            if (rule.ProviderType == null)
                return null;

            // 1️⃣ TargetNodeTag 필터
            List<NodeSource> filteredSources = FilterNodeSources(rule);

            if (rule.NodeConditions.Count > 0 && filteredSources.Count == 0)
                return null;

            // 2️⃣ Provider 생성
            IChoMiniProvider provider =
                (IChoMiniProvider)Activator.CreateInstance(rule.ProviderType);

            // 3️⃣ Reactor 전용 Factory로 Node 생성
            var factory = new ChoMiniReactorFactory();
            factory.Initialize(
                filteredSources,
                new List<IChoMiniProvider> { provider },
                _msg.CompleteSubscriber,
                _msg
            );

            return factory.Create();
        }

        private List<NodeSource> FilterNodeSources(ReactorRule rule)
        {
            var result = new List<NodeSource>();

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

        // ======================================================
        // Dispose
        // ======================================================

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
