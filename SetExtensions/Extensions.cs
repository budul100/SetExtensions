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

            var result = new List<HashSet<T>>();

            foreach (var relevant in relevants)
            {
                var index = 0;
                var disjointCopy = new HashSet<T>(relevant);

                while (index < result.Count())
                {
                    var intersection = new HashSet<T>(result[index]);
                    intersection.IntersectWith(disjointCopy);

                    if ((intersection.Count() == disjointCopy.Count())
                        && (intersection.Count() == result[index].Count())) // They are equal
                    {
                        disjointCopy.Clear();
                        break;
                    }

                    if (intersection.Any())
                    {
                        result[index].ExceptWith(intersection);

                        result.Insert(
                            index: ++index,
                            item: intersection);

                        disjointCopy.ExceptWith(intersection);
                    }

                    ++index;
                }

                if (disjointCopy.Any())
                {
                    result.Add(disjointCopy);
                }
            }

            return result;
        }

        #endregion Public Methods

        #region Private Methods

        private static IEnumerable<IEnumerable<T>> GetRelevants<T>(this IEnumerable<IEnumerable<T>> sets)
        {
            var result = sets
                .Where(s => s?.Any() ?? false)
                .Select(s => new HashSet<T>(s))
                .Distinct(HashSet<T>.CreateSetComparer()).ToArray();

            return result;
        }

        #endregion Private Methods
    }
}