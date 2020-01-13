using System.Collections.Generic;
using System.Linq;

namespace Extensions
{
    public static class SetExtensions
    {
        #region Public Methods

        public static IEnumerable<IEnumerable<T>> MinimumNonIntersectingSets<T>(this IEnumerable<IEnumerable<T>> sets)
        {
            var disjointSets = new List<List<T>>();

            // Special case: if there is a null set we have to add it first
            if (sets.Any(s => !s.Any()))
            {
                disjointSets.Add(new List<T>());
                sets = sets.Where(s => s.Any());
            }

            foreach (IEnumerable<T> newSet in sets)
            {
                var disjointCopy = newSet.ToList();
                var i = 0;

                while (i < disjointSets.Count())
                {
                    var intersection = disjointSets[i].Intersect(disjointCopy).ToList();

                    if ((intersection.Count() == disjointCopy.Count())
                        && (intersection.Count() == disjointSets[i].Count())) // They are equal
                    {
                        disjointCopy.Clear();
                        break;
                    }
                    if (intersection.Any())
                    {
                        disjointSets[i] = disjointSets[i].Except(intersection).ToList();
                        disjointSets.Insert(++i, intersection);
                        disjointCopy = disjointCopy.Except(intersection).ToList();
                    }

                    ++i;
                }

                if (disjointCopy.Any())
                {
                    disjointSets.Add(disjointCopy);
                }
            }

            return disjointSets
                .Where(s => s.Any()).ToArray();
        }

        #endregion Public Methods
    }
}