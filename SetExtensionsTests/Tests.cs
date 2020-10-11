using NUnit.Framework;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;

namespace SetExtensionsTests
{
    public class Tests
    {
        #region Public Methods

        [Test]
        public void SegmentEmptyValues()
        {
            var set1 = new object[] { 1, default, 3 };
            var set2 = new object[] { 1, 1, default, 3, default, 3 };
            var set3 = new object[] { default, 3, 4 };
            var set4 = new object[] { 4, 3, default };
            var set5 = System.Array.Empty<object>();

            var sets = new List<object[]>() { set1, set2, set3, set4, set5, default };

            var result = SetExtensions.Extensions.Segmented(sets).ToArray();

            Assert.IsTrue(result.Count() == 3);

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
        public void SegmentPerformance()
        {
            if (!File.Exists(@"..\..\..\Test.csv"))
            {
                ZipFile.ExtractToDirectory(
                    sourceArchiveFileName: @"..\..\..\Test.zip",
                    destinationDirectoryName: @"..\..\..");
            }

            var sets = File.ReadAllLines(@"..\..\..\Test.csv")
                .Select(l => l.Split(",")).ToList();

            File.Delete(@"..\..\..\Test.csv");

            var result = SetExtensions.Extensions.Segmented(sets).ToArray();

            Assert.IsTrue(result.Count() == 365);
        }

        [Test]
        public void SegmentValues()
        {
            var set1 = new int[] { 1, 2, 3 };
            var set2 = new int[] { 1, 1, 2, 2, 3, 3 };
            var set3 = new int[] { 2, 3, 4 };
            var set4 = new int[] { 4, 3, 2 };
            var set5 = System.Array.Empty<int>();

            var sets = new List<int[]>() { set1, set2, set3, set4, set5, default };

            var result = SetExtensions.Extensions.Segmented(sets).ToArray();

            Assert.IsTrue(result.Count() == 3);

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

        #endregion Public Methods
    }
}