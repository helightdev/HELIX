using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HELIX.Widgets.Visual {
    public class RingBuffer<T> : IReadOnlyList<T> {
        private readonly T[] _buffer;
        private int _index;

        public int Count { get; private set; }
        public int Capacity => _buffer.Length;

        public RingBuffer(int capacity) {
            _buffer = new T[capacity];
        }

        public void Add(T item) {
            _buffer[_index] = item;
            _index = (_index + 1) % _buffer.Length;
            Count = Mathf.Min(Count + 1, _buffer.Length);
        }

        public T Get(int i) {
            // 0 = oldest, Count-1 = newest
            var pos = (_index - Count + i + _buffer.Length) % _buffer.Length;
            return _buffer[pos];
        }

        public IEnumerator<T> GetEnumerator() {
            for (var i = 0; i < Count; i++) yield return Get(i);
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public T this[int index] => Get(index);
    }
}