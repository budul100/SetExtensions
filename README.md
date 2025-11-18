# SetExtensions

Lightweight C# extension methods for working with collections of sets (IEnumerable<IEnumerable<T>>).  
Supports .NET Standard 2.0/2.1 and .NET 7/8.

## Quick overview

The library provides a set of public extension methods defined in `SetExtensions.Extensions`:

- `CartesianProduct<T>(this IEnumerable<IEnumerable<T>> sequences)`  
  Produces the Cartesian product of a sequence of sequences. Each yielded item is an `IEnumerable<T>` representing one combination (one element from each input sequence).

- `IsSubSetOf<T>(this IEnumerable<T> current, IEnumerable<T> other)`  
  Tests whether `current` is a multiset subset of `other`. Duplicate counts are respected (uses lookups), so `{1,1}` is not a subset of `{1}`.

- `IsSubSetOfOrOther<T>(this IEnumerable<T> current, IEnumerable<T> other)`  
  Returns true if either `current` is a multiset subset of `other` or vice‑versa.

- `Segmented<T>(this IEnumerable<IEnumerable<T>> sequences)`  
  Splits the provided sets into non‑intersecting segments (disjoint groups). Useful to reduce overlapping sets into disjoint partitions.

- `ToNeighborGroups<T, TKey>(this IEnumerable<IEnumerable<T>> sequences, Func<T, TKey> keySelector)`  
  Produces groups of neighbor triplets `(Previous, Current, Next)` from the sequences using a combined strategy that clusters by `Current` (via `keySelector`) and groups by best overlaps of previous/next elements. Useful for sequence-context grouping where you care about neighbor relations.

- `Transponded<T>(this IEnumerable<IEnumerable<T>> sets)`  
  Transposes the collection of sequences: returns the i-th elements across all input sequences as the i-th output sequence (similar to matrix transpose). If an input sequence is shorter, `default(T)` is used for missing positions.

## Small usage examples

```csharp	
using SetExtensions;

// Cartesian product 
var sets = new[] { new[] {1,2}, new[] {3,4} }; 
var product = sets.CartesianProduct(); 
// yields [1,3], [1,4], [2,3], [2,4]

// Subset
var a = new[] {1,1,2}; 
var b = new[] {1,1,2,3}; 
bool isSubset = a.IsSubSetOf(b); 
// true

// Segmented
var sets = new[] { new[] {1,1,2}, new[] {1,1,2,3} }; 
var product = sets.Segmented(); 
// yields [1,1,2], [3]

// Transpose 
var rows = new[] { new[] { "a","b" }, new[] { "c","d" } }; 
var cols = rows.Transponded(); 
// yields ["a","c"], ["b","d"]

// Neighbor groups (example uses string key) 
var sequences = new[] { new[] { "a", "b" }, new[] { "a", "b", "c" } , new[] { "b", "c" }}; 
var groups = sequences.ToNeighborGroups(s => s); 
// yields [["a","b", "c"], ["a","b",""], ["","b","c"]
```

## Notes

License and contribution info: refer to the repository at https://github.com/budul100/SetExtensions.