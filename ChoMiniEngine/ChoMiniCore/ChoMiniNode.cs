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


        private IDisposable _skipSubscription;  ///< 구독 핸들 (나중에 필요하면 Dispose)

        public ChoMiniNode(ISubscriber<ChoMiniScopeCompleteRequested> skipSubscriber)
        {
            _skipSubscription = skipSubscriber.Subscribe(msg =>
            {
                Complete();
            });
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