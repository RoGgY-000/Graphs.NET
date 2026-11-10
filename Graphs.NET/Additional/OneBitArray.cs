using System.Runtime.CompilerServices;

namespace Graphs.NET.Additional
{
	internal class OneBitArray
	{
		private readonly int[] _array;

		public static OneBitArray Empty = new(0);

		public int Length { get; private set; }

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public OneBitArray (int capacity)
		{
			_array = new int[GetRequiredLengthFor(capacity)];
			Length = capacity;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public OneBitArray (OneBitArray other)
		{
			_array = new int[other._array.Length];
			other.CopyTo(this);
			Length = other.Length;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static int GetRequiredLengthFor (int itemCount) => (itemCount + 31) >> 5;

		public static void Resize (ref OneBitArray array, int newCapacity)
		{
			if ( array == null )
			{
				array = new OneBitArray(newCapacity);
			}
			else if ( array.Length < newCapacity )
			{
				OneBitArray newArray = new(newCapacity);
				array = newArray;
			}
		}

		public int this[int index]
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get
			{
				int intIndex = index >> 5;
				int bitOffset = index & 31;
				return (_array[intIndex] >> bitOffset) & 1;
			}

			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			set
			{
				int intIndex = index >> 5;
				int bitOffset = index & 31;
				int clearingMask = ~(1 << bitOffset);
				_array[intIndex] = (_array[intIndex] & clearingMask) | (value << bitOffset);
			}
		}
		
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void SetRange (int start, int length, int value)
		{
			int end = start + length;

			int startIntIndex = start >> 5;
			int endIntIndex = end >> 5;

			int startOffset = start & 31;
			int endOffset = end & 31;

			int mask;

			if ( startIntIndex == endIntIndex )
			{
				mask = ((1 << length) - 1) << startOffset;

				if ( value == 1 )
				{
					_array[startIntIndex] |= mask;
				}
				else
				{
					_array[startIntIndex] &= ~mask;
				}
				return;
			}

			if ( startOffset > 0 )
			{
				mask = ~0 << startOffset;

				if ( value == 1 )
				{
					_array[startIntIndex] |= mask;
				}
				else
				{
					_array[startIntIndex] &= ~mask;
				}

				startIntIndex++;
			}

			int middleLength = endIntIndex - startIntIndex;

			if ( middleLength > 0 )
			{
				mask = value == 0 ? 0 : ~0;

				_array.AsSpan(startIntIndex, middleLength).Fill(mask);
			}

			if ( endOffset > 0 )
			{
				mask = (1 << endOffset) - 1;

				if ( value == 1 )
				{
					_array[endIntIndex] |= mask;
				}
				else
				{
					_array[endIntIndex] &= ~mask;
				}
			}
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private void CopyTo (OneBitArray other)
		{
			_array.AsSpan().CopyTo(other._array.AsSpan());
		}
	}
}
