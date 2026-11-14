using System;
using System.Collections.Generic;
using System.Text;
using BenchmarkDotNet.Attributes;
using Graphs.NET.Core;
using QuikGraph;

namespace Benchmarks.Building
{
	[MemoryDiagnoser]
	[Orderer(BenchmarkDotNet.Order.SummaryOrderPolicy.FastestToSlowest)]
	[RankColumn]
	[HideColumns("Job", "RatioSD", "Error", "StdDev", "Median")]
	public abstract class GraphBuilder_AddEdge_Benchmark
	{
		public virtual int VertexCapacity { get; set; }
		public virtual int EdgeCount { get; set; }

		private int[] _edges;

		private GraphBuilder _graphBuilder;
		private Dictionary<int, List<int>> _naiveGraph;
		private UndirectedGraph<int, Edge<int>> _quikGraphBuilder;

		[GlobalSetup]
		public void Setup ()
		{
			Random random = new(42);

			_edges = new int[EdgeCount * 2];

			for ( int i = 0; i < EdgeCount; i++ )
			{
				int from = random.Next(VertexCapacity), to = random.Next(VertexCapacity);
				_edges[i * 2] = from;
				_edges[i * 2 + 1] = to;
			}
		}

		[Benchmark]
		[MaxIterationCount(25)]
		public void Naive_AddEdge ()
		{
			_naiveGraph = new(VertexCapacity);
			for ( int i = 0; i < EdgeCount; i++ )
			{
				int from = _edges[i * 2], to = _edges[i * 2 + 1];

				if ( !_naiveGraph.ContainsKey(from) )
				{
					_naiveGraph.Add(from, []);
				}
				if ( !_naiveGraph.ContainsKey(to) )
				{
					_naiveGraph.Add(to, []);
				}

				_naiveGraph[from].Add(to);
				_naiveGraph[to].Add(from);
			}
		}

		[Benchmark]
		[MaxIterationCount(25)]
		public void QuikGraph_AddVerticesAndEdge ()
		{
			_quikGraphBuilder = new();
			for ( int i = 0; i < EdgeCount; i++ )
			{
				int from = _edges[i * 2], to = _edges[i * 2 + 1];

				_quikGraphBuilder.AddEdge(new Edge<int>(from, to));
				_quikGraphBuilder.AddEdge(new Edge<int>(to, from));
			}
		}

		[Benchmark(Baseline = true)]
		[MaxIterationCount(25)]
		public void GraphsNET_AddEdgeAndVertices ()
		{
			_graphBuilder = new(VertexCapacity, EdgeCount*2);
			for ( int i = 0; i < EdgeCount; i++ )
			{
				int from = _edges[i * 2], to = _edges[i * 2 + 1];

				_graphBuilder.AddEdgeAndVertices(from, to);
			}
		}
	}

	public class Small_GraphBuilder_AddEdge_Benchmark : GraphBuilder_AddEdge_Benchmark
	{
		[Params(1, 10, 100)]
		public override int VertexCapacity { get; set; }
		[Params(1, 10, 100)]
		public override int EdgeCount { get; set; }
	}

	public class Medium_GraphBuilder_AddEdge_Benchmark : GraphBuilder_AddEdge_Benchmark
	{
		[Params(1_000, 10_000, 50_000)]
		public override int VertexCapacity { get; set; }
		[Params(1_000, 10_000, 50_000)]
		public override int EdgeCount { get; set; }
	}
}
