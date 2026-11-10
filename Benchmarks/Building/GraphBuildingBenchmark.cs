using System;
using System.Collections.Generic;
using System.Text;
using BenchmarkDotNet.Attributes;
using Graphs.NET.Core;
using Microsoft.VSDiagnostics;
using QuikGraph;

namespace Benchmarks.Building
{
	[CPUUsageDiagnoser]
	[MemoryDiagnoser]
	[Orderer(BenchmarkDotNet.Order.SummaryOrderPolicy.FastestToSlowest)]
	[RankColumn]
	[HideColumns("Job", "RatioSD", "Error", "StdDev", "Median")]
	public class GraphBuildingBenchmark
	{
		[Params(1_000)]
		public int VertexCount;
		[Params(1_000)]
		public int ArcCount;

		private GraphBuilder _graphBuilder;
		private Graph _graph;
		private AdjacencyGraph<int, Edge<int>> _adjacencyGraph;
		private CompressedSparseRowGraph<int> _csrGraph;
		private ArrayAdjacencyGraph<int, Edge<int>> arrayAdjacencyGraph;

		[GlobalSetup]
		public void Setup ()
		{
			Random random = new(42);

			_graphBuilder = new(VertexCount, ArcCount);

			_adjacencyGraph = new(true, VertexCount, ArcCount);

			for ( int i = 0; i < ArcCount; i++ )
			{
				int from = random.Next(VertexCount), to = random.Next(VertexCount);

				_adjacencyGraph.AddVerticesAndEdge(new Edge<int>(from, to));

				_graphBuilder.AddArcAndVertices(from, to);
			}
			Console.WriteLine(_graphBuilder.EdgeCapacity);
			Console.WriteLine(_graphBuilder.EdgeCount);
			Console.WriteLine(_graphBuilder.VertexCapacity);
			Console.WriteLine(_graphBuilder.VertexCount);
		}

		//[Benchmark]
		//public void QuikGraph_ToArrayAdjacencyGraph ()
		//{
		//	arrayAdjacencyGraph = _adjacencyGraph.ToArrayAdjacencyGraph();
		//}

		//[Benchmark]
		//public void QuikGraph_ToCompressedSparseRowGraph ()
		//{
		//	_csrGraph = _adjacencyGraph.ToCompressedRowGraph();
		//}

		[Benchmark(Baseline = true)]
		public void GraphsNET_Build ()
		{
			_graph = _graphBuilder.Build();
		}
	}
}
