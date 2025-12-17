using System.Collections.Generic;
using UnityEngine;
using System;
using MessagePipe;

namespace Yoru.ChoMiniEngine
{
    public class ChoMiniNode : IDisposable
    {
        public float Duration;
        public List<IChoMiniNodeAction> Actions = new List<IChoMiniNodeAction>();
        private readonly HashSet<string> _tags = new();

        private IDisposable _skipSubscription;  ///< 구독 핸들 (나중에 필요하면 Dispose)

        public ChoMiniNode(ISubscriber<ChoMiniScopeCompleteRequested> skipSubscriber)
        {
            _skipSubscription = skipSubscriber.Subscribe(msg =>
            {
                Complete();
            });
        }
        public void AddTag(string tag)
        {
            _tags.Add(tag);
        }

        public bool HasTag(string tag)
        {
            return _tags.Contains(tag);
        }

        private void Complete()
        {
            Duration = 0;
        }

        public void Dispose()
        {
            _skipSubscription?.Dispose();
            _skipSubscription = null;
        }

    }


}