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
        // Evaluate
        // --------------------------------------------------

        private void Evaluate(ReactorTrigger trigger)
        {
            if (_disposed) return;

            var scheduleCtx = new ReactorScheduleContext(trigger);

            foreach (var rule in _rules)
            {
                // 1) Scheduler 조건 (AND)
                if (!PassScheduleConditions(rule, scheduleCtx))
                    continue;

                // 2) TargetNodeTag 필터링
                List<NodeSource> matchedSources = FilterByTargetNode(rule);

                // Target 조건이 있는데 매칭이 하나도 없음 → 실행 ❌
                if (rule.NodeConditions.Count > 0 && matchedSources.Count == 0)
                    continue;

                // ----------------------------
                // CASE 3: Simple + No Tag
                // CASE 2: Simple + Tag
                // => 둘 다 "조건 통과하면 Do 1회"
                // ----------------------------
                if (rule.ProviderType == null)
                {
                    // Tag가 있으면 matchedSources.Count > 0일 때만 여기까지 옴
                    // Tag가 없으면 전체 대상이므로 여기까지 옴
                    rule.DoHook?.Invoke();
                    continue;
                }

                // ----------------------------
                // CASE 1: Provider + Tag
                // => "태그 매칭된 모든 타겟에 대해 Provider 발동"
                // => Do는 1회 (Scheduler 레벨에서 1번만 호출)
                // ----------------------------

                // Provider 실행은 matchedSources 각각에 대해 Coordinator 생성
                bool anyCoordinatorCreated = false;

                foreach (var src in matchedSources)
                {
                    // Node 생성 (빈 노드면 null 반환하도록 CreateNode가 처리)
                    ChoMiniNode node = CreateNode(rule, src);

                    if (node == null)
                        continue; // 이번 src는 스킵

                    anyCoordinatorCreated = true;

                    new ChoMiniReactorCoordinator(
                        createNode: () => node, // ✅ "1회성 Node"를 이미 만들어서 넘길 경우
                                                // ⚠️ 아래 주석 참고: 진짜 Loop면 createNode가 매번 새로 만들어야 함
                        msg: _msg,
                        isLifetimeLoop: rule.IsLifetimeLoop,
                        doHook: rule.DoHook // Do는 Coordinator 생명주기에 종속시키고 싶으면 Coordinator에서만 Invoke
                    );
                }

                // ✔ Do를 Scheduler에서 1회만 찍고 싶으면 여기서 호출
                // (지금 네 정책: "Provider 지정시 Do 1회")
                if (anyCoordinatorCreated)
                    rule.DoHook?.Invoke();
            }
        }

        // --------------------------------------------------
        // Scheduler 조건 (When*)
        // --------------------------------------------------

        private bool PassScheduleConditions(ReactorRule rule, ReactorScheduleContext ctx)
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
        // - src 1개 기준으로 Node 1개 생성
        // - 빈 Node면 null 반환 (조용히 스킵)
        // --------------------------------------------------

        private ChoMiniNode CreateNode(ReactorRule rule, NodeSource src)
        {
            if (rule.ProviderType == null)
                return null;

            IChoMiniProvider provider =
                (IChoMiniProvider)Activator.CreateInstance(rule.ProviderType);

            var factory = new ChoMiniReactorFactory();
            factory.Initialize(
                sources: new List<NodeSource> { src },
                providers: new List<IChoMiniProvider> { provider },
                skipSubscriber: _msg.CompleteSubscriber,
                scopeMessageContext: _msg
            );

            try
            {
                // ✅ Factory에서 "전부 empty면 null"로 바꿨다는 전제
                // (지금은 throw 버전이었으니, 그걸 null 반환 버전으로 바꾸면 여기서 안전해짐)
                return factory.Create();
            }
            catch (InvalidOperationException e)
            {
                // 개발 중엔 로그로 남기고 스킵
                Debug.LogWarning($"[ReactorScheduler] ReactorFactory skipped: {e.Message}");
                return null;
            }
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
