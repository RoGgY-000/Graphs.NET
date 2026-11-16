using System;
using System.Collections.Generic;
using BenchmarkDotNet.Attributes;
using Graphs.NET.Core;
using QuikGraph;

namespace Benchmarks.Building
{
	public abstract class GraphBuilder_AddEdge
	{
		public virtual int VertexCapacity { get; set; }
		public virtual int EdgeCount { get; set; }

		protected int[] _edges;

		protected GraphBuilder _graphBuilder;
		protected Dictionary<int, List<int>> _naiveGraph;
		protected UndirectedGraph<int, Edge<int>> _quikGraphBuilder;

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
		public void Naive_Add ()
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

		[Benchmark(Baseline = true)]
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

	public abstract class GraphBuilder_AddEdge_WithQuikGraph : GraphBuilder_AddEdge
	{
		[Benchmark]
		public void QuikGraph_AddVerticesAndEdge ()
		{
			_quikGraphBuilder = new(true);
			for ( int i = 0; i < EdgeCount; i++ )
			{
				int from = _edges[i * 2], to = _edges[i * 2 + 1];

				_quikGraphBuilder.AddVerticesAndEdge(new Edge<int>(from, to));
			}
		}
	}

	public class Small_GraphBuilder_AddEdge : GraphBuilder_AddEdge_WithQuikGraph
	{
		[Params(1, 10, 100)]
		public override int VertexCapacity { get; set; }
		[Params(1, 10, 100)]
		public override int EdgeCount { get; set; }
	}

	public class Medium_GraphBuilder_AddEdge : GraphBuilder_AddEdge_WithQuikGraph
	{
		[Params(1_000, 5_000, 10_000)]
		public override int VertexCapacity { get; set; }
		[Params(1_000, 5_000, 10_000)]
		public override int EdgeCount { get; set; }
	}

	public class Large_GraphBuilder_AddEdge : GraphBuilder_AddEdge
	{
		[Params(50_000, 100_000, 500_000)]
		public override int VertexCapacity { get; set; }
		[Params(50_000, 100_000, 500_000)]
		public override int EdgeCount { get; set; }
	}
}
