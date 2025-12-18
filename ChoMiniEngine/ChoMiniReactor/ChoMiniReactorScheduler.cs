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
        // Evaluate (최종 정책 반영)
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

                // 2️⃣ Node 필터링
                var matchedSources = FilterByTargetNode(rule);

                // ==================================================
                // CASE 3
                // Provider NULL + Tag 없음
                // → 조건만 맞으면 Do 1회
                // ==================================================
                if (rule.ProviderType == null && rule.NodeConditions.Count == 0)
                {
                    rule.DoHook?.Invoke();
                    continue;
                }

                // ==================================================
                // CASE 2
                // Provider NULL + Tag 있음
                // → 태그가 하나라도 있으면 Do 1회
                // ==================================================
                if (rule.ProviderType == null && rule.NodeConditions.Count > 0)
                {
                    if (matchedSources.Count > 0)
                        rule.DoHook?.Invoke();

                    continue;
                }

                // ==================================================
                // CASE 1
                // Provider 있음 (+ Tag 있음)
                // → 태그 붙은 모든 NodeSource에 Provider 발동
                // → Do는 전체 기준 1회
                // ==================================================
                if (rule.ProviderType != null)
                {
                    if (matchedSources.Count == 0)
                        continue;

                    foreach (var source in matchedSources)
                    {
                        var node = CreateNode(rule, source);

                        new ChoMiniReactorCoordinator(
                            createNode: () => CreateNode(rule, source),
                            msg: _msg,
                            isLifetimeLoop: rule.IsLifetimeLoop,
                            doHook: rule.DoHook
                        );
                    }

                    continue; // ❗ Scheduler에서는 Do 실행 안 함
                }
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
            // Tag 조건 없음 → 전체 대상
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
            NodeSource source)
        {
            if (rule.ProviderType == null)
                return null;

            IChoMiniProvider provider =
                (IChoMiniProvider)Activator.CreateInstance(rule.ProviderType);

            var factory = new ChoMiniReactorFactory();
            factory.Initialize(
                new List<NodeSource> { source },
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
