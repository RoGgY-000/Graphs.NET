using System;
using System.Collections.Generic;
using BenchmarkDotNet.Attributes;
using Graphs.NET.Core;
using QuikGraph;

namespace Benchmarks.Building
{
	public abstract class GraphBuilder_RemoveVertex
	{
		public virtual int VertexCount { get; set; }
		public virtual int ArcCount { get; set; }

		protected GraphBuilder _graphBuilder;
		protected Dictionary<int, List<int>> _naiveGraph;
		protected AdjacencyGraph<int, Edge<int>> _quikGraphBuilder;

		[IterationSetup]
		public void Setup ()
		{
			Random random = new(42);

			_naiveGraph = new(VertexCount);
			_graphBuilder = new(VertexCount, ArcCount);
			_quikGraphBuilder = new(true, VertexCount, ArcCount);

			for ( int vertex = 0; vertex < VertexCount; vertex++ )
			{
				_naiveGraph.Add(vertex, []);

				_graphBuilder.AddVertex(vertex);

				_quikGraphBuilder.AddVertex(vertex);
				
			}

			for ( int i = 0; i < ArcCount; i++ )
			{
				int from = random.Next(VertexCount), to = random.Next(VertexCount);

				_naiveGraph[from].Add(to);

				_graphBuilder.AddArcAndVertices(from, to);

				_quikGraphBuilder.AddVerticesAndEdge(new Edge<int>(from, to));
			}
		}

		[Benchmark]
		public void Naive_Remove ()
		{
			for ( int vertex = 0; vertex < VertexCount; vertex++ )
			{
				_naiveGraph.Remove(vertex);
			}
		}

		[Benchmark(Baseline = true)]
		public void GraphsNET_RemoveVertex ()
		{
			for ( int vertex = 0; vertex < VertexCount; vertex++ )
			{
				_graphBuilder.RemoveVertex(vertex);
			}
		}
	}

	public abstract class GraphBuilder_RemoveVertex_WithQuikGraph : GraphBuilder_RemoveVertex
	{
		[Benchmark]
		public void QuikGraph_RemoveVertex ()
		{
			for ( int vertex = 0; vertex < VertexCount; vertex++ )
			{
				_quikGraphBuilder.RemoveVertex(vertex);
			}
		}
	}

	public class Small_GraphBuilder_RemoveVertex : GraphBuilder_RemoveVertex_WithQuikGraph
	{
		[Params(1, 10, 100)]
		public override int VertexCount { get; set; }
		[Params(1, 10, 100)]
		public override int ArcCount { get; set; }
	}

	public class Medium_GraphBuilder_RemoveVertex : GraphBuilder_RemoveVertex_WithQuikGraph
	{
		[Params(1_000, 5_000, 10_000)]
		public override int VertexCount { get; set; }
		[Params(1_000, 5_000, 10_000)]
		public override int ArcCount { get; set; }
	}

	public class Large_GraphBuilder_RemoveVertex : GraphBuilder_RemoveVertex
	{
		[Params(50_000, 100_000, 500_000)]
		public override int VertexCount { get; set; }
		[Params(50_000, 100_000, 500_000)]
		public override int ArcCount { get; set; }
	}
}
