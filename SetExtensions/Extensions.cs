using System;
using System.Collections.Generic;
using System.Linq;

namespace SetExtensions
{
    public static class Extensions
    {
        #region Public Methods

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
        /// <param name="sets">Given sets of T</param>
        /// <returns>Non-intersecting sets of the given sets</returns>
        public static IEnumerable<IEnumerable<T>> Segmented<T>(this IEnumerable<IEnumerable<T>> sets)
        {
            if (sets == default)
            {
                throw new ArgumentNullException(nameof(sets));
            }

            var result = new HashSet<HashSet<T>>();

            var relevants = sets
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

        private static int GetLength<T>(this IEnumerable<IEnumerable<T>> sets)
        {
            var result = 0;

            var relevants = sets
                .GetRelevants().ToArray();

            foreach (var relevant in relevants)
            {
                result = Math.Max(
                    val1: result,
                    val2: relevant.Count());
            }

            return result;
        }

        private static IEnumerable<IEnumerable<T>> GetRelevants<T>(this IEnumerable<IEnumerable<T>> sets)
        {
            var result = sets
                .Where(s => s?.Any() ?? false);

            return result;
        }

        private static IEnumerable<T> GetTranspondeds<T>(this T[][] sets, int index)
        {
            foreach (var set in sets)
            {
                var result = index < (set?.Length ?? 0)
                    ? set[index]
                    : default;

                yield return result;
            }
        }

        private static IEnumerable<IEnumerable<T>> GetUniques<T>(this IEnumerable<IEnumerable<T>> sets)
        {
            var result = sets
                .Select(s => new HashSet<T>(s))
                .Distinct(HashSet<T>.CreateSetComparer()).ToArray();

            return result;
        }

        #endregion Private Methods
    }
}