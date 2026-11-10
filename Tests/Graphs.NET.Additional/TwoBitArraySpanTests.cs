using Graphs.NET.Additional;

namespace Tests.Graphs.NET.Additional
{
	public class TwoBitArraySpanTests
	{
		[Fact]
		public void ReadingOneZeroElementTwoBitArrayTest ()
		{
			int[] zero = [0];
			TwoBitArraySpan twoBitArray = new(zero.AsSpan());

			for ( int i = 0; i < TwoBitArraySpan.GetRequiredLengthFor(zero.Length); i++ )
			{
				Assert.Equal(0, twoBitArray[i]);
			}
		}

		[Fact]
		public void ClearingTwoBitArrayTest ()
		{
			int[] arr = [1, 2, 3, 4, 5];
			TwoBitArraySpan twoBitArray = new(arr.AsSpan());

			twoBitArray.Clear();

			for ( int i = 0; i < TwoBitArraySpan.GetRequiredLengthFor(arr.Length); i++ )
			{
				Assert.Equal(0, twoBitArray[i]);
			}
		}

		[Fact]
		public void ReadingOneElementTwoBitArrayTest ()
		{
			int[] filled = [~0];
			TwoBitArraySpan twoBitArray = new(filled.AsSpan());

			for ( int i = 0; i < TwoBitArraySpan.GetRequiredLengthFor(filled.Length); i++ )
			{
				Assert.Equal(3, twoBitArray[i]);
			}
		}

		[Fact]
		public void ReadingTwoElementsTwoBitArrayTest ()
		{
			int[] filled = [0, ~0];
			TwoBitArraySpan twoBitArray = new(filled.AsSpan());

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
			TwoBitArraySpan twoBitArray = new(arr.AsSpan());

			for ( int i = 0; i < TwoBitArraySpan.GetRequiredLengthFor(arr.Length); i++ )
			{
				twoBitArray[i] = i & 3;
				Assert.Equal(i % 3, twoBitArray[i]);
			}

			twoBitArray.Clear();

			for ( int i = 0; i < TwoBitArraySpan.GetRequiredLengthFor(arr.Length); i++ )
			{
				twoBitArray[i] = i & 3;
			}

			for ( int i = 0; i < TwoBitArraySpan.GetRequiredLengthFor(arr.Length); i++ )
			{
				Assert.Equal(i % 3, twoBitArray[i]);
			}
		}

		[Fact]
		public void WritingTwoElementsTwoBitArrayTest ()
		{
			int[] arr = [0, 0];
			TwoBitArraySpan twoBitArray = new(arr.AsSpan());

			for ( int i = 0; i < TwoBitArraySpan.GetRequiredLengthFor(arr.Length); i++ )
			{
				twoBitArray[i] = i & 3;
				Assert.Equal(i % 3, twoBitArray[i]);
			}

			twoBitArray.Clear();

			for ( int i = 0; i < TwoBitArraySpan.GetRequiredLengthFor(arr.Length); i++ )
			{
				twoBitArray[i] = i & 3;
			}

			for ( int i = 0; i < TwoBitArraySpan.GetRequiredLengthFor(arr.Length); i++ )
			{
				Assert.Equal(i % 3, twoBitArray[i]);
			}
		}
	}
}
