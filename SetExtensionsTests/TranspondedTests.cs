using System.Collections.Generic;
using System.Linq;
using SetExtensions;
using Xunit;

namespace SetExtensionsTests
{
    public class TranspondedTests
    {
        #region Private Fields

        private int yieldCount;

        #endregion Private Fields

        #region Public Methods

        /// <summary>
        /// Transposition from enumerables (with lazy enumerables included)
        /// yields correct columns and triggers expected enumerations.
        /// </summary>
        [Fact]
        public void Transponded_FromEnumerableWithLazySources()
        {
            var sets = new List<IEnumerable<int?>>()
            {
                GetNumbers(),
                GetNulls(),
                GetNumbers(),
                GetNumbers(),
                default,
                GetNumbers() };

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

        /// <summary>
        /// Transposition of string sequences with different sizes
        /// yields expected columns (reference types, nulls for missing).
        /// </summary>
        [Fact]
        public void Transponded_Values_DifferentSizes_Class()
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

        /// <summary>
        /// Transposition of integer sequences with different sizes
        /// yields expected columns (value types, default when missing).
        /// </summary>
        [Fact]
        public void Transponded_Values_DifferentSizes_Struct()
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

        /// <summary>
        /// Transposition when most sequences share the same length yields expected columns.
        /// </summary>
        [Fact]
        public void Transponded_Values_SameSizes()
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