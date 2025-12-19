
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

        public IEnumerable<KeyValuePair<Type, object>> GetPairs()
        {
            return _values;
        }

        public bool Has(object key)
        {
            if (key == null)
                return false;

            var keyType = key.GetType();

            if (_values.TryGetValue(keyType, out var value))
            {
                return Equals(value, key);
            }

            return false;
        }


    }
}