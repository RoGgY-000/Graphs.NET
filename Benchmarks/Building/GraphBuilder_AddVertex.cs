using System;
using System.Collections.Generic;
using System.Text;
using BenchmarkDotNet.Attributes;
using Graphs.NET.Core;
using QuikGraph;

namespace Benchmarks.Building
{
	public abstract class GraphBuilder_AddVertex
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
		
		public void Naive_Add ()
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
		
		public void QuikGraph_AddVertices ()
		{
			_quikGraphBuilder = new(true, VertexCount);
			_quikGraphBuilder.AddVertexRange(_vertices);
		}

		[Benchmark(Baseline = true)]
		
		public void GraphsNET_AddVertices ()
		{
			_graphBuilder = new(VertexCount);
			_graphBuilder.AddVertices(_vertices);
		}
	}

	public class Small_GraphBuilder_AddVertex : GraphBuilder_AddVertex
	{
		[Params(1, 10, 100)]
		public override int VertexCount { get; set; }
	}

	public class Medium_GraphBuilder_AddVertex : GraphBuilder_AddVertex
	{
		[Params(1_000, 5_000, 10_000)]
		public override int VertexCount { get; set; }
	}

	public class Large_GraphBuilder_AddVertex : GraphBuilder_AddVertex
	{
		[Params(100_000, 500_000, 1_000_000)]
		public override int VertexCount { get; set; }
	}
}
