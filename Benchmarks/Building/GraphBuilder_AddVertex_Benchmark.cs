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
	public abstract class GraphBuilder_AddVertex_Benchmark
	{
		public virtual int VertexCount { get; set; }

		private int[] _vertices;

		private GraphBuilder _graphBuilder;
		private Dictionary<int, List<int>> _naiveGraph;
		private AdjacencyGraph<int, Edge<int>> _quikGraphBuilder;

		[GlobalSetup]
		public void Setup ()
		{
			Random random = new(42);

			_vertices = new int[VertexCount];

			for ( int i = 0; i < VertexCount; i++ )
			{
				int vertex = random.Next(VertexCount);
				_vertices[i] = vertex;
			}
		}

		[Benchmark]
		[MaxIterationCount(25)]
		public void Naive_AddVertices ()
		{
			_naiveGraph = new(VertexCount);
			for ( int i = 0; i < VertexCount; i++ )
			{
				int vertex = _vertices[i];

				if ( !_naiveGraph.ContainsKey(vertex) )
				{
					_naiveGraph.Add(vertex, []);
				}
			}
		}

		[Benchmark]
		[MaxIterationCount(25)]
		public void QuikGraph_AddVertex ()
		{
			_quikGraphBuilder = new(true, VertexCount);
			for ( int i = 0; i < VertexCount; i++ )
			{
				int vertex = _vertices[i];

				_quikGraphBuilder.AddVertex(vertex);
			}
		}

		[Benchmark]
		[MaxIterationCount(25)]
		public void GraphsNET_AddVertex ()
		{
			_graphBuilder = new(VertexCount);
			for ( int i = 0; i < VertexCount; i++ )
			{
				int vertex = _vertices[i];

				_graphBuilder.AddVertex(vertex);
			}
		}

		[Benchmark]
		[MaxIterationCount(25)]
		public void QuikGraph_AddVertices ()
		{
			_quikGraphBuilder = new(true, VertexCount);
			_quikGraphBuilder.AddVertexRange(_vertices);
		}

		[Benchmark(Baseline = true)]
		[MaxIterationCount(25)]
		public void GraphsNET_AddVertices ()
		{
			_graphBuilder = new(VertexCount);
			_graphBuilder.AddVertices(_vertices);
		}
	}

	public class Small_GraphBuilder_AddVertex_Benchmark : GraphBuilder_AddVertex_Benchmark
	{
		[Params(1, 10, 100)]
		public override int VertexCount { get; set; }
	}

	public class Medium_GraphBuilder_AddVertex_Benchmark : GraphBuilder_AddVertex_Benchmark
	{
		[Params(1_000, 10_000, 100_000)]
		public override int VertexCount { get; set; }
	}

	public class Large_GraphBuilder_AddVertex_Benchmark : GraphBuilder_AddVertex_Benchmark
	{
		[Params(1_000_000, 100_000_000, 1_000_000_000)]
		public override int VertexCount { get; set; }
	}
}
