using System;
using System.Collections;

namespace HELIX.Widgets.Utilities {
    public class DummyCounterList : IList {
        private int _count;

        public IEnumerator GetEnumerator() {
            for (var i = 0; i < _count; i++) { yield return i; }
        }

        public void CopyTo(Array array, int index) {
            throw new NotImplementedException();
        }

        public int Count {
            get => _count;
            set => _count = value;
        }

        public bool IsSynchronized => false;
        public object SyncRoot => this;

        public int Add(object value) {
            throw new NotImplementedException();
        }

        public void Clear() { }

        public bool Contains(object value) {
            if (value is int i) { return i >= 0 && i < _count; }

            return false;
        }

        public int IndexOf(object value) {
            if (value is int i && i >= 0 && i < _count) { return i; }

            return -1;
        }

        public void Insert(int index, object value) {
            throw new NotImplementedException();
        }

        public void Remove(object value) {
            throw new NotImplementedException();
        }

        public void RemoveAt(int index) {
            throw new NotImplementedException();
        }

        public bool IsFixedSize => false;
        public bool IsReadOnly => true;

        public object this[int index] {
            get => index < 0 || index >= _count ? null : index;
            set => throw new NotImplementedException();
        }
    }
}