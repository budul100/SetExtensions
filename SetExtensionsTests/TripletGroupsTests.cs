using SetExtensions;
using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace SetExtensionsTests
{
    public class TripletGroupsTests
    {
        #region Public Methods

        [Fact]
        public void CanUseKeySelectorToNormalizeValues()
        {
            // This test shows that the keySelector is actually used.
            //
            // We treat strings case-insensitively by grouping on ToUpperInvariant().
            //
            // Sequences:
            //   [a, b, c]
            //   [A, B, C]
            //
            // If the keySelector works as intended, triplets with 'b' and 'B'
            // as current should land in the same cluster.

            IEnumerable<IEnumerable<string>> sequences =
            [
                ["a", "b", "c"],
                ["A", "B", "C"]
            ];

            var groups = sequences
                .ToNeighborGroups(s => s.ToUpperInvariant()).ToList();

            Assert.Single(groups);

            var allTriplets = groups.SelectMany(g => g).ToList();

            Assert.Contains(allTriplets, t => t.Previous == "a" && t.Current == "b" && t.Next == "c");
            Assert.Contains(allTriplets, t => t.Previous == "A" && t.Current == "B" && t.Next == "C");
        }

        [Fact]
        public void CrossingPreviousAndNext_ShouldNotEndInSameGroup_IdealBehaviour()
        {
            // This test describes the *intended* semantics:
            //
            //   (A, B, null)  and  (C, B, A)
            //
            // should NOT end up in the same group, because they differ on Previous,
            // and the fact that one of them has Next == null should not "pull" them
            // into a shared Next-based group across the logical boundary.
            //
            // Sequences:
            //   [A, B]
            //   [C, B, A]
            //
            // Triplets with current == "B":
            //   (A, B, null)   from [A,B]
            //   (C, B, A)      from [C,B,A]

            IEnumerable<IEnumerable<string>> sequences =
            [
                ["A", "B"],
                ["A", "B"],
                ["C", "B", "A"]
            ];

            var groups = sequences
                .ToNeighborGroups(s => s).ToList();

            Assert.Equal(2, groups.Count());

            foreach (var group in groups)
            {
                bool hasFirst = group.Any(t => t.Previous == "A" && t.Current == "B" && t.Next == null);
                bool hasSecond = group.Any(t => t.Previous == "C" && t.Current == "B" && t.Next == "A");

                Assert.False(hasFirst && hasSecond, "A,B,null and C,B,A must not end up in the same group.");
            }
        }

        [Fact]
        public void MaxByPrevOrNext_PicksLargestGroupFirst()
        {
            // All triplets with current == "X":
            //   (A, X, null)   x2
            //   (B, X, null)
            //   (C, X, null)
            //
            // Previous-based grouping (by A/B/C):
            //   GroupPrev(A): 2 items
            //   GroupPrev(B): 1 item
            //   GroupPrev(C): 1 item

            IEnumerable<IEnumerable<string>> sequences =
            [
                ["A", "X"],
                ["A", "X"],
                ["B", "X"],
                ["C", "X"]
            ];

            var groups = sequences
                .ToNeighborGroups(s => s).ToList();

            Assert.Equal(3, groups.Count);
            Assert.Equal(4, groups.Sum(e => e.Count()));
        }

        [Fact]
        public void MiddleElement_B_IsGroupedAcrossThreeSequences()
        {
            // Sequences:
            //   1: A B
            //   2: A B C
            //   3: B C
            //
            // Triplets with current == "B":
            //   (A, B, null)   from [A,B]
            //   (A, B, C)      from [A,B,C]
            //   (null, B, C)   from [B,C]
            //
            // The grouping algorithm should put these three
            // into a *single* group for current == "B".

            IEnumerable<IEnumerable<string>> sequences =
            [
                ["A", "B"],
                ["A", "B", "C"],
                ["B", "C"]
            ];

            var groups = sequences
                .ToNeighborGroups(s => s).ToList();

            Assert.Single(groups);

            Assert.Equal(3, groups.Single().Count());
            Assert.Contains(groups.Single(), t => t.Previous == "A" && t.Current == "B" && t.Next == null);
            Assert.Contains(groups.Single(), t => t.Previous == "A" && t.Current == "B" && t.Next == "C");
            Assert.Contains(groups.Single(), t => t.Previous == null && t.Current == "B" && t.Next == "C");
        }

        #endregion Public Methods
    }
}