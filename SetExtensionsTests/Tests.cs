using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;

namespace SetExtensionsTests
{
    public class Tests
    {
        #region Public Methods

        [SetUp]
        public void Setup()
        {
        }

        [Test]
        public void TestSegmentation()
        {
            var set1 = new int[] { 1, 2, 3 };
            var set2 = new int[] { 2, 3, 4 };

            var sets = new List<int[]>() { set1, set2 };

            var result = SetExtensions.Extensions.Segemented(sets).ToArray();

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