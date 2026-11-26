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
        /// Computes the Cartesian product of the input sequences.
        /// </summary>
        /// <typeparam name="T">Element type.</typeparam>
        /// <param name="sequences">Sequences to combine.</param>
        /// <returns>All combinations as sequences of <typeparamref name="T"/>.</returns>
        public static IEnumerable<IEnumerable<T>> CartesianProduct<T>(this IEnumerable<IEnumerable<T>> sequences)
        {
            ArgumentNullException.ThrowIfNull(sequences);

            IEnumerable<IEnumerable<T>> seed = [[]];

            var result = sequences.Aggregate(
                seed: seed,
                func: (a, i) => a.CombineWith(i));

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
            if (current is null
                || other is null)
            {
                return false;
            }

            // Each sequence is enumerated exactly once (an empty lookup means an empty sequence)
            var currentLookup = current.ToLookup(x => x);

            if (currentLookup.Count == 0)
            {
                return false;
            }

            var otherLookup = other.ToLookup(x => x);

            if (otherLookup.Count == 0)
            {
                return false;
            }

            var result = currentLookup
                .All(e => e.Count() <= otherLookup[e.Key].Count());

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
            // Materialized once, because both directions are tested
            var currents = current?.ToArray();
            var others = other?.ToArray();

            var result = currents?.Length > 0
                && others?.Length > 0
                && (currents.IsSubSetOf(others) || others.IsSubSetOf(currents));

            return result;
        }

        /// <summary>
        /// Produces non-overlapping sets from the provided collections (disjoint segmentation).
        /// </summary>
        /// <typeparam name="T">Element type.</typeparam>
        /// <param name="sequences">Input collections.</param>
        /// <param name="comparer">Optional equality comparer for element comparison.</param>
        /// <returns>Non-intersecting sets covering elements from the inputs.</returns>
        public static IEnumerable<IEnumerable<T>> Segmented<T>(this IEnumerable<IEnumerable<T>> sequences,
            IEqualityComparer<T> comparer = null)
        {
            ArgumentNullException.ThrowIfNull(sequences);

            comparer ??= EqualityComparer<T>.Default;

            var result = new List<HashSet<T>>();

            var relevants = sequences
                .GetUniques(comparer);

            foreach (var relevant in relevants)
            {
                var index = 0;
                var disjoint = new HashSet<T>(relevant, comparer);

                while (index < result.Count
                    && disjoint.Count > 0)
                {
                    var intersection = new HashSet<T>(
                        collection: result[index],
                        comparer: comparer);

                    intersection.IntersectWith(disjoint);

                    if (intersection.Count == disjoint.Count
                        && intersection.Count == result[index].Count)
                    {
                        disjoint.Clear();
                        break;
                    }

                    if (intersection.Count > 0)
                    {
                        result[index].ExceptWith(intersection);

                        if (result[index].Count == 0)
                        {
                            result.RemoveAt(index);
                        }
                        else
                        {
                            ++index;
                        }

                        result.Add(intersection);
                        disjoint.ExceptWith(intersection);
                    }
                    else
                    {
                        ++index;
                    }
                }

                if (disjoint.Count > 0)
                {
                    result.Add(disjoint);
                }
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
            ArgumentNullException.ThrowIfNull(sequences);

            var result = new List<IEnumerable<T>>();

            // Changed into array due to performance reasons
            var relevants = sequences
                .Select(s => s?.ToArray()).ToArray();

            var length = relevants.GetMaxLength();

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

        private static IEnumerable<IEnumerable<T>> CombineWith<T>(this IEnumerable<IEnumerable<T>> accumulator,
            IEnumerable<T> items)
        {
            // Materialized once, because it is combined with every accumulated sequence
            var relevants = items.ToArray();

            foreach (var accseq in accumulator)
            {
                foreach (var item in relevants)
                {
                    yield return accseq.Concat([item]);
                }
            }
        }

        private static int GetMaxLength<T>(this T[][] sequences)
        {
            var result = 0;

            foreach (var sequence in sequences)
            {
                if (sequence != null
                    && sequence.Length > result)
                {
                    result = sequence.Length;
                }
            }

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

        private static IEnumerable<T>[] GetUniques<T>(this IEnumerable<IEnumerable<T>> sequences,
            IEqualityComparer<T> comparer)
        {
            // Each inner sequence is enumerated exactly once, null and empty sequences are dropped
            var result = sequences
                .Select(s => s is null ? null : new HashSet<T>(s, comparer))
                .Where(s => s?.Count > 0)
                .Distinct(HashSet<T>.CreateSetComparer()).ToArray();

            return result;
        }

        #endregion Private Methods
    }
}
