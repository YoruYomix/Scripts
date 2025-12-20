using MessagePipe;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Yoru.ChoMiniEngine
{
    // 팩토리

    // ======================================================
    // Sequence Factory
    // ======================================================
    public class ChoMiniSequenceFactory : IChoMiniFactory
    {
        private List<NodeSource> _nodeSources;

        private List<IChoMiniProvider> _providers;
        private int _index = 0;
        private ISubscriber<ChoMiniScopeCompleteRequested> _skipSubscriber;
        private ChoMiniScopeMessageContext _messageContext;
        public int Count => _nodeSources?.Count ?? 0;

        // ------------------------------
        // Initialize
        // ------------------------------
        public void Initialize(
            List<NodeSource> nodeSources,
            List<IChoMiniProvider> providers,
            ISubscriber<ChoMiniScopeCompleteRequested> skipSubscriber,
            ChoMiniScopeMessageContext scopeMessageContext)
        {
            if (nodeSources == null)
                throw new ArgumentNullException(nameof(nodeSources));

            _nodeSources = nodeSources;
            _providers = providers;
            _skipSubscriber = skipSubscriber;
            _messageContext = scopeMessageContext;
        }

        // ------------------------------
        // Create Node
        // ------------------------------
        public ChoMiniNode Create()
        {
            if (_nodeSources == null || _nodeSources.Count == 0)
                throw new InvalidOperationException(
                    "ChoMiniSequenceFactory is not initialized or has no NodeSources."
                );

            // 1) 현재 step 선택
            NodeSource source = _nodeSources[_index];
            _index = (_index + 1) % _nodeSources.Count;

            // 2) Node 생성
            ChoMiniNode node = new ChoMiniNode(_skipSubscriber);

            // 3) Provider에게 source 전달
            foreach (object item in source.Items)
            {
                if (item == null) continue;

                foreach (IChoMiniProvider provider in _providers)
                {
                    if (provider == null) continue;

                    provider.CollectEffects(item, node, _messageContext);
                }
            }

            // 4) Duration 계산
            float maxDuration = 0f;
            foreach (var effect in node.Actions)
            {
                maxDuration = Mathf.Max(maxDuration, effect.GetRequiredDuration());
            }

            node.Duration = maxDuration;
            return node;
        }
    }
}