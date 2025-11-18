using System;
using System.Collections.Generic;
using System.Linq;

namespace SetExtensions
{
    /// <summary>
    /// Provides extension methods for sequence operations such as Cartesian product,
    /// segmentation, neighbor grouping and transposition helpers.
    /// </summary>
    public static class Extensions
    {
        #region Public Methods

        /// <summary>
        /// Computes the Cartesian product of the input sequences.
        /// </summary>
        /// <typeparam name="T">Element type.</typeparam>
        /// <param name="sequences">Sequences to combine.</param>
        /// <returns>All combinations as sequences of <typeparamref name="T"/>.</returns>
        public static IEnumerable<IEnumerable<T>> CartesianProduct<T>(this IEnumerable<IEnumerable<T>> sequences)
        {
            IEnumerable<IEnumerable<T>> seed = new[] { Enumerable.Empty<T>() };

            var result = sequences.Aggregate(
                seed: seed,
                func: (a, i) => a.GetEnumerabled(i));

            return result;
        }

        /// <summary>
        /// Determines whether all elements of <paramref name="current"/> appear in <paramref name="other"/>,
        /// respecting element multiplicity.
        /// </summary>
        /// <typeparam name="T">Element type.</typeparam>
        /// <param name="current">Sequence to test for containment.</param>
        /// <param name="other">Sequence to test against.</param>
        /// <returns>True if <paramref name="current"/> is a multiset subset of <paramref name="other"/>.</returns>
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
        /// Determines whether one sequence is a multiset subset of the other (either direction).
        /// </summary>
        /// <typeparam name="T">Element type.</typeparam>
        /// <param name="current">First sequence.</param>
        /// <param name="other">Second sequence.</param>
        /// <returns>True if either sequence is a multiset subset of the other.</returns>
        public static bool IsSubSetOfOrOther<T>(this IEnumerable<T> current, IEnumerable<T> other)
        {
            var result = (current?.Any() ?? false)
                && (other?.Any() ?? false)
                && (current.IsSubSetOf(other) || other.IsSubSetOf(current));

            return result;
        }

        /// <summary>
        /// Produces non-overlapping sets from the provided collections (disjoint segmentation).
        /// </summary>
        /// <typeparam name="T">Element type.</typeparam>
        /// <param name="sequences">Input collections.</param>
        /// <returns>Non-intersecting sets covering elements from the inputs.</returns>
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

            var relevants = sequences
                .Where(s => s?.Count() > 0).ToArray();

            var allClusters = new List<(T Previous, T Current, T Next)>();

            // Build all triplets from all sequences
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

                    allClusters.Add((previous, current, next));
                }
            }

            var result = new List<IEnumerable<(T Previous, T Current, T Next)>>();

            // Cluster by key(current) to avoid mixing different "center" elements
            var currentGroups = allClusters
                .GroupBy(t => keySelector(t.Current))
                .Select(g => g.OrderByDescending(c => !c.Previous.IsDefault()
                    && !c.Next.IsDefault())).ToArray();

            // Group by its neighbors
            foreach (var currentGroup in currentGroups)
            {
                var clusterResult = GetGroupsByPrevOrNext(
                    cluster: currentGroup,
                    keySelector: keySelector,
                    mergeEnds: mergeEnds);

                result.AddRange(clusterResult);
            }

            return result;
        }

        /// <summary>
        /// Transposes the collection-of-collections: converts rows to columns.
        /// Missing elements are returned as default(T).
        /// </summary>
        /// <typeparam name="T">Element type.</typeparam>
        /// <param name="sequences">Input sequences.</param>
        /// <returns>Transposed collections of elements.</returns>
        public static IEnumerable<IEnumerable<T>> Transponded<T>(this IEnumerable<IEnumerable<T>> sequences)
        {
            if (sequences == default)
            {
                throw new ArgumentNullException(nameof(sequences));
            }

            var result = new List<IEnumerable<T>>();

            // Changed into array due to performance reasons
            var relevants = sequences
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

        private static List<List<(T Previous, T Current, T Next)>> GetGroupsByPrevOrNext<T, TKey>(this
            IEnumerable<(T Previous, T Current, T Next)> cluster, Func<T, TKey> keySelector, bool mergeEnds)
        {
            // Candidate groups based only on Previous and only on Next
            var prevGroups = cluster.GetGroupsPreviousOnly(
                keySelector: keySelector,
                mergeEnds: mergeEnds).ToArray();
            var nextGroups = cluster.GetGroupsNextOnly(
                keySelector: keySelector,
                mergeEnds: mergeEnds).ToArray();

            var remaining = new HashSet<(T Previous, T Current, T Next)>(cluster);
            var result = new List<List<(T Previous, T Current, T Next)>>();

            // As long as we still have unassigned triplets
            while (remaining.Count > 0)
            {
                List<(T Previous, T Current, T Next)> bestGroup = null;
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
                    .Where(remaining.Contains).ToList();

                result.Add(chosen);

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
                    result.Add(new List<(T Previous, T Current, T Next)> { item });
                }
            }

            return result;
        }

        private static List<List<(T Previous, T Current, T Next)>> GetGroupsNextOnly<T, TKey>(this
            IEnumerable<(T Previous, T Current, T Next)> cluster, Func<T, TKey> keySelector, bool mergeEnds)
        {
            var result = new List<List<(T Previous, T Current, T Next)>>();
            var reps = new List<(T Previous, T Current, T Next)>(); // representatives

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
                        keySelector: keySelector,
                        mergeEnds: mergeEnds))
                    {
                        idx = i;
                        break;
                    }
                }

                if (idx == -1)
                {
                    result.Add(new List<(T, T, T)> { relevant });
                    reps.Add(relevant);
                }
                else
                {
                    result[idx].Add(relevant);

                    var (previous, current, next) = reps[idx];

                    if (next.IsDefault() && !tNext.IsDefault())
                    {
                        next = tNext;
                    }

                    reps[idx] = (previous, current, next);
                }
            }

            return result;
        }

        private static List<List<(T Previous, T Current, T Next)>> GetGroupsPreviousOnly<T, TKey>(this
            IEnumerable<(T Previous, T Current, T Next)> cluster, Func<T, TKey> keySelector, bool mergeEnds)
        {
            var result = new List<List<(T Previous, T Current, T Next)>>();
            var reps = new List<(T Previous, T Current, T Next)>(); // representatives

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
                        keySelector: keySelector,
                        mergeEnds: mergeEnds))
                    {
                        idx = i;
                        break;
                    }
                }

                if (idx == -1)
                {
                    result.Add(new List<(T, T, T)> { relevant });
                    reps.Add(relevant);
                }
                else
                {
                    result[idx].Add(relevant);

                    var (previous, current, next) = reps[idx];

                    if (previous.IsDefault() && !tPrev.IsDefault())
                    {
                        previous = tPrev;
                    }

                    reps[idx] = (previous, current, next);
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

        private static bool IsCompatibleCrossing<T, TKey>(this (T Previous, T Current, T Next) current,
            (T Previous, T Current, T Next) other, Func<T, TKey> keySelector)
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

        private static bool IsCompatibleNextOnly<T, TKey>(this (T Previous, T Current, T Next) current,
            (T Previous, T Current, T Next) other, Func<T, TKey> keySelector, bool mergeEnds)
        {
            bool comparer(T c, T o) => EqualityComparer<TKey>.Default.Equals(
                x: keySelector(c),
                y: keySelector(o));

            if (!current.Next.IsDefault()
                && !other.Next.IsDefault()
                && !comparer(current.Next, other.Next))
                return false;

            if (!mergeEnds
                && !current.Previous.IsDefault()
                && !other.Previous.IsDefault()
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

        private static bool IsCompatiblePreviousOnly<T, TKey>(this (T Previous, T Current, T Next) current,
            (T Previous, T Current, T Next) other, Func<T, TKey> keySelector, bool mergeEnds)
        {
            bool comparer(T c, T o) => EqualityComparer<TKey>.Default.Equals(
                x: keySelector(c),
                y: keySelector(o));

            if (!current.Previous.IsDefault()
                && !other.Previous.IsDefault()
                && !comparer(current.Previous, other.Previous))
                return false;

            if (!mergeEnds
                && !current.Next.IsDefault()
                && !other.Next.IsDefault()
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