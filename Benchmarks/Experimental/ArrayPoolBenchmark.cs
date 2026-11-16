using System.Buffers;
using BenchmarkDotNet.Attributes;

namespace Benchmarks.Experimental
{
	public class ArrayPoolBenchmark
	{
		[Params(1, 10, 100, 1_000, 10_000, 100_000, 1_000_000, 10_000_000, 100_000_000, 1_000_000_000)]
		public int Length;

		private readonly ArrayPool<int> pool = ArrayPool<int>.Shared;
		private int[] array;

		[Benchmark]
		public int RentAndReturnArray ()
		{
			array = pool.Rent(Length);
			int length = array.Length;
			pool.Return(array);
			return length;
		}
	}
}
