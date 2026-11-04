using System;
using System.Buffers;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;

namespace Graphs.NET
{
	internal readonly ref struct TwoBitArray
	{
		private readonly Span<int> _buffer;
		private readonly int _capacity;

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public TwoBitArray (Span<int> buffer)
		{
			_buffer = buffer;
			_capacity = buffer.Length << 4;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static int GetRequiredLengthFor (int itemCount)
		{
			ArgumentOutOfRangeException.ThrowIfNegative(itemCount);

			return (itemCount + 15) >> 4;
		}

		public readonly int this[int index]
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get
			{
				ArgumentOutOfRangeException.ThrowIfNegative(index);
				ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(index, _capacity);

				int intIndex = index >> 4;
				int bitOffset = (index & 15) << 1;
				return (_buffer[intIndex] >> bitOffset) & 3;
			}

			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			set
			{
				ArgumentOutOfRangeException.ThrowIfNegative(index);
				ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(index, _capacity);

				ArgumentOutOfRangeException.ThrowIfNegative(value);
				ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(value, 4);

				int intIndex = index >> 4;
				int bitOffset = (index & 15) << 1;
				int clearingMask = ~(3 << bitOffset);
				_buffer[intIndex] = (_buffer[intIndex] & clearingMask) | (value << bitOffset);
			}
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public readonly void Clear ()
		{
			_buffer.Clear();
		}
	}
}
