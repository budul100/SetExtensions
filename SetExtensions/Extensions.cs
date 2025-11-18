using System;
using System.Collections.Generic;
using System.Linq;

namespace SetExtensions
{
    public static class Extensions
    {
        #region Public Methods

        /// <summary>
        /// Gets the cartesian product of multiple sets. The algorithm was influenced
        /// by https://ericlippert.com/2010/06/28/computing-a-cartesian-product-with-linq/
        /// </summary>
        /// <param name="sequences">A set of the resulting sets.</param>
        /// <returns></returns>
        public static IEnumerable<IEnumerable<T>> CartesianProduct<T>(this IEnumerable<IEnumerable<T>> sequences)
        {
            IEnumerable<IEnumerable<T>> seed = new[] { Enumerable.Empty<T>() };

            var result = sequences.Aggregate(
                seed: seed,
                func: (a, i) => a.GetEnumerabled(i));

            return result;
        }

        /// <summary>
        /// Returns a boolean value if the current enumerable is part of the other enumerable
        /// </summary>
        /// <param name="current">Current set of T</param>
        /// <param name="other">Other set of T</param>
        /// <returns></returns>
        public static bool IsSubSetOf<T>(this IEnumerable<T> current, IEnumerable<T> other)
        {
            var result = false;

            if ((current?.Any() ?? false)
                && (other?.Any() ?? false))
            {
                var currentLookup = current.ToLookup(x => x);
                var otherLookup = other.ToLookup(x => x);

                result = currentLookup
                    .All(e => e.Count() <= otherLookup[e.Key].Count());
            }

            return result;
        }

        /// <summary>
        /// Returns a boolean value if the current enumerable is part of the other enumerable
        /// or if the other enumerable is part of the current enumerable
        /// </summary>
        /// <param name="current">Current set of T</param>
        /// <param name="other">Other set of T</param>
        /// <returns></returns>
        public static bool IsSubSetOfOrOther<T>(this IEnumerable<T> current, IEnumerable<T> other)
        {
            var result = (current?.Any() ?? false)
                && (other?.Any() ?? false)
                && (current.IsSubSetOf(other) || other.IsSubSetOf(current));

            return result;
        }

        /// <summary>
        /// Gets all non-intersecting sets of the given amount sets.
        /// </summary>
        /// <param name="sequences">Given sets of T</param>
        /// <returns>Non-intersecting sets of the given sets</returns>
        public static IEnumerable<IEnumerable<T>> Segmented<T>(this IEnumerable<IEnumerable<T>> sequences)
        {
            if (sequences == default)
            {
                throw new ArgumentNullException(nameof(sequences));
            }

            var result = new HashSet<HashSet<T>>();

            var relevants = sequences
                .GetRelevants()
                .GetUniques().ToArray();

            foreach (var relevant in relevants)
            {
                var index = 0;
                var disjoint = new HashSet<T>(relevant);

                while (index < result.Count)
                {
                    var intersection = new HashSet<T>(result.ElementAt(index));
                    intersection.IntersectWith(disjoint);

                    if ((intersection.Count == disjoint.Count)
                        && (intersection.Count == result.ElementAt(index).Count)) // They are equal
                    {
                        disjoint.Clear();
                        break;
                    }

                    if (intersection.Count > 0)
                    {
                        result.ElementAt(index).ExceptWith(intersection);

                        if (result.ElementAt(index).Count == 0)
                        {
                            result.Remove(result.ElementAt(index));
                        }

                        result.Add(intersection);

                        disjoint.ExceptWith(intersection);
                    }

                    ++index;
                }

                if (disjoint.Count > 0)
                {
                    result.Add(disjoint);
                }
            }

            return result;
        }

        /// <summary>
        /// Builds triplet groups from a set of sequences using a combined
        /// "max-by-size" strategy over Previous-based and Next-based groupings.
        ///
        /// A triplet is (Previous, Current, Next), where:
        /// - Previous is default(T) at the start of a sequence
        /// - Next    is default(T) at the end of a sequence
        ///
        /// Grouping logic:
        /// 1. For each distinct key(current), we consider all triplets with that
        ///    current (cluster).
        /// 2. Inside the cluster, we build:
        ///    - candidate groups based only on Previous
        ///    - candidate groups based only on Next
        ///    (using relaxed matching: equal-by-key OR one side is default(T))
        /// 3. We then repeatedly:
        ///    - find the candidate group (from either side) that has the largest
        ///      overlap with remaining elements in this cluster,
        ///    - emit that overlap as the next result group,
        ///    - remove those elements from the remaining set,
        ///    - repeat until no candidate has overlap > 0.
        /// 4. Any leftover singletons are returned as groups of size 1.
        /// </summary>
        public static IEnumerable<IEnumerable<(T Previous, T Current, T Next)>> ToNeighborGroups<T, TKey>(this
            IEnumerable<IEnumerable<T>> sequences, Func<T, TKey> keySelector)
        {
            if (sequences is null)
            {
                throw new ArgumentNullException(nameof(sequences));
            }

            if (keySelector is null)
            {
                throw new ArgumentNullException(nameof(keySelector));
            }

            var relevants = sequences
                .Where(s => s?.Count() > 0).ToArray();

            var allClusters = new List<(T Previous, T Current, T Next, bool Part)>();

            // 1) Build all triplets from all sequences
            foreach (var relevant in relevants)
            {
                for (var index = 0; index < relevant.Count(); index++)
                {
                    var previous = index > 0
                        ? relevant.ElementAt(index - 1)
                        : default;

                    var current = relevant.ElementAt(index);

                    var next = index < relevant.Count() - 1
                        ? relevant.ElementAt(index + 1)
                        : default;

                    var part = relevant.Count() >= 3
                        && (index == 0 || index == relevant.Count() - 1);

                    allClusters.Add((previous, current, next, part));
                }
            }

            var clusterGroups = new List<IEnumerable<(T Previous, T Current, T Next)>>();

            // 2) Cluster by key(current) to avoid mixing different "center" elements
            var currentGroups = allClusters
                .GroupBy(t => keySelector(t.Current))
                .Select(g => g.OrderByDescending(c => !c.Previous.IsDefault()
                    && !c.Next.IsDefault()))
                .OrderByDescending(g => !g.First().Previous.IsDefault()
                    && !g.First().Next.IsDefault())
                .ThenByDescending(g => g.Count()).ToArray();

            foreach (var currentGroup in currentGroups)
            {
                var clusterResult = GetGroupsByPrevOrNext(
                    cluster: currentGroup,
                    keySelector: keySelector);

                clusterGroups.AddRange(clusterResult);
            }

            var result = clusterGroups
                .GetCleanedGroups().ToArray();

            return result;
        }

        /// <summary>
        /// Gets the values of the given sets as transponded sets.
        /// </summary>
        /// <param name="sets">Given sets of T</param>
        /// <returns>Transponded sets of the given sets</returns>
        public static IEnumerable<IEnumerable<T>> Transponded<T>(this IEnumerable<IEnumerable<T>> sets)
        {
            if (sets == default)
            {
                throw new ArgumentNullException(nameof(sets));
            }

            var result = new List<IEnumerable<T>>();

            // Changed into array due to performance reasons
            var relevants = sets
                .Select(s => s?.ToArray()).ToArray();

            var length = relevants.GetLength();

            for (var index = 0; index < length; index++)
            {
                var currents = relevants
                    .GetTranspondeds(index).ToArray();

                result.Add(currents);
            }

            return result;
        }

        #endregion Public Methods

        #region Private Methods

        private static IEnumerable<IEnumerable<(T Previous, T Current, T Next)>> GetCleanedGroups<T>(this
            IEnumerable<IEnumerable<(T Previous, T Current, T Next)>> groups)
        {
            var ordereds = groups
                .OrderByDescending(g => g.Count()).ToArray();

            var tuples = new HashSet<(T, T)>();

            foreach (var ordered in ordereds)
            {
                var groupeds = ordered
                    .GroupBy(e => (e.Previous, e.Current, e.Next)).ToArray();

                var results = new List<(T Previous, T Current, T Next)>();

                foreach (var grouped in groupeds)
                {
                    if (grouped.Key.Previous.IsDefault())
                    {
                        var tuple = (grouped.Key.Current, grouped.Key.Next);

                        if (!tuples.Contains(tuple))
                        {
                            tuples.Add(tuple);
                            results.AddRange(grouped);
                        }
                    }
                    else if (grouped.Key.Next.IsDefault())
                    {
                        var tuple = (grouped.Key.Previous, grouped.Key.Current);

                        if (!tuples.Contains(tuple))
                        {
                            tuples.Add(tuple);
                            results.AddRange(grouped);
                        }
                    }
                    else
                    {
                        results.AddRange(grouped);
                    }
                }

                if (results.Count > 0)
                {
                    yield return results;
                }
            }
        }

        private static IEnumerable<IEnumerable<T>> GetEnumerabled<T>(this IEnumerable<IEnumerable<T>> accumulator,
            IEnumerable<T> items)
        {
            foreach (var accseq in accumulator)
            {
                foreach (var item in items)
                {
                    yield return accseq.Concat(new[] { item });
                }
            }
        }

        /// <summary>
        /// Combined "max-by-size" strategy:
        /// - Build candidate groups using Previous-only and Next-only rules.
        /// - Always take the candidate with the largest intersection with
        ///   the remaining elements (regardless of whether it is Prev- or Next-based).
        /// - Remove used elements and repeat.
        /// </summary>
        private static IEnumerable<(T Previous, T Current, T Next)>[] GetGroupsByPrevOrNext<T, TKey>(this
            IEnumerable<(T Previous, T Current, T Next, bool Part)> cluster, Func<T, TKey> keySelector)
        {
            // Candidate groups based only on Previous and only on Next
            var prevGroups = cluster.GetGroupsPreviousOnly(keySelector)
                .OrderByDescending(g => g.Count).ToList();

            var nextGroups = cluster.GetGroupsNextOnly(keySelector)
                .OrderByDescending(g => g.Count).ToList();

            var remaining = new HashSet<(T Previous, T Current, T Next, bool Part)>(cluster);
            var groups = new List<List<(T Previous, T Current, T Next, bool Part)>>();

            // As long as we still have unassigned triplets
            while (remaining.Count > 0)
            {
                List<(T Previous, T Current, T Next, bool Part)> bestGroup = null;
                int bestCount = 0;

                // Check all Previous-based groups
                foreach (var g in prevGroups)
                {
                    int count = 0;
                    foreach (var item in g)
                    {
                        if (remaining.Contains(item))
                            count++;
                    }

                    if (count > bestCount)
                    {
                        bestCount = count;
                        bestGroup = g;
                    }
                }

                // Check all Next-based groups
                foreach (var g in nextGroups)
                {
                    int count = 0;
                    foreach (var item in g)
                    {
                        if (remaining.Contains(item))
                            count++;
                    }

                    if (count > bestCount)
                    {
                        bestCount = count;
                        bestGroup = g;
                    }
                }

                // If no candidate group has any overlap left,
                // we stop and handle leftovers as singletons.
                if (bestGroup is null || bestCount == 0)
                {
                    break;
                }

                // Intersection of bestGroup with remaining is the actual group
                var chosen = bestGroup
                    .Where(remaining.Contains)
                    .ToList();

                groups.Add(chosen);

                // Remove chosen elements from remaining
                foreach (var item in chosen)
                {
                    remaining.Remove(item);
                }
            }

            // Any remaining elements that were not picked by any group
            // become singleton groups
            if (remaining.Count > 0)
            {
                foreach (var item in remaining)
                {
                    groups.Add(new List<(T Previous, T Current, T Next, bool Part)> { item });
                }
            }

            var result = groups
                .Select(g => g
                    .Where(c => !c.Part)
                    .Select(c => (c.Previous, c.Current, c.Next)))
                .Where(g => g.Any()).ToArray();

            return result;
        }

        /// <summary>
        /// Builds candidate groups using only Next for compatibility:
        /// - key(Current) must match (enforced by outer clustering).
        /// - Next is compatible if:
        ///     * key(Next) equal OR
        ///     * one side is default(T).
        /// </summary>
        private static List<List<(T Previous, T Current, T Next, bool Part)>> GetGroupsNextOnly<T, TKey>(this
            IEnumerable<(T Previous, T Current, T Next, bool Part)> cluster, Func<T, TKey> keySelector)
        {
            var result = new List<List<(T Previous, T Current, T Next, bool Part)>>();
            var reps = new List<(T Previous, T Current, T Next, bool Part)>(); // representatives

            var relevants = cluster.ToArray();

            foreach (var relevant in relevants)
            {
                var tPrev = relevant.Previous;
                var tCurr = relevant.Current;
                var tNext = relevant.Next;

                int idx = -1;

                for (int i = 0; i < reps.Count; i++)
                {
                    if (reps[i].IsCompatibleNextOnly(
                        other: relevant,
                        keySelector: keySelector))
                    {
                        idx = i;
                        break;
                    }
                }

                if (idx == -1)
                {
                    result.Add(new List<(T, T, T, bool)> { relevant });
                    reps.Add(relevant);
                }
                else
                {
                    result[idx].Add(relevant);

                    var (previous, current, next, part) = reps[idx];

                    if (next.IsDefault() && !tNext.IsDefault())
                    {
                        next = tNext;
                    }

                    reps[idx] = (previous, current, next, part);
                }
            }

            return result;
        }

        /// <summary>
        /// Builds candidate groups using only Previous for compatibility:
        /// - key(Current) must match (enforced by outer clustering).
        /// - Previous is compatible if:
        ///     * key(Previous) equal OR
        ///     * one side is default(T).
        /// </summary>
        private static List<List<(T Previous, T Current, T Next, bool Part)>> GetGroupsPreviousOnly<T, TKey>(this
            IEnumerable<(T Previous, T Current, T Next, bool Part)> cluster, Func<T, TKey> keySelector)
        {
            var result = new List<List<(T Previous, T Current, T Next, bool Part)>>();
            var reps = new List<(T Previous, T Current, T Next, bool Part)>(); // representatives

            var relevants = cluster.ToArray();

            foreach (var relevant in relevants)
            {
                var tPrev = relevant.Previous;
                var tCurr = relevant.Current;
                var tNext = relevant.Next;

                int idx = -1;

                for (int i = 0; i < reps.Count; i++)
                {
                    if (reps[i].IsCompatiblePreviousOnly(
                        other: relevant,
                        keySelector: keySelector))
                    {
                        idx = i;
                        break;
                    }
                }

                if (idx == -1)
                {
                    result.Add(new List<(T, T, T, bool)> { relevant });
                    reps.Add(relevant);
                }
                else
                {
                    result[idx].Add(relevant);

                    var (previous, current, next, part) = reps[idx];

                    if (previous.IsDefault() && !tPrev.IsDefault())
                    {
                        previous = tPrev;
                    }

                    reps[idx] = (previous, current, next, part);
                }
            }

            return result;
        }

        private static int GetLength<T>(this IEnumerable<IEnumerable<T>> sequences)
        {
            var result = 0;

            var relevants = sequences
                .GetRelevants().ToArray();

            foreach (var relevant in relevants)
            {
                result = Math.Max(
                    val1: result,
                    val2: relevant.Count());
            }

            return result;
        }

        private static IEnumerable<IEnumerable<T>> GetRelevants<T>(this IEnumerable<IEnumerable<T>> sequences)
        {
            var result = sequences
                .Where(s => s?.Any() ?? false);

            return result;
        }

        private static IEnumerable<T> GetTranspondeds<T>(this T[][] sequences, int index)
        {
            foreach (var set in sequences)
            {
                var result = index < (set?.Length ?? 0)
                    ? set[index]
                    : default;

                yield return result;
            }
        }

        private static IEnumerable<T>[] GetUniques<T>(this IEnumerable<IEnumerable<T>> sequences)
        {
            var result = sequences
                .Select(s => new HashSet<T>(s))
                .Distinct(HashSet<T>.CreateSetComparer()).ToArray();

            return result;
        }

        private static bool IsCompatibleCrossing<T, TKey>(this (T Previous, T Current, T Next, bool Part) current,
            (T Previous, T Current, T Next, bool Part) other, Func<T, TKey> keySelector)
        {
            if (current.Previous.IsDefault()
                && current.Next.IsDefault()
                && other.Previous.IsDefault()
                && other.Next.IsDefault())
                return true;

            bool comparer(T c, T o) => EqualityComparer<TKey>.Default.Equals(
                x: keySelector(c),
                y: keySelector(o));

            if (!current.Previous.IsDefault()
                && !other.Next.IsDefault()
                && comparer(current.Previous, other.Next))
                return false;

            if (!other.Previous.IsDefault()
                && !current.Next.IsDefault()
                && comparer(other.Previous, current.Next))
                return false;

            return true;
        }

        private static bool IsCompatibleNextOnly<T, TKey>(this (T Previous, T Current, T Next, bool Part) current,
            (T Previous, T Current, T Next, bool Part) other, Func<T, TKey> keySelector)
        {
            bool comparer(T c, T o) => EqualityComparer<TKey>.Default.Equals(
                x: keySelector(c),
                y: keySelector(o));

            if (!current.Next.IsDefault()
                && !other.Next.IsDefault()
                && !comparer(current.Next, other.Next))
                return false;

            if (current.Next.IsDefault()
                && other.Next.IsDefault()
                && !comparer(current.Previous, other.Previous))
                return false;

            if (!current.IsCompatibleCrossing(
                other: other,
                keySelector: keySelector))
                return false;

            if (!comparer(current.Current, other.Current))
                return false;

            return true;
        }

        private static bool IsCompatiblePreviousOnly<T, TKey>(this (T Previous, T Current, T Next, bool Part) current,
            (T Previous, T Current, T Next, bool Part) other, Func<T, TKey> keySelector)
        {
            bool comparer(T c, T o) => EqualityComparer<TKey>.Default.Equals(
                x: keySelector(c),
                y: keySelector(o));

            if (!current.Previous.IsDefault()
                && !other.Previous.IsDefault()
                && !comparer(current.Previous, other.Previous))
                return false;

            if (current.Previous.IsDefault()
                && other.Previous.IsDefault()
                && !comparer(current.Next, other.Next))
                return false;

            if (!current.IsCompatibleCrossing(
                other: other,
                keySelector: keySelector))
                return false;

            if (!comparer(current.Current, other.Current))
                return false;

            return true;
        }

        private static bool IsDefault<T>(this T value) => EqualityComparer<T>.Default.Equals(value, default);

        #endregion Private Methods
    }
}