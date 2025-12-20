using System;
using System.Collections.Generic;


namespace Yoru.ChoMiniEngine
{
    public readonly struct NodeSource
    {
        public readonly IReadOnlyList<object> Items;
        private readonly HashSet<string> _tags;

        // ⭐ 추가
        public IEnumerable<string> Tags => _tags;

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