// -----------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------------------

using BenchmarkDotNet.Reports;
using BenchmarkDotNet.Running;
using Benchmarks.Algorithms;
using Benchmarks.Building;

namespace GraphBenchmarks
{
	internal class Program
	{
		static void Main ()
		{
			//Summary quikGraphCycleDetection = BenchmarkRunner.Run<FastestQuikGraphCycleDetectionBenchmark>();
			//Summary quikGraphTopologicalSort = BenchmarkRunner.Run<FastestQuikGraphTopologicalSortBenchmark>();

			//Summary containCycle = BenchmarkRunner.Run<ContainCycleAlgorithmBenchmark>();
			//Summary topologicalSort = BenchmarkRunner.Run<TopologicalSortAlgorithmBenchmark>();

			//Summary addition = BenchmarkRunner.Run<GraphAdditionEdgesBenchmark>();
			Summary building = BenchmarkRunner.Run<GraphBuildingBenchmark>();
			//Summary full = BenchmarkRunner.Run<GraphFullLifecycleBenchmark>();
		}
	}
}
