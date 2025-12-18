using System;
using System.Collections.Generic;
using MessagePipe;
using UnityEngine;

namespace Yoru.ChoMiniEngine
{
    public sealed class ChoMiniReactorScheduler : IDisposable
    {
        private readonly IReadOnlyList<ReactorRule> _rules;
        private readonly ChoMiniScopeMessageContext _msg;

        private readonly List<IDisposable> _subs = new();
        private bool _disposed;

        public ChoMiniReactorScheduler(
            IReadOnlyList<ReactorRule> rules,
            ChoMiniScopeMessageContext msg)
        {
            _rules = rules ?? Array.Empty<ReactorRule>();
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

            for (int i = 0; i < _rules.Count; i++)
            {
                var rule = _rules[i];

                var providerName = rule.ProviderType?.Name ?? "SimpleReactor";
                var lifetimeText = (rule.ProviderType != null) ? rule.IsLifetimeLoop.ToString() : "N/A";

                bool pass = true;
                string? failCondName = null;

                foreach (var cond in rule.ScheduleConditions)
                {
                    if (!cond.IsSatisfied(ctx))
                    {
                        pass = false;
                        failCondName = cond.GetType().Name; // 어떤 조건에서 떨어졌는지
                        break;
                    }
                }

                if (!pass)
                {
                    Debug.Log(
                        $"[ReactorScheduler] FAIL " +
                        $"Trigger={trigger} Provider={providerName} Lifetime={lifetimeText} " +
                        $"FailCond={failCondName}"
                    );
                    continue;
                }

                Debug.Log(
                    $"[ReactorScheduler] PASS " +
                    $"Trigger={trigger} Provider={providerName} Lifetime={lifetimeText} " +
                    $"ScheduleCondCount={rule.ScheduleConditions.Count} NodeCondCount={rule.NodeConditions.Count}"
                );



            }
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