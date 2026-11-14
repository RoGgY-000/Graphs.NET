using System;
using System.Collections.Generic;
using BenchmarkDotNet.Attributes;
using Graphs.NET.Core;
using Microsoft.VSDiagnostics;
using QuikGraph;

namespace Benchmarks.Building
{
	[MemoryDiagnoser]
	[Orderer(BenchmarkDotNet.Order.SummaryOrderPolicy.FastestToSlowest)]
	[RankColumn]
	[HideColumns("Job", "RatioSD", "Error", "StdDev", "Median")]
	public abstract class GraphBuilder_AddArc_Benchmark
	{
		public virtual int VertexCapacity { get; set; }
		public virtual int ArcCount { get; set; }

		private int[] _arcs;

		private GraphBuilder _graphBuilder;
		private Dictionary<int, List<int>> _naiveGraph;
		private AdjacencyGraph<int, Edge<int>> _quikGraphBuilder;

		[GlobalSetup]
		public void Setup ()
		{
			Random random = new(42);

			_arcs = new int[ArcCount * 2];

			for ( int i = 0; i < ArcCount; i++ )
			{
				int from = random.Next(VertexCapacity), to = random.Next(VertexCapacity);
				_arcs[i * 2] = from;
				_arcs[i * 2 + 1] = to;
			}
		}

		[Benchmark]
		[MaxIterationCount(25)]
		public void Naive_AddArcs ()
		{
			_naiveGraph = new(VertexCapacity);
			for ( int i = 0; i < ArcCount; i++ )
			{
				int from = _arcs[i * 2], to = _arcs[i * 2 + 1];

				if ( !_naiveGraph.ContainsKey(from) )
				{
					_naiveGraph.Add(from, []);
				}
				if ( !_naiveGraph.ContainsKey(to) )
				{
					_naiveGraph.Add(to, []);
				}

				_naiveGraph[from].Add(to);
			}
		}

		[Benchmark]
		[MaxIterationCount(25)]
		public void QuikGraph_AddVerticesAndEdge ()
		{
			_quikGraphBuilder = new(true, VertexCapacity, ArcCount);
			for ( int i = 0; i < ArcCount; i++ )
			{
				int from = _arcs[i * 2], to = _arcs[i * 2 + 1];

				_quikGraphBuilder.AddVerticesAndEdge(new Edge<int>(from, to));
			}
		}

		[Benchmark(Baseline = true)]
		[MaxIterationCount(25)]
		public void GraphsNET_AddArcAndVertices ()
		{
			_graphBuilder = new(VertexCapacity, ArcCount);
			for ( int i = 0; i < ArcCount; i++ )
			{
				int from = _arcs[i * 2], to = _arcs[i * 2 + 1];

				_graphBuilder.AddArcAndVertices(from, to);
			}
		}
	}

	public class Small_GraphBuilder_AddArc_Benchmark : GraphBuilder_AddArc_Benchmark
	{
		[Params(1, 10, 100)]
		public override int VertexCapacity { get; set; }
		[Params(1, 10, 100)]
		public override int ArcCount { get; set; }
	}

	public class Medium_GraphBuilder_AddArc_Benchmark : GraphBuilder_AddArc_Benchmark
	{
		[Params(1_000, 10_000, 50_000)]
		public override int VertexCapacity { get; set; }
		[Params(1_000, 10_000, 50_000)]
		public override int ArcCount { get; set; }
	}
}
