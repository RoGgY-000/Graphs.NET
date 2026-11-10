using System;
using System.Collections.Generic;
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
	public class GraphAdditionEdgesBenchmark
	{
		[Params(1_000)]
		public int VertexCount;
		[Params(1_000)]
		public int ArcCount;

		private int[] _arcs;

		private GraphBuilder _graphBuilder;
		private Graph _graph;
		private Dictionary<int, List<int>> _naiveGraph;
		private AdjacencyGraph<int, Edge<int>> _quikGraphBuilder;
		private CompressedSparseRowGraph<int> _quikGraph;

		[GlobalSetup]
		public void Setup ()
		{
			Random random = new(42);

			_arcs = new int[ArcCount * 2];

			for ( int i = 0; i < ArcCount; i++ )
			{
				int from = random.Next(VertexCount), to = random.Next(VertexCount);
				_arcs[i * 2] = from;
				_arcs[i * 2 + 1] = to;
			}
		}

		[Benchmark]
		public void Naive_EdgesAddition ()
		{
			_naiveGraph = new(VertexCount);
			for ( int i = 0; i < ArcCount; i++ )
			{
				int from = _arcs[i * 2], to = _arcs[i * 2 + 1];

				if ( !_naiveGraph.ContainsKey(from) )
				{
					_naiveGraph.Add(from, []);
				}

				_naiveGraph[from].Add(to);
			}
		}

		[Benchmark]
		public void QuikGraph_EdgesAddition ()
		{
			_quikGraphBuilder = new(true, VertexCount, ArcCount);
			for ( int i = 0; i < ArcCount; i++ )
			{
				int from = _arcs[i * 2], to = _arcs[i * 2 + 1];

				_quikGraphBuilder.AddVerticesAndEdge(new Edge<int>(from, to));
			}
		}

		[Benchmark(Baseline = true)]
		public void GraphsNET_EdgesAddition ()
		{
			_graphBuilder = new(VertexCount, ArcCount);
			for ( int i = 0; i < ArcCount; i++ )
			{
				int from = _arcs[i * 2], to = _arcs[i * 2 + 1];

				_graphBuilder.AddArcAndVertices(from, to);
			}
		}
	}
}
