using System;
using BenchmarkDotNet.Attributes;
using Graphs.NET.Core;
using QuikGraph;

namespace Benchmarks.Building
{
	public abstract class GraphBuilder_Build
	{
		public virtual int VertexCapacity { get; set; }
		public virtual int ArcCount { get; set; }

		protected GraphBuilder _graphBuilder;
		protected Graph _graph;
		protected AdjacencyGraph<int, Edge<int>> _adjacencyGraph;
		protected CompressedSparseRowGraph<int> _csrGraph;
		protected ArrayAdjacencyGraph<int, Edge<int>> arrayAdjacencyGraph;

		[GlobalSetup]
		public void Setup ()
		{
			Random random = new(42);

			_graphBuilder = new(VertexCapacity, ArcCount);

			_adjacencyGraph = new(true, VertexCapacity, ArcCount);

			for ( int i = 0; i < ArcCount; i++ )
			{
				int from = random.Next(VertexCapacity), to = random.Next(VertexCapacity);

				_adjacencyGraph.AddVerticesAndEdge(new Edge<int>(from, to));

				_graphBuilder.AddArcAndVertices(from, to);
			}
		}

		[Benchmark(Baseline = true)]
		
		public void GraphsNET_Build ()
		{
			_graph = _graphBuilder.Build();
		}
	}

	public abstract class GraphBuilder_Build_WithQuikGraph : GraphBuilder_Build
	{
		[Benchmark]
		public void QuikGraph_ToArrayAdjacencyGraph ()
		{
			arrayAdjacencyGraph = _adjacencyGraph.ToArrayAdjacencyGraph();
		}

		[Benchmark]
		public void QuikGraph_ToCompressedSparseRowGraph ()
		{
			_csrGraph = _adjacencyGraph.ToCompressedRowGraph();
		}
	}

	public class Small_GraphBuilder_Build : GraphBuilder_Build_WithQuikGraph
	{
		[Params(1, 10, 100)]
		public override int VertexCapacity { get; set; }
		[Params(1, 10, 100)]
		public override int ArcCount { get; set; }
	}

	public class Medium_GraphBuilder_Build : GraphBuilder_Build_WithQuikGraph
	{
		[Params(1_000, 5_000, 10_000)]
		public override int VertexCapacity { get; set; }
		[Params(1_000, 5_000, 10_000)]
		public override int ArcCount { get; set; }
	}

	public class Large_GraphBuilder_Build : GraphBuilder_Build
	{
		[Params(50_000, 100_000, 500_000)]
		public override int VertexCapacity { get; set; }
		[Params(50_000, 100_000, 500_000)]
		public override int ArcCount { get; set; }
	}
}
