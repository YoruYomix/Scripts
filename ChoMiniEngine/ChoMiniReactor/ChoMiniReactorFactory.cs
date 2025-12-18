using MessagePipe;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Yoru.ChoMiniEngine
{
    /// <summary>
    /// Reactor 전용 Factory
    /// - NodeSource를 태그 조건으로 필터링
    /// - 통과한 것만 Node로 생산
    /// </summary>
    public sealed class ChoMiniReactorFactory : IChoMiniFactory
    {
        private List<NodeSource> _sources;
        private List<IChoMiniProvider> _providers;
        private ISubscriber<ChoMiniScopeCompleteRequested> _skipSubscriber;
        private ChoMiniScopeMessageContext _messageContext;

        private int _index;

        public int Count => _sources?.Count ?? 0;

        // ------------------------------
        // Initialize
        // ------------------------------
        public void Initialize(
            List<NodeSource> sources,
            List<IChoMiniProvider> providers,
            ISubscriber<ChoMiniScopeCompleteRequested> skipSubscriber,
            ChoMiniScopeMessageContext scopeMessageContext)
        {
            _sources = sources ?? throw new ArgumentNullException(nameof(sources));
            _providers = providers ?? new List<IChoMiniProvider>();
            _skipSubscriber = skipSubscriber;
            _messageContext = scopeMessageContext;

            _index = 0;

            Debug.Log($"[ReactorFactory] Filtered NodeSource Count = {_sources.Count}");
        }

        // ------------------------------
        // Create Node
        // ------------------------------
        public ChoMiniNode Create()
        {
            if (_sources == null || _sources.Count == 0)
                throw new InvalidOperationException(
                    "ChoMiniReactorFactory has no NodeSources."
                );

            int tried = 0;
            int max = _sources.Count;

            while (tried < max)
            {
                NodeSource source = _sources[_index];
                _index = (_index + 1) % _sources.Count;
                tried++;

                ChoMiniNode node = new ChoMiniNode(_skipSubscriber);

                foreach (var item in source.Items)
                {
                    if (item == null) continue;

                    foreach (var provider in _providers)
                    {
                        if (provider == null) continue;
                        provider.CollectEffects(item, node, _messageContext);
                    }
                }

                if (node.Actions.Count == 0)
                {
                    Debug.Log("[ReactorFactory] Empty node skipped");
                    node.Dispose();
                    continue; // 다음 source
                }

                float maxDuration = 0f;
                foreach (var action in node.Actions)
                    maxDuration = Mathf.Max(maxDuration, action.GetRequiredDuration());

                node.Duration = maxDuration;
                return node;
            }

            // ❗ 한 바퀴 다 돌았는데도 유효 Node 없음
            throw new InvalidOperationException(
                "[ReactorFactory] No valid Node could be created (all empty)."
            );
        }

    }
}
