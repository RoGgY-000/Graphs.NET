using System.Runtime.CompilerServices;

namespace Graphs.NET.Additional
{
	internal readonly ref struct TwoBitArraySpan
	{
		private readonly Span<int> _buffer;

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public TwoBitArraySpan (Span<int> buffer)
		{
			_buffer = buffer;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static int GetRequiredLengthFor (int itemCount) => (itemCount + 15) >> 4;

		public readonly int this[int index]
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get
			{
				int intIndex = index >> 4;
				int bitOffset = (index & 15) << 1;
				return (_buffer[intIndex] >> bitOffset) & 3;
			}

			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			set
			{
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
