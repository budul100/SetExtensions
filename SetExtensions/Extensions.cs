using System;
using System.Collections.Generic;
using System.Linq;

namespace SetExtensions
{
    public static class Extensions
    {
        #region Public Methods

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

            var relevants = GetRelevants(sets);

            var result = new HashSet<HashSet<T>>();

            foreach (var relevant in relevants)
            {
                var index = 0;
                var disjoint = new HashSet<T>(relevant);

                while (index < result.Count())
                {
                    var intersection = new HashSet<T>(result.ElementAt(index));
                    intersection.IntersectWith(disjoint);

                    if ((intersection.Count() == disjoint.Count())
                        && (intersection.Count() == result.ElementAt(index).Count())) // They are equal
                    {
                        disjoint.Clear();
                        break;
                    }

                    if (intersection.Any())
                    {
                        result.ElementAt(index).ExceptWith(intersection);

                        if (!result.ElementAt(index).Any())
                        {
                            result.Remove(result.ElementAt(index));
                        }

                        result.Add(intersection);

                        disjoint.ExceptWith(intersection);
                    }

                    ++index;
                }

                if (disjoint.Any())
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
            var length = sets.GetLength();

            var result = new List<IEnumerable<T>>();

            for (var index = 0; index < length; index++)
            {
                var currents = sets
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
                .Where(s => s?.Any() ?? false).ToArray();

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
                .Where(s => s?.Any() ?? false)
                .Select(s => new HashSet<T>(s))
                .Distinct(HashSet<T>.CreateSetComparer()).ToArray();

            return result;
        }

        private static IEnumerable<T> GetTranspondeds<T>(this IEnumerable<IEnumerable<T>> sets, int index)
        {
            foreach (var set in sets)
            {
                var result = index < (set?.Count() ?? 0)
                    ? set.ElementAt(index)
                    : default;

                yield return result;
            }
        }

        #endregion Private Methods
    }
}