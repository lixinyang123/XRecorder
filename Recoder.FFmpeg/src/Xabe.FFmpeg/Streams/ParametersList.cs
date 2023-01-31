﻿using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Xabe.FFmpeg.Streams
{
    internal class ParametersList<T> : IEnumerable<T>
    {
        private readonly Dictionary<T, T> _items = new Dictionary<T, T>();

        public IEnumerator<T> GetEnumerator()
        {
            return _items.Select(x => x.Value).GetEnumerator();
        }
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        internal void Add(T item)
        {
            _items[item] = item;
        }

        internal void Remove(T item)
        {
            if (_items.ContainsKey(item))
            {
                _items.Remove(item);
            }
        }

        internal void Clear()
        {
            _items.Clear();
        }
    }
}
