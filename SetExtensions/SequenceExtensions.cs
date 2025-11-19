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

        #endregion Private Methods
    }
}