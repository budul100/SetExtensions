# SetExtensions

Lightweight C# extension methods for working with collections of sets (`IEnumerable<IEnumerable<T>>`).  
Supports .NET Standard 2.0 / 2.1 and .NET 7 / 8.

## Quick overview

Public extension methods are defined in `SetExtensions.Extensions`:

- `CartesianProduct<T>(this IEnumerable<IEnumerable<T>> sequences)`  
  Produces the Cartesian product of a sequence of sequences. Each yielded item is an `IEnumerable<T>` representing one combination (one element from each input sequence).

- `IsSubSetOf<T>(this IEnumerable<T> current, IEnumerable<T> other)`  
  Tests whether `current` is a multiset subset of `other`. Duplicate counts are respected (uses lookups). Note: the implementation returns `false` if either sequence is null or empty.

- `IsSubSetOfOrOther<T>(this IEnumerable<T> current, IEnumerable<T> other)`  
  Returns true if either `current` is a multiset subset of `other` or vice‑versa. Both sequences must be non‑empty for this method to return `true`.

- `Segmented<T>(this IEnumerable<IEnumerable<T>> sequences)`  
  Splits the provided sets into non‑intersecting segments (disjoint groups). Empty or null inner sequences are ignored. Duplicate groups are collapsed and element multiplicity inside groups is normalized (sets).

- `ToNeighborGroups<T, TKey>(this IEnumerable<IEnumerable<T>> sequences, Func<T, TKey> keySelector, bool mergeEnds = false)`  
  Produces groups of neighbor triplets `(Previous, Current, Next)` from the sequences using a combined strategy that first clusters by `Current` (via `keySelector`) and then groups by best overlaps of previous/next elements. The optional `mergeEnds` parameter (default `false`) controls whether sequence ends may be treated as compatible when comparing neighbors. Missing neighbors are represented by `default(T)`.

- `Transponded<T>(this IEnumerable<IEnumerable<T>> sets)`  
  Transposes the collection of sequences: returns the i-th elements across all input sequences as the i-th output sequence (matrix transpose). If an input sequence is shorter, `default(T)` is used for missing positions. The implementation materializes inner sequences to arrays for performance.

## Small usage examples

```csharp	
using SetExtensions;
// Cartesian product 
var sets = new[] { new[] {1,2}, new[] {3,4} }; 
var product = sets.CartesianProduct(); 
// yields sequences: [1,3], [1,4], [2,3], [2,4]

// Subset 
var a = new[] {1,1,2}; 
var b = new[] {1,1,2,3}; 
var isSubset = a.IsSubSetOf(b); 
// true

// Segmented 
var setsForSeg = new[] { new[] {1,1,2}, new[] {1,1,2,3} }; 
var segments = setsForSeg.Segmented(); 
// yields disjoint groups covering values, e.g. [1,1,2], [3] (order not guaranteed)

// Transpose 
var rows = new[] { new[] { "a","b" }, new[] { "c","d" } }; 
var cols = rows.Transponded(); 
// yields ["a","c"], ["b","d"]

// Neighbor groups (example uses string key) 
var sequences = new[] { new[] { "a", "b" }, new[] { "a", "b", "c" } , new[] { "b", "c" } }; 
var groups = sequences.ToNeighborGroups(s => s); 
// yields [(null, "a", "b"), (null, "a", "b")], [("a", "b", null), ("a", "b", "c"), (null, "b", "c")], [("b", "c", null), ("b, "c", null)]
```

## Notes

License and contribution info: refer to the repository at https://github.com/budul100/SetExtensions.