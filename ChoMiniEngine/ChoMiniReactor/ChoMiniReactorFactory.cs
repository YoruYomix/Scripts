using MessagePipe;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Yoru.ChoMiniEngine
{
    /// <summary>
    /// Reactor 전용 Factory
    /// - NodeSource를 순회하며 Provider로 Action을 수집
    /// - Action이 하나도 없는 Node는 스킵
    /// - 모든 NodeSource가 무효면 null 반환
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

            Debug.Log($"[ReactorFactory] NodeSource Count = {_sources.Count}");
        }

        // ------------------------------
        // Create Node
        // ------------------------------
        public ChoMiniNode Create()
        {
            if (_sources == null || _sources.Count == 0)
            {
                Debug.LogWarning("[ReactorFactory] No NodeSources.");
                return null;
            }

            int tried = 0;
            int max = _sources.Count;

            while (tried < max)
            {
                NodeSource source = _sources[_index];
                _index = (_index + 1) % _sources.Count;
                tried++;

                ChoMiniNode node = new ChoMiniNode(_skipSubscriber);

                // Provider에게 source 전달
                foreach (var item in source.Items)
                {
                    if (item == null) continue;

                    foreach (var provider in _providers)
                    {
                        if (provider == null) continue;
                        provider.CollectEffects(item, node, _messageContext);
                    }
                }

                // ❌ Action이 하나도 없으면 의미 없는 Node → 스킵
                if (node.Actions.Count == 0)
                {
#if UNITY_EDITOR
                    Debug.Log("[ReactorFactory] Empty node skipped");
#endif
                    node.Dispose();
                    continue;
                }

                // Duration 계산
                float maxDuration = 0f;
                foreach (var action in node.Actions)
                {
                    maxDuration = Mathf.Max(
                        maxDuration,
                        action.GetRequiredDuration()
                    );
                }

                node.Duration = maxDuration;
                return node;
            }

            // ❗ 모든 NodeSource가 무효 → 조용히 실패
#if UNITY_EDITOR
            Debug.Log("[ReactorFactory] No valid node created (all empty)");
#endif
            return null;
        }
    }
}
