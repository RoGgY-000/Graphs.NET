using System;
using System.Collections.Generic;
using BenchmarkDotNet.Attributes;
using Benchmarks.Extensions;
using Graphs.NET.Core;
using Microsoft.VSDiagnostics;
using QuikGraph;
using QuikGraph.Algorithms;

namespace Benchmarks.Algorithms
{
	[CPUUsageDiagnoser]
	[MemoryDiagnoser]
	[Orderer(BenchmarkDotNet.Order.SummaryOrderPolicy.FastestToSlowest)]
	[RankColumn]
	public class ContainCycleAlgorithmBenchmark
	{
		[Params(10_000)]
		public int VertexCount;
		[Params(1_000_000)]
		public int ArcCount;

		private GraphBuilder _graphBuilder;
		private Graph _graph;
		private Dictionary<int, List<int>> _naiveGraph;
		private AdjacencyGraph<int, Edge<int>> _quikGraphBuilder;
		private CompressedSparseRowGraph<int> _quikGraph;

		[GlobalSetup]
		public void Setup ()
		{
			Random random = new(42);

			_graphBuilder = new GraphBuilder(VertexCount, ArcCount);
			_naiveGraph = new Dictionary<int, List<int>>(VertexCount);
			_quikGraphBuilder = new AdjacencyGraph<int, Edge<int>>(true);

			for ( int i = 0; i < VertexCount; i++ )
			{
				_quikGraphBuilder.AddVertex(i);
			}

			for ( int i = 0; i < ArcCount; i++ )
			{
				int from = random.Next(0, VertexCount), to = random.Next(0, VertexCount);

				_graphBuilder.AddArc(from, to);

				if ( !_naiveGraph.ContainsKey(from) )
				{
					_naiveGraph.Add(from, []);
				}
				_naiveGraph[from].Add(to);

				_quikGraphBuilder.AddEdge(new Edge<int>(from, to));
			}
			_graph = _graphBuilder.Build();
			_quikGraph = _quikGraphBuilder.ToCompressedRowGraph();
		}

		[Benchmark]
		public bool Naive_CycleDetection ()
		{
			return NaiveGraphExtensions.RunNaiveCycleDetection(_naiveGraph, VertexCount);
		}

		[Benchmark]
		public bool QuikGraph_CycleDetection ()
		{
			return !_quikGraph.IsDirectedAcyclicGraph();
		}

		[Benchmark(Baseline = true)]
		public bool GraphsNET_CycleDetection ()
		{
			_graph._isAcyclicCalculated = false;
			return _graph.CalculateIsAcyclic();
		}
	}
}
