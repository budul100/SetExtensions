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
        #region Public Methods

        /// <summary>
        /// Performance check: segments values read from a CSV file (smoke/perf test).
        /// </summary>
        [Fact]
        public void Segmented_PerformanceOnCsv()
        {
            var folderName = Path.Combine(
                path1: Path.GetTempPath(),
                path2: Path.GetRandomFileName());

            ZipFile.ExtractToDirectory(
                sourceArchiveFileName: @"..\..\..\Samples\SegmentedTests.zip",
                destinationDirectoryName: folderName);

            var fileName = Path.Combine(
                path1: folderName,
                path2: "Test.csv");

            var sets = File.ReadAllLines(fileName)
                .Select(l => l.Split(",")).ToList();

            var result = sets.Segmented().ToArray();

            Assert.Equal(365, result.Length);
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

            var sets = new List<object[]>() { set1, set2, set3, set4, set5, default };

            var result = sets.Segmented().ToArray();

            Assert.Equal(3, result.Length);

            Assert.Equal(
                expected: [1],
                actual: result[0]);
            Assert.Equal(
                expected: [default, 3],
                actual: result[1]);
            Assert.Equal(
                expected: [4],
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

            var sets = new List<int[]>() { set1, set2, set3, set4, set5, default };

            var result = sets.Segmented().ToArray();

            Assert.Equal(3, result.Length);

            Assert.Equal(
                expected: [1],
                actual: result[0]);
            Assert.Equal(
                expected: [2, 3],
                actual: result[1]);
            Assert.Equal(
                expected: [4],
                actual: result[2]);
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

            var sets = new List<int[]>() { set1, set2, set3, set4, set5, default };

            var result = sets.Segmented().ToArray();

            Assert.Equal(2, result.Length);

            Assert.Equal(
                expected: [1],
                actual: result[0]);
            Assert.Equal(
                expected: [2],
                actual: result[1]);
        }

        #endregion Public Methods
    }
}