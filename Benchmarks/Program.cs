using BenchmarkDotNet.Running;

namespace GraphBenchmarks
{
	internal class Program
	{
		static void Main ()
		{
			BenchmarkRunner.Run(typeof(Program).Assembly);
		}
	}
}
