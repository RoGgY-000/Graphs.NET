using System;
using System.Collections.Generic;
using BenchmarkDotNet.Attributes;
using Graphs.NET.Core;
using QuikGraph;

namespace Benchmarks.Building
{
	public abstract class GraphBuilder_RemoveArc
	{
		public virtual int VertexCapacity { get; set; }
		public virtual int ArcCount { get; set; }

		protected int[] _arcs;

		protected GraphBuilder _graphBuilder;
		protected Dictionary<int, List<int>> _naiveGraph;
		protected AdjacencyGraph<int, Edge<int>> _quikGraphBuilder;

		[IterationSetup]
		public void Setup ()
		{
			Random random = new(42);

			_arcs = new int[ArcCount * 2];

			for ( int i = 0; i < ArcCount; i++ )
			{
				int from = random.Next(VertexCapacity), to = random.Next(VertexCapacity);
				_arcs[i * 2] = from;
				_arcs[i * 2 + 1] = to;

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

				_quikGraphBuilder.AddVerticesAndEdge(new Edge<int>(from, to));
			}
		}

		[Benchmark]
		public void Naive_Remove ()
		{
			for ( int i = 0; i < ArcCount; i++ )
			{
				int from = _arcs[i * 2], to = _arcs[i * 2 + 1];

				_naiveGraph[from].Remove(to);
			}
		}

		[Benchmark(Baseline = true)]
		public void GraphsNET_RemoveArc ()
		{
			for ( int i = 0; i < ArcCount; i++ )
			{
				int from = _arcs[i * 2], to = _arcs[i * 2 + 1];

				_graphBuilder.RemoveArc(from, to);
			}
		}
	}

	public abstract class GraphBuilder_RemoveArc_WithQuikGraph : GraphBuilder_RemoveArc
	{
		[Benchmark]
		public void QuikGraph_RemoveEdge ()
		{
			for ( int i = 0; i < ArcCount; i++ )
			{
				int from = _arcs[i * 2], to = _arcs[i * 2 + 1];

				_quikGraphBuilder.RemoveEdge(new Edge<int>(from, to));
			}
		}
	}

	public class Small_GraphBuilder_RemoveArc : GraphBuilder_RemoveArc_WithQuikGraph
	{
		[Params(1, 10, 100)]
		public override int VertexCapacity { get; set; }
		[Params(1, 10, 100)]
		public override int ArcCount { get; set; }
	}

	public class Medium_GraphBuilder_RemoveArc : GraphBuilder_RemoveArc_WithQuikGraph
	{
		[Params(1_000, 5_000, 10_000)]
		public override int VertexCapacity { get; set; }
		[Params(1_000, 5_000, 10_000)]
		public override int ArcCount { get; set; }
	}

	public class Large_GraphBuilder_RemoveArc : GraphBuilder_RemoveArc
	{
		[Params(50_000, 100_000, 500_000)]
		public override int VertexCapacity { get; set; }
		[Params(50_000, 100_000, 500_000)]
		public override int ArcCount { get; set; }
	}
}
