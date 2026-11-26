using System;
using System.Collections.Generic;
using System.Linq;
using SetExtensions.Models;

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
        /// Missing neighbors (sequence start / end) are represented by default(T).
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
            ArgumentNullException.ThrowIfNull(sequences);

            ArgumentNullException.ThrowIfNull(keySelector);

            // Each inner sequence is enumerated exactly once
            var relevants = sequences
                .Select(s => s?.ToArray())
                .Where(s => s?.Length > 0).ToArray();

            var triplets = relevants
                .GetTriplets(keySelector).ToArray();

            var result = new List<IEnumerable<(T Previous, T Current, T Next)>>();

            var tripletGroups = triplets
                .GroupBy(t => t.CurrKey)
                .Select(g => g.OrderByDescending(c => !c.PrevIsDefault
                    && !c.NextIsDefault)).ToArray();

            // Group by its neighbors
            foreach (var tripletGroup in tripletGroups)
            {
                var clusterResult = GetGroupsByPrevOrNext(
                    triplets: tripletGroup,
                    mergeEnds: mergeEnds);

                result.AddRange(clusterResult);
            }

            return result;
        }

        #endregion Public Methods

        #region Private Methods

        private static List<IEnumerable<(T Previous, T Current, T Next)>> GetGroupsByPrevOrNext<T, TKey>(this
            IEnumerable<Triplet<T, TKey>> triplets, bool mergeEnds)
        {
            // Candidate groups based only on Previous and only on Next
            var prevGroups = triplets.GetGroupsPreviousOnly(mergeEnds).ToArray();
            var nextGroups = triplets.GetGroupsNextOnly(mergeEnds).ToArray();

            var remaining = new HashSet<Triplet<T, TKey>>(triplets);
            var result = new List<IEnumerable<(T Previous, T Current, T Next)>>();

            while (remaining.Count > 0)
            {
                Triplet<T, TKey>[] bestGroup = null;
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

                // Every triplet is part of at least one candidate group, so this is only a safety net
                if (bestGroup is null || bestCount == 0)
                    break;

                var chosen = bestGroup
                    .Where(remaining.Contains).ToArray();

                var contents = chosen
                    .SelectMany(c => c.Contents).ToArray();

                result.Add(contents);

                foreach (var item in chosen)
                {
                    remaining.Remove(item);
                }
            }

            return result;
        }

        private static List<Triplet<T, TKey>[]> GetGroupsNextOnly<T, TKey>(this IEnumerable<Triplet<T, TKey>> triplets,
            bool mergeEnds)
        {
            var groups = new List<List<Triplet<T, TKey>>>();
            var reps = new List<Triplet<T, TKey>>();

            var relevants = triplets.ToArray();

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
                    groups.Add([relevant]);
                    reps.Add(relevant);
                }
                else
                {
                    groups[idx].Add(relevant);
                    var rep = reps[idx];

                    if (rep.NextIsDefault
                        && !relevant.NextIsDefault)
                    {
                        // Keep Previous of the representative, take Next from the relevant triplet
                        reps[idx] = new Triplet<T, TKey>(
                            previous: rep.Previous,
                            current: rep.Current,
                            next: relevant.Next,
                            prevKey: rep.PrevKey,
                            currKey: rep.CurrKey,
                            nextKey: relevant.NextKey,
                            prevIsDefault: rep.PrevIsDefault,
                            nextIsDefault: relevant.NextIsDefault);
                    }
                }
            }

            var result = groups
                .Select(g => g.ToArray()).ToList();

            return result;
        }

        private static List<Triplet<T, TKey>[]> GetGroupsPreviousOnly<T, TKey>(this IEnumerable<Triplet<T, TKey>> triplets,
            bool mergeEnds)
        {
            var groups = new List<List<Triplet<T, TKey>>>();
            var reps = new List<Triplet<T, TKey>>();

            var relevants = triplets.ToArray();

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
                    groups.Add([relevant]);
                    reps.Add(relevant);
                }
                else
                {
                    groups[idx].Add(relevant);
                    var rep = reps[idx];

                    if (rep.PrevIsDefault
                        && !relevant.PrevIsDefault)
                    {
                        // Take Previous from the relevant triplet, keep Next of the representative
                        reps[idx] = new Triplet<T, TKey>(
                            previous: relevant.Previous,
                            current: rep.Current,
                            next: rep.Next,
                            prevKey: relevant.PrevKey,
                            currKey: rep.CurrKey,
                            nextKey: rep.NextKey,
                            prevIsDefault: relevant.PrevIsDefault,
                            nextIsDefault: rep.NextIsDefault);
                    }
                }
            }

            var result = groups
                .Select(g => g.ToArray()).ToList();

            return result;
        }

        private static IEnumerable<Triplet<T, TKey>> GetTriplets<T, TKey>(this T[][] sequences,
            Func<T, TKey> keySelector)
        {
            // The key contains the "neighbor exists" flags, so that a real neighbor
            // whose key equals default(TKey) is not mixed up with a missing neighbor.
            var result = new Dictionary<(bool, TKey, TKey, bool, TKey), Triplet<T, TKey>>();

            foreach (var sequence in sequences)
            {
                for (var index = 0; index < sequence.Length; index++)
                {
                    var prevIsDefault = index == 0;
                    var nextIsDefault = index == sequence.Length - 1;

                    var previous = prevIsDefault
                        ? default
                        : sequence[index - 1];
                    var current = sequence[index];
                    var next = nextIsDefault
                        ? default
                        : sequence[index + 1];

                    var prevKey = prevIsDefault
                        ? default
                        : keySelector(previous);
                    var currKey = keySelector(current);
                    var nextKey = nextIsDefault
                        ? default
                        : keySelector(next);

                    var key = (prevIsDefault, prevKey, currKey, nextIsDefault, nextKey);

                    if (!result.TryGetValue(
                        key: key,
                        value: out var triplet))
                    {
                        triplet = new Triplet<T, TKey>(
                            previous: previous,
                            current: current,
                            next: next,
                            prevKey: prevKey,
                            currKey: currKey,
                            nextKey: nextKey,
                            prevIsDefault: prevIsDefault,
                            nextIsDefault: nextIsDefault);

                        result.Add(
                            key: key,
                            value: triplet);
                    }

                    triplet.Contents.Add((previous, current, next));
                }
            }

            return result.Values;
        }

        private static bool IsCompatibleCrossing<T, TKey>(this Triplet<T, TKey> a, Triplet<T, TKey> b)
        {
            var cmp = EqualityComparer<TKey>.Default;

            // a.prev == b.next ?
            if (!a.PrevIsDefault && !b.NextIsDefault && cmp.Equals(a.PrevKey, b.NextKey))
                return false;

            // b.prev == a.next ?
            if (!b.PrevIsDefault && !a.NextIsDefault && cmp.Equals(b.PrevKey, a.NextKey))
                return false;

            return true;
        }

        private static bool IsCompatibleNextOnly<T, TKey>(this Triplet<T, TKey> a, Triplet<T, TKey> b,
            bool mergeEnds)
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

        private static bool IsCompatiblePreviousOnly<T, TKey>(this Triplet<T, TKey> a, Triplet<T, TKey> b,
            bool mergeEnds)
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

        #endregion Private Methods
    }
}
