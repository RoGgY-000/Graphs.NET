using System;
using System.Collections.Generic;
using System.Text;
using BenchmarkDotNet.Attributes;
using Graphs.NET.Core;
using Microsoft.VSDiagnostics;
using QuikGraph;

namespace Benchmarks.Building
{
	[MemoryDiagnoser]
	[Orderer(BenchmarkDotNet.Order.SummaryOrderPolicy.FastestToSlowest)]
	[RankColumn]
	[HideColumns("Job", "RatioSD", "Error", "StdDev", "Median")]
	public abstract class GraphBuilder_Build_Benchmark
	{
		public virtual int VertexCount { get; set; }
		public virtual int ArcCount { get; set; }

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
		}

		[Benchmark]
		[MaxIterationCount(25)]
		public void QuikGraph_ToArrayAdjacencyGraph ()
		{
			arrayAdjacencyGraph = _adjacencyGraph.ToArrayAdjacencyGraph();
		}

		[Benchmark]
		[MaxIterationCount(25)]
		public void QuikGraph_ToCompressedSparseRowGraph ()
		{
			_csrGraph = _adjacencyGraph.ToCompressedRowGraph();
		}

		[Benchmark(Baseline = true)]
		[MaxIterationCount(25)]
		public void GraphsNET_Build ()
		{
			_graph = _graphBuilder.Build();
		}
	}

	public class Small_GraphBuilder_Build_Benchmark : GraphBuilder_Build_Benchmark
	{
		[Params(1, 10, 100)]
		public override int VertexCount { get; set; }
		[Params(1, 10, 100)]
		public override int ArcCount { get; set; }
	}

	public class Medium_GraphBuilder_Build_Benchmark : GraphBuilder_Build_Benchmark
	{
		[Params(1_000, 10_000, 50_000)]
		public override int VertexCount { get; set; }
		[Params(1_000, 10_000, 50_000)]
		public override int ArcCount { get; set; }
	}
}
