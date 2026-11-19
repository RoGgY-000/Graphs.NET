using Graphs.NET.Additional;

namespace Tests.Graphs.NET.Additional
{
	public class OneBitArrayTests
	{
		[Fact]
		public void Length_OneBitArrayTest ()
		{
			for ( int i = 0; i < 100; i++ )
			{
				OneBitArray oneBitArray = new(i);
				Assert.Equal(i, oneBitArray.Length);
			}
		}

		[Fact]
		public void ReadingZero_OneBitArrayTest ()
		{
			OneBitArray oneBitArray = new(100);

			for ( int i = 0; i < oneBitArray.Length; i++ )
			{
				Assert.Equal(0, oneBitArray[i]);
			}
		}

		[Fact]
		public void WritingOneInPlace_OneBitArrayTest ()
		{
			OneBitArray oneBitArray = new(100);

			for ( int i = 0; i < oneBitArray.Length; i++ )
			{
				oneBitArray[i] = 1;
				Assert.Equal(1, oneBitArray[i]);
			}
		}

		[Fact]
		public void WritingOne_OneBitArrayTest ()
		{
			OneBitArray oneBitArray = new(100);

			for ( int i = 0; i < oneBitArray.Length; i++ )
			{
				oneBitArray[i] = 1;
			}
			for ( int i = 0; i < oneBitArray.Length; i++ )
			{
				Assert.Equal(1, oneBitArray[i]);
			}
		}

		[Fact]
		public void WritingZero_OneBitArrayTest ()
		{
			OneBitArray oneBitArray = new(100);

			for ( int i = 0; i < oneBitArray.Length; i++ )
			{
				oneBitArray[i] = 1;
			}
			for ( int i = 0; i < oneBitArray.Length; i++ )
			{
				oneBitArray[i] = 0;
			}
			for ( int i = 0; i < oneBitArray.Length; i++ )
			{
				Assert.Equal(0, oneBitArray[i]);
			}
		}

		[Fact]
		public void WritingOnesAndZeros_OneBitArrayTest ()
		{
			OneBitArray oneBitArray = new(100);

			for ( int i = 0; i < oneBitArray.Length; i++ )
			{
				oneBitArray[i] = i & 1;
			}
			for ( int i = 0; i < oneBitArray.Length; i++ )
			{
				Assert.Equal(i & 1, oneBitArray[i]);
			}
		}
	}
}
