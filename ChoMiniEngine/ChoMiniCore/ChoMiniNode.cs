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

    public readonly struct NodeSource
    {
        public readonly IReadOnlyList<object> Items;
        private readonly HashSet<string> _tags;

        // 1) 기존 코드 호환: tags 없이도 생성 가능
        public NodeSource(IReadOnlyList<object> items)
            : this(items, Array.Empty<string>())
        {
        }

        // 2) 가장 편한 DSL용: 가변 인자 태그
        public NodeSource(IReadOnlyList<object> items, params string[] tags)
            : this(items, (IEnumerable<string>)tags)
        {
        }

        // 3) 범용: IEnumerable로 받기
        public NodeSource(IReadOnlyList<object> items, IEnumerable<string> tags)
        {
            Items = items ?? throw new ArgumentNullException(nameof(items));
            _tags = tags != null ? new HashSet<string>(tags) : new HashSet<string>();
        }

        public bool HasTag(string tag)
        {
            return _tags != null && _tags.Contains(tag);
        }
    }
}