using Graphs.NET.Additional;

namespace TwoBitArrayTests
{
	public class TwoBitArrayTests
	{
		[Fact]
		public void ReadingOneZeroElementTwoBitArrayTest ()
		{
			int[] zero = [0];
			TwoBitArray twoBitArray = new(zero.AsSpan());

			for ( int i = 0; i < TwoBitArray.GetRequiredLengthFor(zero.Length); i++ )
			{
				Assert.Equal(0, twoBitArray[i]);
			}
		}

		[Fact]
		public void ClearingTwoBitArrayTest ()
		{
			int[] arr = [1, 2, 3, 4, 5];
			TwoBitArray twoBitArray = new(arr.AsSpan());

			twoBitArray.Clear();

			for ( int i = 0; i < TwoBitArray.GetRequiredLengthFor(arr.Length); i++ )
			{
				Assert.Equal(0, twoBitArray[i]);
			}
		}

		[Fact]
		public void ReadingOneElementTwoBitArrayTest ()
		{
			int[] filled = [~0];
			TwoBitArray twoBitArray = new(filled.AsSpan());

			for ( int i = 0; i < TwoBitArray.GetRequiredLengthFor(filled.Length); i++ )
			{
				Assert.Equal(3, twoBitArray[i]);
			}
		}

		[Fact]
		public void ReadingTwoElementsTwoBitArrayTest ()
		{
			int[] filled = [0, ~0];
			TwoBitArray twoBitArray = new(filled.AsSpan());

			for ( int i = 0; i < 16; i++ )
			{
				Assert.Equal(0, twoBitArray[i]);
			}
			for ( int i = 16; i < 32; i++ )
			{
				Assert.Equal(3, twoBitArray[i]);
			}
		}

		[Fact]
		public void WritingOneElementTwoBitArrayTest ()
		{
			int[] arr = [0];
			TwoBitArray twoBitArray = new(arr.AsSpan());

			for ( int i = 0; i < TwoBitArray.GetRequiredLengthFor(arr.Length); i++ )
			{
				twoBitArray[i] = i & 3;
				Assert.Equal(i % 3, twoBitArray[i]);
			}

			twoBitArray.Clear();

			for ( int i = 0; i < TwoBitArray.GetRequiredLengthFor(arr.Length); i++ )
			{
				twoBitArray[i] = i & 3;
			}

			for ( int i = 0; i < TwoBitArray.GetRequiredLengthFor(arr.Length); i++ )
			{
				Assert.Equal(i % 3, twoBitArray[i]);
			}
		}

		[Fact]
		public void WritingTwoElementsTwoBitArrayTest ()
		{
			int[] arr = [0, 0];
			TwoBitArray twoBitArray = new(arr.AsSpan());

			for ( int i = 0; i < TwoBitArray.GetRequiredLengthFor(arr.Length); i++ )
			{
				twoBitArray[i] = i & 3;
				Assert.Equal(i % 3, twoBitArray[i]);
			}

			twoBitArray.Clear();

			for ( int i = 0; i < TwoBitArray.GetRequiredLengthFor(arr.Length); i++ )
			{
				twoBitArray[i] = i & 3;
			}

			for ( int i = 0; i < TwoBitArray.GetRequiredLengthFor(arr.Length); i++ )
			{
				Assert.Equal(i % 3, twoBitArray[i]);
			}
		}
	}
}
