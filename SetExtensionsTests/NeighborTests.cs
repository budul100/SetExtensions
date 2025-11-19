using System;
using System.Collections.Generic;
using System.Linq;
using SetExtensions;
using Xunit;

namespace SetExtensionsTests
{
    public class NeighborTests
    {
        #region Public Methods

        /// <summary>
        /// Ensures that sequences with the same middle element
        /// but different neighbors do not end up in the same group.
        /// </summary>
        [Fact]
        public void ToNeighborGroups_CrossingPreviousAndNext_ShouldNotEndInSameGroup()
        {
            IEnumerable<IEnumerable<string>> sequences =
            [
                ["A", "B"],
                ["A", "B"],
                ["C", "B", "A"]
            ];

            var groups = sequences
                .ToNeighborGroups(s => s).ToList();

            Assert.Equal(5, groups.Count);

            foreach (var group in groups)
            {
                bool hasFirst = group.Any(t => t.Previous == "A" && t.Current == "B" && t.Next == null);
                bool hasSecond = group.Any(t => t.Previous == "C" && t.Current == "B" && t.Next == "A");

                Assert.False(hasFirst && hasSecond, "A,B,null and C,B,A must not end up in the same group.");
            }
        }

        /// <summary>
        /// Confirms that MaxByPrevOrNext strategy picks the largest group first when grouping.
        /// </summary>
        [Fact]
        public void ToNeighborGroups_MaxByPrevOrNext_PicksLargestGroupFirst()
        {
            IEnumerable<IEnumerable<string>> sequences =
            [
                ["A", "X"],
                ["A", "X"],
                ["B", "X"],
                ["C", "X"]
            ];

            var groups = sequences
                .ToNeighborGroups(s => s).ToList();

            Assert.Equal(6, groups.Count);
            Assert.Equal(8, groups.Sum(e => e.Count()));
        }

        /// <summary>
        /// Validates behavior when ends are merged or not merged via the mergeEnds parameter.
        /// </summary>
        [Fact]
        public void ToNeighborGroups_MergeEnds_ControlsGroupingAtSequenceEnds()
        {
            IEnumerable<IEnumerable<string>> sequences =
            [
                ["A", "X"],
                ["A", "X"],
                ["B", "X"],
                ["B", "X", "D"]
            ];

            var mergedGroups = sequences.ToNeighborGroups(
                keySelector: s => s,
                mergeEnds: true).ToList();

            var unmergedGroups = sequences.ToNeighborGroups(
                keySelector: s => s,
                mergeEnds: false).ToList();

            Assert.Equal(4, mergedGroups.Count);
            Assert.Equal(9, mergedGroups.Sum(e => e.Count()));

            Assert.Equal(5, unmergedGroups.Count);
            Assert.Equal(9, unmergedGroups.Sum(e => e.Count()));
        }

        /// <summary>
        /// Ensures the middle element is grouped correctly across three overlapping sequences.
        /// </summary>
        [Fact]
        public void ToNeighborGroups_MiddleElement_IsGroupedAcrossThreeSequences()
        {
            IEnumerable<IEnumerable<string>> sequences =
            [
                ["A", "B"],
                ["A", "B", "C"],
                ["B", "C"]
            ];

            var groups = sequences
                .ToNeighborGroups(s => s).ToList();

            Assert.Equal(3, groups.Count);

            Assert.Contains(groups, g => g.Any(t => t.Previous == "A" && t.Current == "B" && t.Next == null));
            Assert.Contains(groups, g => g.Any(t => t.Previous == "A" && t.Current == "B" && t.Next == "C"));
            Assert.Contains(groups, g => g.Any(t => t.Previous == null && t.Current == "B" && t.Next == "C"));

            Assert.Contains(groups, g => g.All(t => t.Previous == "B" && t.Current == "C" && t.Next == null));
            Assert.Contains(groups, g => g.All(t => t.Previous == null && t.Current == "A" && t.Next == "B"));
        }

        /// <summary>
        /// Verifies that a provided key selector is used
        /// to normalize values (e.g. case-insensitive grouping).
        /// </summary>
        [Fact]
        public void ToNeighborGroups_UsesKeySelectorToNormalizeValues()
        {
            IEnumerable<IEnumerable<string>> sequences =
            [
                ["a", "b", "c"],
                ["A", "B", "C"]
            ];

            var groups = sequences
                .ToNeighborGroups(s => s.ToUpperInvariant()).ToList();

            Assert.Equal(3, groups.Count);

            var allTriplets = groups
                .SelectMany(g => g).ToList();

            Assert.Equal(6, allTriplets.Count);

            Assert.Contains(allTriplets,
                t => (t.Previous == "a" && t.Current == "b") || (t.Current == "b" && t.Next == "c"));
            Assert.Contains(allTriplets,
                t => (t.Previous == "A" && t.Current == "B") || (t.Current == "b" && t.Next == "C"));
        }

        #endregion Public Methods
    }
}