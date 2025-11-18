using System.Collections.Generic;
using System.Linq;
using SetExtensions;
using Xunit;

namespace SetExtensionsTests
{
    public class SequenceTests
    {
        #region Public Methods

        /// <summary>
        /// Verifies that CartesianProduct returns all combinations for the provided input sequences.
        /// </summary>
        [Fact]
        public void CartesianProduct_ReturnsAllCombinations()
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

        /// <summary>
        /// Tests multiset subset behaviour: counts and duplicates must be respected.
        /// </summary>
        [Fact]
        public void IsSubSetOf_MultisetSubsetBehavior()
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

        /// <summary>
        /// Tests symmetric multiset subset detection (either sequence may be subset of the other).
        /// </summary>
        [Fact]
        public void IsSubSetOfOrOther_EitherDirection()
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

        #endregion Public Methods
    }
}