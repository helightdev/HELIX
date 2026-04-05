using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace HELIX.Widgets.Utilities {
    public class IndexedReferencePriorityQueue<TElement, TPriority>
        where TPriority : IComparable<TPriority> where TElement : class {
        private readonly List<(TElement Element, TPriority Priority)> _heap = new();
        private readonly Dictionary<TElement, int> _elementIndices = new(new ReferenceEqualityComparer<TElement>());

        public int Count => _heap.Count;

        public bool Contains(TElement element) => _elementIndices.ContainsKey(element);

        public bool Enqueue(TElement element, TPriority priority) {
            if (Contains(element)) return false;

            _elementIndices[element] = _heap.Count;
            _heap.Add((element, priority));
            BubbleUp(_heap.Count - 1);
            return true;
        }

        public TElement Dequeue() {
            if (_heap.Count == 0) throw new InvalidOperationException("Queue is empty.");
            var result = _heap[0].Element;
            RemoveAt(0);
            return result;
        }

        public bool TryDequeue(out TElement element) {
            element = null;
            if (_heap.Count == 0) return false;
            element = _heap[0].Element;
            RemoveAt(0);
            return true;
        }

        public bool Remove(TElement element) {
            if (!_elementIndices.TryGetValue(element, out var index)) return false;
            RemoveAt(index);
            return true;
        }

        private void RemoveAt(int index) {
            var lastIndex = _heap.Count - 1;
            var elementToRemove = _heap[index].Element;

            if (index < lastIndex) {
                Swap(index, lastIndex);
                _elementIndices.Remove(elementToRemove);
                _heap.RemoveAt(lastIndex);

                // One of these will maintain the heap property
                if (!BubbleDown(index)) { BubbleUp(index); }
            } else {
                _elementIndices.Remove(elementToRemove);
                _heap.RemoveAt(lastIndex);
            }
        }

        private void BubbleUp(int index) {
            while (index > 0) {
                var parent = (index - 1) / 2;
                if (_heap[index].Priority.CompareTo(_heap[parent].Priority) >= 0) break;
                Swap(index, parent);
                index = parent;
            }
        }

        private bool BubbleDown(int index) {
            var moved = false;
            while (true) {
                var left = 2 * index + 1;
                var right = 2 * index + 2;
                var smallest = index;

                if (left < _heap.Count && _heap[left].Priority.CompareTo(_heap[smallest].Priority) < 0) smallest = left;
                if (right < _heap.Count && _heap[right].Priority.CompareTo(_heap[smallest].Priority) < 0)
                    smallest = right;

                if (smallest == index) break;

                Swap(index, smallest);
                index = smallest;
                moved = true;
            }

            return moved;
        }

        private void Swap(int i, int j) {
            (_heap[i], _heap[j]) = (_heap[j], _heap[i]);

            // Update the hash-map with the new indices
            _elementIndices[_heap[i].Element] = i;
            _elementIndices[_heap[j].Element] = j;
        }
    }

    public class ReferenceEqualityComparer<T> : IEqualityComparer<T> where T : class {
        bool IEqualityComparer<T>.Equals(T x, T y) => ReferenceEquals(x, y);
        int IEqualityComparer<T>.GetHashCode(T obj) => RuntimeHelpers.GetHashCode(obj);
    }
}