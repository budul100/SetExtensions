using SetExtensions;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using Xunit;

namespace SetExtensionsTests
{
    public class SequenceTests
    {
        #region Private Fields

        private int yieldCount;

        #endregion Private Fields

        #region Public Methods

        [Fact]
        public void GetCartesianProduct()
        {
            var set1 = new int[] { 1, 2, 3 };
            var set2 = new int[] { 2, 3, 4 };
            var set3 = new int[] { 4, 5, 6 };

            var result = new[] { set1, set2, set3 }
                .CartesianProduct().ToArray();

            Assert.Equal(27, result.Length);

            Assert.Contains(result, r => r.SequenceEqual([1, 2, 4]));
            Assert.Contains(result, r => r.SequenceEqual([1, 2, 5]));
            Assert.Contains(result, r => r.SequenceEqual([1, 2, 6]));
            Assert.Contains(result, r => r.SequenceEqual([1, 3, 4]));
            Assert.Contains(result, r => r.SequenceEqual([1, 3, 5]));
            Assert.Contains(result, r => r.SequenceEqual([1, 3, 6]));
            Assert.Contains(result, r => r.SequenceEqual([1, 4, 4]));
            Assert.Contains(result, r => r.SequenceEqual([1, 4, 5]));
            Assert.Contains(result, r => r.SequenceEqual([1, 4, 6]));

            Assert.Contains(result, r => r.SequenceEqual([2, 2, 4]));
            Assert.Contains(result, r => r.SequenceEqual([2, 2, 5]));
            Assert.Contains(result, r => r.SequenceEqual([2, 2, 6]));
            Assert.Contains(result, r => r.SequenceEqual([2, 3, 4]));
            Assert.Contains(result, r => r.SequenceEqual([2, 3, 5]));
            Assert.Contains(result, r => r.SequenceEqual([2, 3, 6]));
            Assert.Contains(result, r => r.SequenceEqual([2, 4, 4]));
            Assert.Contains(result, r => r.SequenceEqual([2, 4, 5]));
            Assert.Contains(result, r => r.SequenceEqual([2, 4, 6]));

            Assert.Contains(result, r => r.SequenceEqual([3, 2, 4]));
            Assert.Contains(result, r => r.SequenceEqual([3, 2, 5]));
            Assert.Contains(result, r => r.SequenceEqual([3, 2, 6]));
            Assert.Contains(result, r => r.SequenceEqual([3, 3, 4]));
            Assert.Contains(result, r => r.SequenceEqual([3, 3, 5]));
            Assert.Contains(result, r => r.SequenceEqual([3, 3, 6]));
            Assert.Contains(result, r => r.SequenceEqual([3, 4, 4]));
            Assert.Contains(result, r => r.SequenceEqual([3, 4, 5]));
            Assert.Contains(result, r => r.SequenceEqual([3, 4, 6]));
        }

        [Fact]
        public void IsSubsetOf()
        {
            var set1 = new object[] { 1, default, 3 };
            var set2 = new object[] { 1, 1, default, 3, default, 3 };
            var set3 = new object[] { default, 3, 4 };
            var set4 = new object[] { 4, 3, default };
            var set5 = System.Array.Empty<object>();
            var set6 = default(IEnumerable<object>);

            Assert.True(set1.IsSubSetOf(set2));
            Assert.False(set2.IsSubSetOf(set1));

            Assert.True(set3.IsSubSetOf(set4));
            Assert.True(set4.IsSubSetOf(set3));

            Assert.False(set5.IsSubSetOf(set1));
            Assert.False(set5.IsSubSetOf(set6));
            Assert.False(set6.IsSubSetOf(set5));
        }

        [Fact]
        public void IsSubSetOfOrOther()
        {
            var set1 = new object[] { 1, default, 3 };
            var set2 = new object[] { 1, 1, default, 3, default, 3 };
            var set3 = new object[] { default, 3, 4 };
            var set4 = new object[] { 4, 3, default };
            var set5 = System.Array.Empty<object>();
            var set6 = default(IEnumerable<object>);

            Assert.True(set1.IsSubSetOfOrOther(set2));
            Assert.True(set2.IsSubSetOfOrOther(set1));

            Assert.True(set3.IsSubSetOfOrOther(set4));
            Assert.True(set4.IsSubSetOfOrOther(set3));

            Assert.False(set5.IsSubSetOfOrOther(set1));
            Assert.False(set5.IsSubSetOf(set6));
            Assert.False(set6.IsSubSetOf(set5));
        }

        [Fact]
        public void SegmentPerformance()
        {
            var folderName = Path.Combine(
                path1: Path.GetTempPath(),
                path2: Path.GetRandomFileName());

            ZipFile.ExtractToDirectory(
                sourceArchiveFileName: @"..\..\..\SequenceTests.zip",
                destinationDirectoryName: folderName);

            var fileName = Path.Combine(
                path1: folderName,
                path2: "Test.csv");

            var sets = File.ReadAllLines(fileName)
                .Select(l => l.Split(",")).ToList();

            var result = sets.Segmented().ToArray();

            Assert.Equal(365, result.Length);
        }

        [Fact]
        public void SegmentValueEmpties()
        {
            var set1 = new object[] { 1, default, 3 };
            var set2 = new object[] { 1, 1, default, 3, default, 3 };
            var set3 = new object[] { default, 3, 4 };
            var set4 = new object[] { 4, 3, default };
            var set5 = System.Array.Empty<object>();

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

        [Fact]
        public void SegmentValueGroups()
        {
            var set1 = new int[] { 1, 2, 3 };
            var set2 = new int[] { 1, 1, 2, 2, 3, 3 };
            var set3 = new int[] { 2, 3, 4 };
            var set4 = new int[] { 4, 3, 2 };
            var set5 = System.Array.Empty<int>();

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

        [Fact]
        public void SegmentValueSingles()
        {
            var set1 = new int[] { 1, };
            var set2 = new int[] { 2, };
            var set3 = new int[] { 2, };
            var set4 = new int[] { 1, };
            var set5 = System.Array.Empty<int>();

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

        [Fact]
        public void TranspondedFromEnumerable()
        {
            var sets = new List<IEnumerable<int?>>() { GetNumbers(), GetNulls(), GetNumbers(), GetNumbers(), default, GetNumbers() };

            yieldCount = 0;

            var result = sets.Transponded().ToArray();

            Assert.Equal(5, yieldCount);
            Assert.Equal(4, result.Length);

            Assert.Equal(
                expected: [1, default, 1, 1, default, 1],
                actual: result[0]);
            Assert.Equal(
                expected: [2, default, 2, 2, default, 2],
                actual: result[1]);
            Assert.Equal(
                expected: [3, default, 3, 3, default, 3],
                actual: result[2]);
            Assert.Equal(
                expected: [4, default, 4, 4, default, 4],
                actual: result[3]);
        }

        [Fact]
        public void TranspondedValuesDifferentSizesClass()
        {
            var set1 = new string[] { "1", "2", "3" };
            var set2 = new string[] { "1", "1", "2", "2" };
            var set3 = System.Array.Empty<string>();
            var set4 = new string[] { "2", "3", "4" };
            var set5 = new string[] { "4", "3", "2" };

            var sets = new List<string[]>() { set1, set2, set3, set4, set5, default };

            var result = sets.Transponded().ToArray();

            Assert.Equal(4, result.Length);

            Assert.Equal(
                expected: ["1", "1", default, "2", "4", default],
                actual: result[0]);
            Assert.Equal(
                expected: ["2", "1", default, "3", "3", default],
                actual: result[1]);
            Assert.Equal(
                expected: ["3", "2", default, "4", "2", default],
                actual: result[2]);
            Assert.Equal(
                expected: [default, "2", default, default, default, default],
                actual: result[3]);
        }

        [Fact]
        public void TranspondedValuesDifferentSizesStruct()
        {
            var set1 = new int[] { 1, 2, 3 };
            var set2 = new int[] { 1, 1, 2, 2 };
            var set3 = System.Array.Empty<int>();
            var set4 = new int[] { 2, 3, 4 };
            var set5 = new int[] { 4, 3, 2 };

            var sets = new List<int[]>() { set1, set2, set3, set4, set5, default };

            var result = sets.Transponded().ToArray();

            Assert.Equal(4, result.Length);

            Assert.Equal(
                expected: [1, 1, 0, 2, 4, 0],
                actual: result[0]);
            Assert.Equal(
                expected: [2, 1, 0, 3, 3, 0],
                actual: result[1]);
            Assert.Equal(
                expected: [3, 2, 0, 4, 2, 0],
                actual: result[2]);
            Assert.Equal(
                expected: [0, 2, 0, 0, 0, 0],
                actual: result[3]);
        }

        [Fact]
        public void TranspondedValuesSameSizes()
        {
            var set1 = new int[] { 1, 2, 3 };
            var set2 = new int[] { 1, 1, 2 };
            var set3 = System.Array.Empty<int>();
            var set4 = new int[] { 2, 3, 4 };
            var set5 = new int[] { 4, 3, 2 };

            var sets = new List<int[]>() { set1, set2, set3, set4, set5, default };

            var result = sets.Transponded().ToArray();

            Assert.Equal(3, result.Length);

            Assert.Equal(
                expected: [1, 1, 0, 2, 4, 0],
                actual: result[0]);
            Assert.Equal(
                expected: [2, 1, 0, 3, 3, 0],
                actual: result[1]);
            Assert.Equal(
                expected: [3, 2, 0, 4, 2, 0],
                actual: result[2]);
        }

        #endregion Public Methods

        #region Private Methods

        private IEnumerable<int?> GetNulls()
        {
            yieldCount++;

            yield return default;
            yield return default;
            yield return default;
            yield return default;
        }

        private IEnumerable<int?> GetNumbers()
        {
            yieldCount++;

            yield return 1;
            yield return 2;
            yield return 3;
            yield return 4;
        }

        #endregion Private Methods
    }
}