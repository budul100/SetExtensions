using System.Collections.Generic;

namespace SetExtensions.Models
{
    /// <summary>
    /// A (Previous, Current, Next) neighbor triplet including the original occurrences it represents.
    /// PrevIsDefault / NextIsDefault are true if the neighbor does not exist (sequence start / end).
    /// </summary>
    internal sealed class Triplet<T, TKey>(T previous, T current, T next, TKey prevKey, TKey currKey, TKey nextKey,
        bool prevIsDefault, bool nextIsDefault)
    {
        #region Public Properties

        public List<(T, T, T)> Contents { get; } = [];

        public T Current { get; } = current;

        public TKey CurrKey { get; } = currKey;

        public T Next { get; } = next;

        public bool NextIsDefault { get; } = nextIsDefault;

        public TKey NextKey { get; } = nextKey;

        public T Previous { get; } = previous;

        public bool PrevIsDefault { get; } = prevIsDefault;

        public TKey PrevKey { get; } = prevKey;

        #endregion Public Properties
    }
}
