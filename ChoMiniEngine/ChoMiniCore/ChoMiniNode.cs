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
        public List<object> _objects;

        private IDisposable _skipSubscription;  ///< 구독 핸들 (나중에 필요하면 Dispose)

        public ChoMiniNode(ISubscriber<ChoMiniLocalSkipRequested> skipSubscriber, List<object> objects)
        {
            _objects = objects;
            _skipSubscription = skipSubscriber.Subscribe(msg =>
            {
                Complete();
            });
        }

        public void Dispose()
        {
            _skipSubscription?.Dispose();
            _skipSubscription = null;
        }

        private void Complete()
        {
            Duration = 0;
            UnityEngine.Debug.Log($"[FlowNode] Skip 호출됨, Duration=0 으로 변경");
        }
    }


}