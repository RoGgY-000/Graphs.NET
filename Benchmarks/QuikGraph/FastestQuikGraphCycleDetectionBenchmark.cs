using System;
using System.Collections.Generic;
using System.Text;
using BenchmarkDotNet.Attributes;
using Microsoft.VSDiagnostics;
using QuikGraph;
using QuikGraph.Algorithms;

namespace Benchmarks.QuikGraph
{
	[CPUUsageDiagnoser]
	[MemoryDiagnoser]
	[Orderer(BenchmarkDotNet.Order.SummaryOrderPolicy.FastestToSlowest)]
	[RankColumn]
	public class FastestQuikGraphCycleDetectionBenchmark
	{
		[Params(100)]
		public int VertexCount;

		[Params(10, 100, 1_000)]
		public int ArcCount;

		private AdjacencyGraph<int, Edge<int>> adjacencyGraph;
		private ArrayAdjacencyGraph<int, Edge<int>> arrayAdjacencyGraph;
		private CompressedSparseRowGraph<int> compressedSparseRowGraph;

		[GlobalSetup]
		public void Setup ()
		{
			Random random = new Random(42);

			adjacencyGraph = new();

			for ( int i = 0; i < VertexCount; i++ )
			{
				adjacencyGraph.AddVertex(i);
			}

			for ( int i = 0; i < ArcCount; i++ )
			{
				int from = random.Next(0, VertexCount), to = random.Next(0, VertexCount);

				adjacencyGraph.AddEdge(new Edge<int>(from, to));
			}
			arrayAdjacencyGraph = adjacencyGraph.ToArrayAdjacencyGraph();
			compressedSparseRowGraph = adjacencyGraph.ToCompressedRowGraph();
		}

		[Benchmark]
		public bool AdjacencyGraph_ContainCycle ()
		{
			return !adjacencyGraph.IsDirectedAcyclicGraph();
		}

		[Benchmark]
		public bool ArrayAdjacencyGraph_ContainCycle ()
		{
			return !arrayAdjacencyGraph.IsDirectedAcyclicGraph();
		}

		[Benchmark(Baseline = true)]
		public bool CompressedSparseRowGraph_ContainCycle ()
		{
			return !compressedSparseRowGraph.IsDirectedAcyclicGraph();
		}
	}
}
