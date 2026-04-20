using System;
using System.Collections;

namespace HELIX.Widgets.Utilities {
    public class DummyCounterList : IList {

        public IEnumerator GetEnumerator() {
            for (var i = 0; i < Count; i++) yield return i;
        }

        public void CopyTo(Array array, int index) {
            throw new NotImplementedException();
        }

        public int Count { get; set; }

        public bool IsSynchronized => false;
        public object SyncRoot => this;

        public int Add(object value) {
            throw new NotImplementedException();
        }

        public void Clear() { }

        public bool Contains(object value) {
            if (value is int i) return i >= 0 && i < Count;

            return false;
        }

        public int IndexOf(object value) {
            if (value is int i && i >= 0 && i < Count) return i;

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
            get => index < 0 || index >= Count ? null : index;
            set => throw new NotImplementedException();
        }

    }
}