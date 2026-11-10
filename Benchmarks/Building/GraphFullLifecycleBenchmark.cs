using System;
using System.Collections.Generic;
using System.Text;
using BenchmarkDotNet.Attributes;
using Graphs.NET.Core;
using Graphs.NET.Algorithms;
using Microsoft.VSDiagnostics;
using QuikGraph;
using QuikGraph.Algorithms;
using System.Collections;

namespace Benchmarks.Building
{
	[CPUUsageDiagnoser]
	[MemoryDiagnoser]
	[Orderer(BenchmarkDotNet.Order.SummaryOrderPolicy.FastestToSlowest)]
	[RankColumn]
	[HideColumns("Job", "RatioSD", "Error", "StdDev", "Median")]
	public class GraphFullLifecycleBenchmark
	{
		[Params(1, 10, 100, 1_000)]
		public int VertexCount;
		[Params(1, 10, 100, 1_000)]
		public int ArcCount;

		private int _newArcFrom;
		private int[] _arrayVertexCount;
		private int[] _arrayVertexCountPlusOne;

		private int[] _arcs;

		private GraphBuilder _graphBuilder;
		private Graph _graph;
		private AdjacencyGraph<int, Edge<int>> _adjacencyGraph;

		[GlobalSetup]
		public void Setup ()
		{
			Random random = new(42);

			_arrayVertexCount = new int[VertexCount];
			_arrayVertexCountPlusOne = new int[VertexCount + 1];

			_arcs = new int[ArcCount * 2];

			for ( int i = 0; i < ArcCount; i++ )
			{
				int from = random.Next(VertexCount), to = random.Next(VertexCount);

				_arcs[i * 2] = from;
				_arcs[i * 2 + 1] = to;
			}

			_newArcFrom = random.Next(VertexCount);
		}

		[Benchmark]
		public void QuikGraph_TopologicalSortWithAddVertexAdjacencyGraph ()
		{
			_adjacencyGraph = new(true, VertexCount, ArcCount);
			for ( int i = 0; i < ArcCount; i++ )
			{
				int from = _arcs[i * 2], to = _arcs[i * 2 + 1];

				_adjacencyGraph.AddVerticesAndEdge(new Edge<int>(from, to));
			}

			try
			{
				IEnumerable<int> res = _adjacencyGraph.TopologicalSort();
			}
			catch { }

			_adjacencyGraph.AddVerticesAndEdge(new Edge<int>(_newArcFrom, VertexCount));

			try
			{
				IEnumerable<int> res = _adjacencyGraph.TopologicalSort();
			}
			catch { }
		}

		[Benchmark(Baseline = true)]
		public void GraphsNET_TopologicalSortWithAddVertex ()
		{
			_graphBuilder = new(VertexCount, ArcCount);
			for ( int i = 0; i < ArcCount; i++ )
			{
				int from = _arcs[i * 2], to = _arcs[i * 2 + 1];

				_graphBuilder.AddArcAndVertices(from, to);
			}

			_graph = _graphBuilder.Build();
			_graph.TryTopologicalSort(_arrayVertexCount.AsSpan());

			_graphBuilder.AddArcAndVertices(_newArcFrom, VertexCount);

			_graph = _graphBuilder.Build();
			_graph.TryTopologicalSort(_arrayVertexCountPlusOne.AsSpan());
		}
	}
}
