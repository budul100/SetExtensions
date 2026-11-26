using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using SetExtensions;
using Xunit;

namespace SetExtensionsTests
{
    public class SegmentedTests
    {
        #region Private Fields

        private static readonly string[] ExpectedA = ["A"];
        private static readonly string[] ExpectedB = ["B"];
        private static readonly string[] ExpectedC = ["C"];

        private static readonly int[] ExpectedInt1 = [1];
        private static readonly int[] ExpectedInt12 = [1, 2];
        private static readonly int[] ExpectedInt2 = [2];
        private static readonly int[] ExpectedInt23 = [2, 3];
        private static readonly int[] ExpectedInt34 = [3, 4];
        private static readonly int[] ExpectedInt4 = [4];
        private static readonly object[] ExpectedObject1 = [1];
        private static readonly object[] ExpectedObject4 = [4];
        private static readonly object[] ExpectedObjectNull3 = [default, 3];

        #endregion Private Fields

        #region Public Methods

        /// <summary>
        /// Empty outer sequence must return empty result without throwing.
        /// </summary>
        [Fact]
        public void Segmented_EmptyInput_ReturnsEmpty()
        {
            var sets = new List<int[]>();

            var result = sets.Segmented().ToArray();

            Assert.Empty(result);
        }

        /// <summary>
        /// Null input must throw ArgumentNullException.
        /// </summary>
        [Fact]
        public void Segmented_NullInput_ThrowsArgumentNullException()
        {
            IEnumerable<IEnumerable<int>> sequences = null;

            Assert.Throws<ArgumentNullException>(() => sequences.Segmented().ToArray());
        }

        /// <summary>
        /// Performance check: segments values read from a CSV file (smoke/perf test).
        /// </summary>
        [Fact]
        public void Segmented_PerformanceOnCsv()
        {
            var folderName = Path.Combine(
                path1: Path.GetTempPath(),
                path2: Path.GetRandomFileName());

            try
            {
                // The archive is copied to the output directory by the test project
                ZipFile.ExtractToDirectory(
                    sourceArchiveFileName: Path.Combine(AppContext.BaseDirectory, "Samples", "SegmentedTests.zip"),
                    destinationDirectoryName: folderName);

                var fileName = Path.Combine(
                    path1: folderName,
                    path2: "Test.csv");

                var sets = File.ReadAllLines(fileName)
                    .Select(l => l.Split(",")).ToList();

                var result = sets.Segmented().ToArray();

                Assert.Equal(365, result.Length);
            }
            finally
            {
                if (Directory.Exists(folderName))
                {
                    Directory.Delete(
                        path: folderName,
                        recursive: true);
                }
            }
        }

        /// <summary>
        /// Segmentation should ignore empties and return distinct non-empty groups preserving values.
        /// </summary>
        [Fact]
        public void Segmented_Values_EmptiesAreIgnored()
        {
            var set1 = new object[] { 1, default, 3 };
            var set2 = new object[] { 1, 1, default, 3, default, 3 };
            var set3 = new object[] { default, 3, 4 };
            var set4 = new object[] { 4, 3, default };
            var set5 = Array.Empty<object>();

            var sets = new List<object[]> { set1, set2, set3, set4, set5, default };

            var result = sets.Segmented().ToArray();

            Assert.Equal(3, result.Length);

            Assert.Equal(
                expected: ExpectedObject1,
                actual: result[0]);
            Assert.Equal(
                expected: ExpectedObjectNull3,
                actual: result[1]);
            Assert.Equal(
                expected: ExpectedObject4,
                actual: result[2]);
        }

        /// <summary>
        /// Segmentation groups integer values into distinct clusters (grouped values).
        /// </summary>
        [Fact]
        public void Segmented_Values_GroupedValues()
        {
            var set1 = new int[] { 1, 2, 3 };
            var set2 = new int[] { 1, 1, 2, 2, 3, 3 };
            var set3 = new int[] { 2, 3, 4 };
            var set4 = new int[] { 4, 3, 2 };
            var set5 = Array.Empty<int>();

            var sets = new List<int[]> { set1, set2, set3, set4, set5, default };

            var result = sets.Segmented().ToArray();

            Assert.Equal(3, result.Length);

            Assert.Equal(
                expected: ExpectedInt1,
                actual: result[0]);
            Assert.Equal(
                expected: ExpectedInt23,
                actual: result[1]);
            Assert.Equal(
                expected: ExpectedInt4,
                actual: result[2]);
        }

        /// <summary>
        /// Identical sequences must collapse into a single group.
        /// </summary>
        [Fact]
        public void Segmented_Values_IdenticalSequences_CollapseIntoOne()
        {
            var set1 = new int[] { 1, 2 };
            var set2 = new int[] { 1, 2 };

            var sets = new List<int[]> { set1, set2 };

            var result = sets.Segmented().ToArray();

            var item = Assert.Single(result);

            Assert.Equal(
                expected: ExpectedInt12.OrderBy(x => x),
                actual: item.OrderBy(x => x));
        }

        /// <summary>
        /// Regression test for index-skipping bug when Remove + Add shifts positions
        /// during iteration. Three sequences with partial pairwise overlaps in a
        /// specific order that previously caused an element to be skipped.
        /// {A,B,C} + {C,D} + {A,D} -> [B], [C], [A], [D]
        /// (all four elements must appear in exactly one group each)
        /// </summary>
        [Fact]
        public void Segmented_Values_IndexStabilityAfterRemoveAndAdd()
        {
            var set1 = new string[] { "A", "B", "C" };
            var set2 = new string[] { "C", "D" };
            var set3 = new string[] { "A", "D" };

            var sets = new List<string[]> { set1, set2, set3 };

            var result = sets.Segmented().ToArray();

            // Every element must appear in exactly one group
            var allElements = result.SelectMany(g => g).ToArray();
            Assert.Equal(4, allElements.Distinct().Count());
            Assert.Equal(4, allElements.Length);

            // Each element appears exactly once
            Assert.Single(result, g => g.Contains("A"));
            Assert.Single(result, g => g.Contains("B"));
            Assert.Single(result, g => g.Contains("C"));
            Assert.Single(result, g => g.Contains("D"));
        }

        /// <summary>
        /// Completely disjoint sequences must remain as separate groups without merging.
        /// </summary>
        [Fact]
        public void Segmented_Values_NoOverlap_RemainsDisjoint()
        {
            var set1 = new int[] { 1, 2 };
            var set2 = new int[] { 3, 4 };

            var sets = new List<int[]> { set1, set2 };

            var result = sets.Segmented().ToArray();

            Assert.Equal(2, result.Length);
            Assert.Contains(result, g => g.SequenceEqual(ExpectedInt12) || g.OrderBy(x => x).SequenceEqual(ExpectedInt12));
            Assert.Contains(result, g => g.SequenceEqual(ExpectedInt34) || g.OrderBy(x => x).SequenceEqual(ExpectedInt34));
        }

        /// <summary>
        /// Single-element sequences where one is a subset of another
        /// must be split into the shared element and the remainder.
        /// {A} + {A,B} -> [A], [B]
        /// </summary>
        [Fact]
        public void Segmented_Values_SingleElementSubset_SplitsCorrectly()
        {
            var set1 = new string[] { "A" };
            var set2 = new string[] { "A", "B" };

            var sets = new List<string[]> { set1, set2 };

            var result = sets.Segmented().ToArray();

            Assert.Equal(2, result.Length);
            Assert.Contains(result, g => g.SequenceEqual(ExpectedA));
            Assert.Contains(result, g => g.SequenceEqual(ExpectedB));
        }

        /// <summary>
        /// Segmentation collapses repeating singletons into single groups.
        /// </summary>
        [Fact]
        public void Segmented_Values_Singletons()
        {
            var set1 = new int[] { 1, };
            var set2 = new int[] { 2, };
            var set3 = new int[] { 2, };
            var set4 = new int[] { 1, };
            var set5 = Array.Empty<int>();

            var sets = new List<int[]> { set1, set2, set3, set4, set5, default };

            var result = sets.Segmented().ToArray();

            Assert.Equal(2, result.Length);

            Assert.Equal(
                expected: ExpectedInt1,
                actual: result[0]);
            Assert.Equal(
                expected: ExpectedInt2,
                actual: result[1]);
        }

        /// <summary>
        /// Three-way partial overlaps must produce three disjoint segments.
        /// {A,B} + {B,C} + {A,C} -> [A], [B], [C]
        /// </summary>
        [Fact]
        public void Segmented_Values_ThreeWayOverlap_ProducesThreeSegments()
        {
            var set1 = new string[] { "A", "B" };
            var set2 = new string[] { "B", "C" };
            var set3 = new string[] { "A", "C" };

            var sets = new List<string[]> { set1, set2, set3 };

            var result = sets.Segmented().ToArray();

            Assert.Equal(3, result.Length);
            Assert.Contains(result, g => g.SequenceEqual(ExpectedA));
            Assert.Contains(result, g => g.SequenceEqual(ExpectedB));
            Assert.Contains(result, g => g.SequenceEqual(ExpectedC));
        }

        #endregion Public Methods
    }
}
