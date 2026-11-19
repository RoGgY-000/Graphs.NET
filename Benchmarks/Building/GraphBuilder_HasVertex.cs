using System;
using System.Collections.Generic;
using BenchmarkDotNet.Attributes;
using Graphs.NET.Core;
using QuikGraph;

namespace Benchmarks.Building
{
	public abstract class GraphBuilder_HasVertex
	{
		public virtual int VertexCapacity { get; set; }
		public virtual int ArcCount { get; set; }

		protected int[] _vertices;

		protected GraphBuilder _graphBuilder;
		protected Dictionary<int, List<int>> _naiveGraph;
		protected AdjacencyGraph<int, Edge<int>> _adjacencyGraph;

		[GlobalSetup]
		public void Setup ()
		{
			Random random = new(42);

			_vertices = new int[VertexCapacity];

			for ( int i = 0; i < ArcCount; i++ )
			{
				int from = random.Next(VertexCapacity), to = random.Next(VertexCapacity);
				_vertices[i * 2] = from;
				_vertices[i * 2 + 1] = to;

				if ( !_naiveGraph.ContainsKey(from) )
				{
					_naiveGraph.Add(from, []);
				}
				if ( !_naiveGraph.ContainsKey(to) )
				{
					_naiveGraph.Add(to, []);
				}

				_naiveGraph[from].Add(to);

				_graphBuilder.AddArcAndVertices(from, to);

				_adjacencyGraph.AddVerticesAndEdge(new Edge<int>(from, to));
			}
		}

		[Benchmark]
		public void Naive_Contains ()
		{
			for ( int i = 0; i < ArcCount; i++ )
			{
				int from = _vertices[i * 2], to = _vertices[i * 2 + 1];

				_naiveGraph.ContainsKey(from);
				_naiveGraph.ContainsKey(to);
			}
		}

		[Benchmark(Baseline = true)]
		public void GraphsNET_HasArc ()
		{
			bool contains = false;
			for ( int i = 0; i < ArcCount; i++ )
			{
				int from = _vertices[i * 2], to = _vertices[i * 2 + 1];

				contains = _graphBuilder.HasArc(from, to);
			}
		}
	}

	public abstract class GraphBuilder_HasVertex_WithQuikGraph : GraphBuilder_HasVertex
	{
		[Benchmark]
		public void QuikGraph_ContainsEdge ()
		{
			bool contains = false;
			for ( int i = 0; i < ArcCount; i++ )
			{
				int from = _vertices[i * 2], to = _vertices[i * 2 + 1];

				contains = _adjacencyGraph.ContainsEdge(new Edge<int>(from, to));
			}
		}
	}

	public class Small_GraphBuilder_HasVertex : GraphBuilder_HasVertex_WithQuikGraph
	{
		[Params(1, 10, 100)]
		public override int VertexCapacity { get; set; }
		[Params(1, 10, 100)]
		public override int ArcCount { get; set; }
	}

	public class Medium_GraphBuilder_HasVertex : GraphBuilder_HasVertex_WithQuikGraph
	{
		[Params(1_000, 5_000, 10_000)]
		public override int VertexCapacity { get; set; }
		[Params(1_000, 5_000, 10_000)]
		public override int ArcCount { get; set; }
	}

	public class Large_GraphBuilder_HasVertex : GraphBuilder_HasVertex
	{
		[Params(50_000, 100_000, 500_000)]
		public override int VertexCapacity { get; set; }
		[Params(50_000, 100_000, 500_000)]
		public override int ArcCount { get; set; }
	}
}
