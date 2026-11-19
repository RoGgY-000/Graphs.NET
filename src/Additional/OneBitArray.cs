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
				array.CopyTo(newArray);
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
		private void CopyTo (OneBitArray other)
		{
			_array.AsSpan().CopyTo(other._array.AsSpan());
		}
	}
}
