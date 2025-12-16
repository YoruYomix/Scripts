
using System;
using System.Collections.Generic;

namespace Yoru.ChoMiniEngine
{
    public sealed class ChoMiniOptions
    {
        private readonly Dictionary<Type, object> _values = new Dictionary<Type, object>();

        public void Set<T>(T value) where T : notnull
        {
            _values[typeof(T)] = value;
        }

        public bool TryGet<T>(out T value)
        {
            if (_values.TryGetValue(typeof(T), out var obj))
            {
                value = (T)obj;
                return true;
            }

            value = default;
            return false;
        }

        public IEnumerable<KeyValuePair<Type, object>> DebugPairs()
        {
            return _values;
        }

    }
}