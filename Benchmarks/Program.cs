// -----------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------------------

using BenchmarkDotNet.Running;
using Benchmarks;
using Benchmarks.Algorithms;
using Benchmarks.QuikGraph;

namespace GraphBenchmarks
{
	internal class Program
	{
		static void Main (string[] args)
		{
			//var quikGraphCycleDetection = BenchmarkRunner.Run<FastestQuikGraphCycleDetectionBenchmark>();
			//var quikGraphTopologicalSort = BenchmarkRunner.Run<FastestQuikGraphTopologicalSortBenchmark>();

			var containCycle = BenchmarkRunner.Run<ContainCycleAlgorithmBenchmark>();
			//var topologicalSort = BenchmarkRunner.Run<TopologicalSortAlgorithmBenchmark>();
		}
	}
}
