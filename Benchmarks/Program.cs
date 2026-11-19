using BenchmarkDotNet.Columns;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Diagnosers;
using BenchmarkDotNet.Jobs;
using BenchmarkDotNet.Order;
using BenchmarkDotNet.Running;

namespace GraphBenchmarks
{
	internal class Program
	{
		static void Main (string[] args)
		{
			ManualConfig config = DefaultConfig.Instance
				.AddJob(Job.Default.WithMaxIterationCount(25))
				.AddDiagnoser(MemoryDiagnoser.Default)
				.AddColumn(RankColumn.Arabic)
				.WithOrderer(new DefaultOrderer(SummaryOrderPolicy.FastestToSlowest))
				.HideColumns("Job", "RatioSD", "Error", "StdDev", "Median");

			BenchmarkSwitcher
				.FromAssembly(typeof(Program).Assembly)
				.Run(args, config);
		}
	}
}
