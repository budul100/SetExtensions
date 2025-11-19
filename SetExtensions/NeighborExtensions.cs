using System;
using System.Collections.Generic;
using System.Linq;

namespace SetExtensions
{
    /// <summary>
    /// Provides extension methods for sequence operations such as Cartesian product,
    /// segmentation, neighbor grouping and transposition helpers.
    /// </summary>
    public static partial class Extensions
    {
        #region Public Methods

        /// <summary>
        /// Build triplet groups (Previous, Current, Next) from sequences, cluster by key and
        /// produce neighbor groups using a max-overlap strategy from Previous- and Next-based candidates.
        /// </summary>
        /// <typeparam name="T">Element type.</typeparam>
        /// <typeparam name="TKey">Key type used for grouping.</typeparam>
        /// <param name="sequences">Input sequences.</param>
        /// <param name="keySelector">Function to extract the grouping key from an element.</param>
        /// <param name="mergeEnds">If true, consider sequence ends as mergeable when comparing neighbors.</param>
        /// <returns>Collections of triplet groups (Previous, Current, Next).</returns>
        public static IEnumerable<IEnumerable<(T Previous, T Current, T Next)>> ToNeighborGroups<T, TKey>(this
            IEnumerable<IEnumerable<T>> sequences, Func<T, TKey> keySelector, bool mergeEnds = false)
        {
            if (sequences is null)
            {
                throw new ArgumentNullException(nameof(sequences));
            }

            if (keySelector is null)
            {
                throw new ArgumentNullException(nameof(keySelector));
            }

            var triplets = sequences
                .Where(s => s?.Count() > 0)
                .GetTriplets(keySelector).ToArray();

            var result = new List<IEnumerable<(T Previous, T Current, T Next)>>();

            var currentGroups = triplets
                .GroupBy(t => t.CurrKey)
                .Select(g => g.OrderByDescending(c => !c.PrevIsDefault && !c.NextIsDefault)).ToArray();

            // Group by its neighbors
            foreach (var currentGroup in currentGroups)
            {
                var clusterResult = GetGroupsByPrevOrNext(
                    cluster: currentGroup,
                    mergeEnds: mergeEnds);

                result.AddRange(clusterResult);
            }

            return result;
        }

        #endregion Public Methods

        #region Private Methods

        private static List<List<(T Previous, T Current, T Next)>> GetGroupsByPrevOrNext<T, TKey>(this
            IEnumerable<TripletWithKeys<T, TKey>> cluster, bool mergeEnds)
        {
            // Candidate groups based only on Previous and only on Next
            var prevGroups = cluster.GetGroupsPreviousOnly(mergeEnds).ToArray();
            var nextGroups = cluster.GetGroupsNextOnly(mergeEnds).ToArray();

            var remaining = new HashSet<TripletWithKeys<T, TKey>>(cluster);
            var result = new List<List<(T Previous, T Current, T Next)>>();

            while (remaining.Count > 0)
            {
                TripletWithKeys<T, TKey>[] bestGroup = null;
                var bestCount = 0;

                foreach (var prevGroup in prevGroups)
                {
                    var count = 0;

                    foreach (var item in prevGroup)
                    {
                        if (remaining.Contains(item))
                            count++;
                    }

                    if (count > bestCount)
                    {
                        bestCount = count;
                        bestGroup = prevGroup;
                    }
                }

                foreach (var nextGroup in nextGroups)
                {
                    var count = 0;

                    foreach (var item in nextGroup)
                    {
                        if (remaining.Contains(item))
                            count++;
                    }

                    if (count > bestCount)
                    {
                        bestCount = count;
                        bestGroup = nextGroup;
                    }
                }

                if (bestGroup is null || bestCount == 0)
                    break;

                var chosen = bestGroup
                    .Where(remaining.Contains).ToArray();

                // map back to plain (T,T,T) for external result
                result.Add(chosen
                    .Select(t => (t.Previous, t.Current, t.Next)).ToList());

                foreach (var item in chosen)
                {
                    remaining.Remove(item);
                }
            }

            if (remaining.Count > 0)
            {
                foreach (var item in remaining)
                {
                    result.Add(new List<(T Previous, T Current, T Next)>
                    {
                        (item.Previous, item.Current, item.Next)
                    });
                }
            }

            return result;
        }

        private static List<TripletWithKeys<T, TKey>[]> GetGroupsNextOnly<T, TKey>(this
            IEnumerable<TripletWithKeys<T, TKey>> cluster, bool mergeEnds)
        {
            var relevants = cluster.ToArray();
            var result = new List<TripletWithKeys<T, TKey>[]>();
            var reps = new List<TripletWithKeys<T, TKey>>(); // representatives

            foreach (var relevant in relevants)
            {
                var idx = -1;

                for (var index = 0; index < reps.Count; index++)
                {
                    if (IsCompatibleNextOnly(
                        a: reps[index],
                        b: relevant,
                        mergeEnds: mergeEnds))
                    {
                        idx = index;
                        break;
                    }
                }

                if (idx == -1)
                {
                    result.Add(new[] { relevant });
                    reps.Add(relevant);
                }
                else
                {
                    var list = result[idx].ToList();
                    list.Add(relevant);
                    result[idx] = list.ToArray();

                    // refine representative: if Next is default and relevant has non-default,
                    // use the more specific one
                    var rep = reps[idx];
                    if (rep.NextIsDefault && !relevant.NextIsDefault)
                    {
                        rep = new TripletWithKeys<T, TKey>(
                            previous: rep.Previous,
                            current: rep.Current,
                            next: relevant.Next,
                            prevKey: rep.PrevKey,
                            currKey: rep.CurrKey,
                            nextKey: relevant.NextKey,
                            prevIsDefault: rep.PrevIsDefault,
                            nextIsDefault: relevant.NextIsDefault);

                        reps[idx] = rep;
                    }
                }
            }

            return result;
        }

        private static List<TripletWithKeys<T, TKey>[]> GetGroupsPreviousOnly<T, TKey>(this
            IEnumerable<TripletWithKeys<T, TKey>> cluster, bool mergeEnds)
        {
            var relevants = cluster.ToArray();
            var result = new List<TripletWithKeys<T, TKey>[]>();
            var reps = new List<TripletWithKeys<T, TKey>>(); // representatives

            foreach (var relevant in relevants)
            {
                var idx = -1;

                for (var index = 0; index < reps.Count; index++)
                {
                    if (IsCompatiblePreviousOnly(
                        a: reps[index],
                        b: relevant,
                        mergeEnds: mergeEnds))
                    {
                        idx = index;
                        break;
                    }
                }

                if (idx == -1)
                {
                    result.Add(new[] { relevant });
                    reps.Add(relevant);
                }
                else
                {
                    var list = result[idx].ToList();
                    list.Add(relevant);
                    result[idx] = list.ToArray();

                    // refine representative: if Prev is default and relevant has non-default,
                    // use the more specific one
                    var rep = reps[idx];
                    if (rep.PrevIsDefault && !relevant.PrevIsDefault)
                    {
                        rep = new TripletWithKeys<T, TKey>(
                            previous: relevant.Previous,
                            current: rep.Current,
                            next: rep.Next,
                            prevKey: relevant.PrevKey,
                            currKey: rep.CurrKey,
                            nextKey: rep.NextKey,
                            prevIsDefault: relevant.PrevIsDefault,
                            nextIsDefault: rep.NextIsDefault);

                        reps[idx] = rep;
                    }
                }
            }

            return result;
        }

        private static IEnumerable<TripletWithKeys<T, TKey>> GetTriplets<T, TKey>(this
            IEnumerable<IEnumerable<T>> sequences, Func<T, TKey> keySelector)
        {
            foreach (var sequence in sequences)
            {
                for (var index = 0; index < sequence.Count(); index++)
                {
                    var previous = index > 0
                        ? sequence.ElementAt(index - 1)
                        : default;

                    var current = sequence.ElementAt(index);

                    var next = index < sequence.Count() - 1
                        ? sequence.ElementAt(index + 1)
                        : default;

                    bool prevIsDefault = previous.IsDefault();
                    bool nextIsDefault = next.IsDefault();

                    var prevKey = prevIsDefault ? default : keySelector(previous);
                    var currKey = keySelector(current);
                    var nextKey = nextIsDefault ? default : keySelector(next);

                    yield return new TripletWithKeys<T, TKey>(
                        previous: previous,
                        current: current,
                        next: next,
                        prevKey: prevKey,
                        currKey: currKey,
                        nextKey: nextKey,
                        prevIsDefault: prevIsDefault,
                        nextIsDefault: nextIsDefault);
                }
            }
        }

        private static bool IsCompatibleCrossing<T, TKey>(this TripletWithKeys<T, TKey> a,
            TripletWithKeys<T, TKey> b)
        {
            var cmp = EqualityComparer<TKey>.Default;

            // trivial case: everything default -> no crossing
            if (a.PrevIsDefault && a.NextIsDefault && b.PrevIsDefault && b.NextIsDefault)
                return true;

            // a.prev == b.next ?
            if (!a.PrevIsDefault && !b.NextIsDefault && cmp.Equals(a.PrevKey, b.NextKey))
                return false;

            // b.prev == a.next ?
            if (!b.PrevIsDefault && !a.NextIsDefault && cmp.Equals(b.PrevKey, a.NextKey))
                return false;

            return true;
        }

        private static bool IsCompatibleNextOnly<T, TKey>(this TripletWithKeys<T, TKey> a,
            TripletWithKeys<T, TKey> b, bool mergeEnds)
        {
            var cmp = EqualityComparer<TKey>.Default;

            // Current must match by key
            if (!cmp.Equals(a.CurrKey, b.CurrKey))
                return false;

            // Next: equal by key OR one side is default
            bool nextOk = a.NextIsDefault
                || b.NextIsDefault
                || cmp.Equals(a.NextKey, b.NextKey);

            if (!nextOk)
                return false;

            // If mergeEnds == false, Previous must also be compatible:
            if (!mergeEnds
                && !a.PrevIsDefault
                && !b.PrevIsDefault
                && !cmp.Equals(a.PrevKey, b.PrevKey))
                return false;

            // no crossing: prev <-> next
            if (!IsCompatibleCrossing(a, b))
                return false;

            return true;
        }

        private static bool IsCompatiblePreviousOnly<T, TKey>(this TripletWithKeys<T, TKey> a,
            TripletWithKeys<T, TKey> b, bool mergeEnds)
        {
            var cmp = EqualityComparer<TKey>.Default;

            // Current must match by key
            if (!cmp.Equals(a.CurrKey, b.CurrKey))
                return false;

            // Previous: equal by key OR one side is default
            bool prevOk = a.PrevIsDefault
                || b.PrevIsDefault
                || cmp.Equals(a.PrevKey, b.PrevKey);

            if (!prevOk)
                return false;

            // If mergeEnds == false, Next must also be compatible:
            if (!mergeEnds
                && !a.NextIsDefault
                && !b.NextIsDefault
                && !cmp.Equals(a.NextKey, b.NextKey))
                return false;

            // no crossing: prev <-> next
            if (!IsCompatibleCrossing(a, b))
                return false;

            return true;
        }

        private static bool IsDefault<T>(this T value) => EqualityComparer<T>.Default.Equals(value, default);

        #endregion Private Methods

        #region Private Structs

        private readonly struct TripletWithKeys<T, TKey>
        {
            #region Public Constructors

            public TripletWithKeys(T previous, T current, T next, TKey prevKey, TKey currKey, TKey nextKey,
                bool prevIsDefault, bool nextIsDefault)
            {
                Previous = previous;
                Current = current;
                Next = next;
                PrevKey = prevKey;
                CurrKey = currKey;
                NextKey = nextKey;
                PrevIsDefault = prevIsDefault;
                NextIsDefault = nextIsDefault;
            }

            #endregion Public Constructors

            #region Public Properties

            public T Current { get; }

            public TKey CurrKey { get; }

            public T Next { get; }

            public bool NextIsDefault { get; }

            public TKey NextKey { get; }

            public T Previous { get; }

            public bool PrevIsDefault { get; }

            public TKey PrevKey { get; }

            #endregion Public Properties
        }

        #endregion Private Structs
    }
}