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
            var disjointSets = new List<List<T>>();

            // Special case: if there is a null set we have to add it first
            if (sets.Any(s => !s.Any()))
            {
                disjointSets.Add(new List<T>());

                sets = sets
                    .Where(s => s.Any()).ToArray();
            }

            foreach (var set in sets)
            {
                var disjointCopy = set.ToList();
                var index = 0;

                while (index < disjointSets.Count())
                {
                    var intersection = disjointSets[index]
                        .Intersect(disjointCopy).ToList();

                    if ((intersection.Count() == disjointCopy.Count())
                        && (intersection.Count() == disjointSets[index].Count())) // They are equal
                    {
                        disjointCopy.Clear();
                        break;
                    }

                    if (intersection.Any())
                    {
                        disjointSets[index] = disjointSets[index]
                            .Except(intersection).ToList();
                        disjointSets.Insert(
                            index: ++index,
                            item: intersection);

                        disjointCopy = disjointCopy
                            .Except(intersection).ToList();
                    }

                    ++index;
                }

                if (disjointCopy.Any())
                {
                    disjointSets.Add(disjointCopy);
                }
            }

            var result = disjointSets
                .Where(s => s.Any()).ToArray();

            return result;
        }

        #endregion Public Methods
    }
}