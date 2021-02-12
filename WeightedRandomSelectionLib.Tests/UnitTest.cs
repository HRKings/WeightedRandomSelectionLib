using WeightedRandomSelectionLib.Structure;
using Xunit;

namespace WeightedRandomSelectionLib.Tests
{
	public class UnitTest
	{
		private WeightedRandomSelector<int> _intSelector;
		private WeightedRandomSelector<string> _stringSelector;

		[Theory]
		[InlineData(0)]
		[InlineData(1)]
		[InlineData(500)]
		[InlineData(int.MaxValue)]
		[InlineData(int.MinValue)]
		[InlineData(-4846)]
		public void Should_Return_Integer(int expected)
		{
			_intSelector = new WeightedRandomSelector<int>();

			_intSelector.Add(expected, 1.0);

			Assert.Equal(expected, _intSelector.Select());
		}

		[Theory]
		[InlineData(0, 1, 2, 3)]
		[InlineData(000, 500, 2645, 35987)]
		[InlineData(-4564, 10, 2498, -4798864)]
		[InlineData(int.MaxValue, int.MinValue, short.MaxValue, short.MinValue)]
		public void Should_Return_OneOf_Integers(int expected1, int expected2, int expected3, int expected4)
		{
			_intSelector = new WeightedRandomSelector<int>();

			int[] testPool = {expected1, expected2, expected3, expected4};

			_intSelector.Add(expected1, 0.5);
			_intSelector.Add(expected2, 0.5);
			_intSelector.Add(expected3, 0.5);
			_intSelector.Add(expected4, 0.5);

			Assert.Contains(_intSelector.Select(), testPool);
		}

		[Theory]
		[InlineData("Test")]
		[InlineData("")]
		[InlineData(" lorem ipsum dolor ")]
		public void Should_Return_String(string expected)
		{
			_stringSelector = new WeightedRandomSelector<string>();

			_stringSelector.Add(expected, 1.0);

			Assert.Equal(expected, _stringSelector.Select());
		}

		[Fact]
		public void Should_Deconstruct()
		{
			WeightedItem<string> item = new("lorem", 5.0);
			(string name, double weight) = item;

			Assert.Equal(name, item.Value);
			Assert.Equal(weight, item.Weight);
		}
	}
}