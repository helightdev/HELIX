using System.Collections.Generic;

namespace HELIX.Widgets.Utilities {
    public class IdentityDictionary<TKey, TValue> : Dictionary<TKey, TValue> where TKey : class {

        public IdentityDictionary() : base(new ReferenceEqualityComparer<TKey>()) { }

    }
}