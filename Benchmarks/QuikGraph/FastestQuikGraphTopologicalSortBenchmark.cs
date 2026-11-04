using System;
using System.Collections.Generic;
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
	public class FastestQuikGraphTopologicalSortBenchmark
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
			Random random = new(42);

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
		public void AdjacencyGraph_TopologicalSort ()
		{
			try
			{
				IEnumerable<int> result = adjacencyGraph.TopologicalSort();
			}
			catch { }
		}

		[Benchmark]
		public void ArrayAdjacencyGraph_TopologicalSort ()
		{
			try
			{
				IEnumerable<int> result = arrayAdjacencyGraph.TopologicalSort();
			}
			catch { }
		}

		[Benchmark(Baseline = true)]
		public void CompressedSparseRowGraph_TopologicalSort ()
		{
			try
			{
				IEnumerable<int> result = compressedSparseRowGraph.TopologicalSort();
			}
			catch { }
		}
	}
}
