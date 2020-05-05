using System.Collections.Generic;
using System.Linq;

namespace SetExtensions
{
    public static class Extensions
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

        public static IEnumerable<IEnumerable<T>> Segmented<T>(this IEnumerable<IEnumerable<T>> sets)
        {
            var given = sets?
                .SelectMany(g => g)
                .Where(g => !g.IsDefault()).ToArray();

            while (given?.Any() ?? false)
            {
                given = given
                    .OrderBy(b => b)
                    .ToArray();

                var result = sets.GetGroups(given)
                    .Distinct().ToArray();

                yield return result;

                given = given
                    .Except(result)
                    .ToArray();
            }
        }

        #endregion Public Methods

        #region Private Methods

        private static IEnumerable<T> GetGroups<T>(this IEnumerable<IEnumerable<T>> allGroups, IEnumerable<T> items)
        {
            var givenGroups = allGroups
                .Where(g => g.Contains(items.First()))
                .ToArray();

            foreach (var item in items)
            {
                var newGroups = allGroups
                    .Where(g => g.Contains(item)).ToArray();

                if (newGroups.Count() == givenGroups.Count())
                {
                    var index = 0;
                    while (index < newGroups.Count())
                    {
                        if (!newGroups[index].SequenceEqual(givenGroups[index]))
                        {
                            break;
                        }

                        index++;
                    }

                    if (index == newGroups.Count())
                    {
                        yield return item;
                    }
                }
            }
        }

        private static bool IsDefault<T>(this T x)
        {
            return EqualityComparer<T>.Default.Equals(
                x: x,
                y: default);
        }

        #endregion Private Methods
    }
}