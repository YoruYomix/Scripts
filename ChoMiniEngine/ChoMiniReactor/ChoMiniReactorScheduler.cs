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
            Debug.Log("리액터 스케쥴러 생성 됨");
        }

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

        private void Evaluate(ReactorTrigger trigger)
        {
            if (_disposed) return;

            var ctx = new ReactorScheduleContext(trigger);

            foreach (var rule in _rules)
            {
                bool pass = true;
                foreach (var cond in rule.ScheduleConditions)
                {
                    if (!cond.IsSatisfied(ctx))
                    {
                        pass = false;
                        break;
                    }
                }

                if (!pass)
                    continue;

                // ✅ SimpleReactor: Do 즉시 실행
                if (rule.ProviderType == null)
                {
                    rule.DoHook?.Invoke();
                    continue;
                }

                // ProviderReactor
                ChoMiniNode? node = AssembleNode(rule);
                if (node == null)
                    continue;

                new ChoMiniReactorCoordinator(
                    node,
                    _msg,
                    rule.IsLifetimeLoop
                );
            }
        }


        // -------------------------
        // Assemble
        // -------------------------

        private ChoMiniNode? AssembleNode(ReactorRule rule)
        {
            // 1️⃣ NodeCondition 필터 (항상 실행)
            List<NodeSource> filtered = new();

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
                    filtered.Add(src);
            }

            // NodeCondition이 있는데 통과한 소스가 없으면 종료
            if (rule.NodeConditions.Count > 0 && filtered.Count == 0)
                return null;

            // 2️⃣ SimpleReactor → Node 없이 Do만 실행
            if (rule.ProviderType == null)
                return new ChoMiniNode(_msg.CompleteSubscriber); // or null + Do만 별도 처리

            // 3️⃣ ProviderReactor → Node 생성
            IChoMiniProvider provider =
                (IChoMiniProvider)Activator.CreateInstance(rule.ProviderType);

            var factory = new ChoMiniReactorFactory();
            factory.Initialize(
                filtered,
                new List<IChoMiniProvider> { provider },
                _msg.CompleteSubscriber,
                _msg
            );

            return factory.Create();
        }


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
