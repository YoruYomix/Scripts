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
                {

                    Debug.Log($"[ReactorScheduler] FAIL: {rule.ProviderType.Name}");

                    continue;
                }


                //Debug.Log(
                //    $"[ReactorScheduler] PASS: {rule.ProviderType.Name} " +
                //    $"Lifetime={rule.IsLifetimeLoop}"
                //);


                // 실행은 아직 안 함
                new ChoMiniReactorCoordinator(rule, _msg);
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