using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using NUnit.Framework;
using SetExtensions;

namespace SetExtensionsTests
{
    public class Tests
    {
        #region Private Fields

        private int yieldCount;

        #endregion Private Fields

        #region Public Methods

        [Test]
        public void GetCartesianProduct()
        {
            var set1 = new int[] { 1, 2, 3 };
            var set2 = new int[] { 2, 3, 4 };
            var set3 = new int[] { 4, 5, 6 };

            var result = new[] { set1, set2, set3 }
                .CartesianProduct().ToArray();

            Assert.True(result.Length == 27);

            Assert.Contains(new int[] { 1, 2, 4 }, result);
            Assert.Contains(new int[] { 1, 2, 5 }, result);
            Assert.Contains(new int[] { 1, 2, 6 }, result);
            Assert.Contains(new int[] { 1, 3, 4 }, result);
            Assert.Contains(new int[] { 1, 3, 5 }, result);
            Assert.Contains(new int[] { 1, 3, 6 }, result);
            Assert.Contains(new int[] { 1, 4, 4 }, result);
            Assert.Contains(new int[] { 1, 4, 5 }, result);
            Assert.Contains(new int[] { 1, 4, 6 }, result);

            Assert.Contains(new int[] { 2, 2, 4 }, result);
            Assert.Contains(new int[] { 2, 2, 5 }, result);
            Assert.Contains(new int[] { 2, 2, 6 }, result);
            Assert.Contains(new int[] { 2, 3, 4 }, result);
            Assert.Contains(new int[] { 2, 3, 5 }, result);
            Assert.Contains(new int[] { 2, 3, 6 }, result);
            Assert.Contains(new int[] { 2, 4, 4 }, result);
            Assert.Contains(new int[] { 2, 4, 5 }, result);
            Assert.Contains(new int[] { 2, 4, 6 }, result);

            Assert.Contains(new int[] { 3, 2, 4 }, result);
            Assert.Contains(new int[] { 3, 2, 5 }, result);
            Assert.Contains(new int[] { 3, 2, 6 }, result);
            Assert.Contains(new int[] { 3, 3, 4 }, result);
            Assert.Contains(new int[] { 3, 3, 5 }, result);
            Assert.Contains(new int[] { 3, 3, 6 }, result);
            Assert.Contains(new int[] { 3, 4, 4 }, result);
            Assert.Contains(new int[] { 3, 4, 5 }, result);
            Assert.Contains(new int[] { 3, 4, 6 }, result);
        }

        [Test]
        public void IsSubsetOf()
        {
            var set1 = new object[] { 1, default, 3 };
            var set2 = new object[] { 1, 1, default, 3, default, 3 };
            var set3 = new object[] { default, 3, 4 };
            var set4 = new object[] { 4, 3, default };
            var set5 = System.Array.Empty<object>();
            var set6 = default(IEnumerable<object>);

            Assert.IsTrue(set1.IsSubSetOf(set2));
            Assert.IsFalse(set2.IsSubSetOf(set1));

            Assert.IsTrue(set3.IsSubSetOf(set4));
            Assert.IsTrue(set4.IsSubSetOf(set3));

            Assert.IsFalse(set5.IsSubSetOf(set1));
            Assert.IsFalse(set5.IsSubSetOf(set6));
            Assert.IsFalse(set6.IsSubSetOf(set5));
        }

        [Test]
        public void IsSubSetOfOrOther()
        {
            var set1 = new object[] { 1, default, 3 };
            var set2 = new object[] { 1, 1, default, 3, default, 3 };
            var set3 = new object[] { default, 3, 4 };
            var set4 = new object[] { 4, 3, default };
            var set5 = System.Array.Empty<object>();
            var set6 = default(IEnumerable<object>);

            Assert.IsTrue(set1.IsSubSetOfOrOther(set2));
            Assert.IsTrue(set2.IsSubSetOfOrOther(set1));

            Assert.IsTrue(set3.IsSubSetOfOrOther(set4));
            Assert.IsTrue(set4.IsSubSetOfOrOther(set3));

            Assert.IsFalse(set5.IsSubSetOfOrOther(set1));
            Assert.IsFalse(set5.IsSubSetOf(set6));
            Assert.IsFalse(set6.IsSubSetOf(set5));
        }

        [Test]
        public void SegmentPerformance()
        {
            var folderName = Path.Combine(
                path1: Path.GetTempPath(),
                path2: Path.GetRandomFileName());

            ZipFile.ExtractToDirectory(
                sourceArchiveFileName: @"..\..\..\Test.zip",
                destinationDirectoryName: folderName);

            var fileName = Path.Combine(
                path1: folderName,
                path2: "Test.csv");

            var sets = File.ReadAllLines(fileName)
                .Select(l => l.Split(",")).ToList();

            var result = sets.Segmented().ToArray();

            Assert.IsTrue(result.Length == 365);
        }

        [Test]
        public void SegmentValueEmpties()
        {
            var set1 = new object[] { 1, default, 3 };
            var set2 = new object[] { 1, 1, default, 3, default, 3 };
            var set3 = new object[] { default, 3, 4 };
            var set4 = new object[] { 4, 3, default };
            var set5 = System.Array.Empty<object>();

            var sets = new List<object[]>() { set1, set2, set3, set4, set5, default };

            var result = sets.Segmented().ToArray();

            Assert.IsTrue(result.Length == 3);

            Assert.AreEqual(
                expected: new object[] { 1 },
                actual: result[0]);
            Assert.AreEqual(
                expected: new object[] { default, 3 },
                actual: result[1]);
            Assert.AreEqual(
                expected: new object[] { 4 },
                actual: result[2]);
        }

        [Test]
        public void SegmentValueGroups()
        {
            var set1 = new int[] { 1, 2, 3 };
            var set2 = new int[] { 1, 1, 2, 2, 3, 3 };
            var set3 = new int[] { 2, 3, 4 };
            var set4 = new int[] { 4, 3, 2 };
            var set5 = System.Array.Empty<int>();

            var sets = new List<int[]>() { set1, set2, set3, set4, set5, default };

            var result = sets.Segmented().ToArray();

            Assert.IsTrue(result.Length == 3);

            Assert.AreEqual(
                expected: new int[] { 1 },
                actual: result[0]);
            Assert.AreEqual(
                expected: new int[] { 2, 3 },
                actual: result[1]);
            Assert.AreEqual(
                expected: new int[] { 4 },
                actual: result[2]);
        }

        [Test]
        public void SegmentValueSingles()
        {
            var set1 = new int[] { 1, };
            var set2 = new int[] { 2, };
            var set3 = new int[] { 2, };
            var set4 = new int[] { 1, };
            var set5 = System.Array.Empty<int>();

            var sets = new List<int[]>() { set1, set2, set3, set4, set5, default };

            var result = sets.Segmented().ToArray();

            Assert.IsTrue(result.Length == 2);

            Assert.AreEqual(
                expected: new int[] { 1 },
                actual: result[0]);
            Assert.AreEqual(
                expected: new int[] { 2 },
                actual: result[1]);
        }

        [Test]
        public void TranspondedFromEnumerable()
        {
            var sets = new List<IEnumerable<int?>>() { GetNumbers(), GetNulls(), GetNumbers(), GetNumbers(), default, GetNumbers() };

            yieldCount = 0;

            var result = sets.Transponded().ToArray();

            Assert.IsTrue(yieldCount == 5);
            Assert.IsTrue(result.Length == 4);

            Assert.AreEqual(
                expected: new int?[] { 1, default, 1, 1, default, 1 },
                actual: result[0]);
            Assert.AreEqual(
                expected: new int?[] { 2, default, 2, 2, default, 2 },
                actual: result[1]);
            Assert.AreEqual(
                expected: new int?[] { 3, default, 3, 3, default, 3 },
                actual: result[2]);
            Assert.AreEqual(
                expected: new int?[] { 4, default, 4, 4, default, 4 },
                actual: result[3]);
        }

        [Test]
        public void TranspondedValuesDifferentSizesClass()
        {
            var set1 = new string[] { "1", "2", "3" };
            var set2 = new string[] { "1", "1", "2", "2" };
            var set3 = System.Array.Empty<string>();
            var set4 = new string[] { "2", "3", "4" };
            var set5 = new string[] { "4", "3", "2" };

            var sets = new List<string[]>() { set1, set2, set3, set4, set5, default };

            var result = sets.Transponded().ToArray();

            Assert.IsTrue(result.Length == 4);

            Assert.AreEqual(
                expected: new string[] { "1", "1", default, "2", "4", default },
                actual: result[0]);
            Assert.AreEqual(
                expected: new string[] { "2", "1", default, "3", "3", default },
                actual: result[1]);
            Assert.AreEqual(
                expected: new string[] { "3", "2", default, "4", "2", default },
                actual: result[2]);
            Assert.AreEqual(
                expected: new string[] { default, "2", default, default, default, default },
                actual: result[3]);
        }

        [Test]
        public void TranspondedValuesDifferentSizesStruct()
        {
            var set1 = new int[] { 1, 2, 3 };
            var set2 = new int[] { 1, 1, 2, 2 };
            var set3 = System.Array.Empty<int>();
            var set4 = new int[] { 2, 3, 4 };
            var set5 = new int[] { 4, 3, 2 };

            var sets = new List<int[]>() { set1, set2, set3, set4, set5, default };

            var result = sets.Transponded().ToArray();

            Assert.IsTrue(result.Length == 4);

            Assert.AreEqual(
                expected: new int[] { 1, 1, 0, 2, 4, 0 },
                actual: result[0]);
            Assert.AreEqual(
                expected: new int[] { 2, 1, 0, 3, 3, 0 },
                actual: result[1]);
            Assert.AreEqual(
                expected: new int[] { 3, 2, 0, 4, 2, 0 },
                actual: result[2]);
            Assert.AreEqual(
                expected: new int[] { 0, 2, 0, 0, 0, 0 },
                actual: result[3]);
        }

        [Test]
        public void TranspondedValuesSameSizes()
        {
            var set1 = new int[] { 1, 2, 3 };
            var set2 = new int[] { 1, 1, 2 };
            var set3 = System.Array.Empty<int>();
            var set4 = new int[] { 2, 3, 4 };
            var set5 = new int[] { 4, 3, 2 };

            var sets = new List<int[]>() { set1, set2, set3, set4, set5, default };

            var result = sets.Transponded().ToArray();

            Assert.IsTrue(result.Length == 3);

            Assert.AreEqual(
                expected: new int[] { 1, 1, 0, 2, 4, 0 },
                actual: result[0]);
            Assert.AreEqual(
                expected: new int[] { 2, 1, 0, 3, 3, 0 },
                actual: result[1]);
            Assert.AreEqual(
                expected: new int[] { 3, 2, 0, 4, 2, 0 },
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