using System;
using System.Collections.Generic;
using BenchmarkDotNet.Attributes;
using Benchmarks.Extensions;
using Graphs.NET.Core;
using Graphs.NET.Algorithms;
using QuikGraph;
using QuikGraph.Algorithms;

namespace Benchmarks.Algorithms
{
	public abstract class Graph_ShortestDistances_Base
	{
		protected const int QueryCount = 100;

		public virtual int VertexCapacity { get; set; }
		public virtual int ArcCount { get; set; }

		protected int[] _distances;
		protected int[] _existingVertices;
		protected int[] _sources;
		protected Graph _graph;

		[GlobalSetup]
		public virtual void Setup ()
		{
			Random random = new(42);

			_distances = new int[VertexCapacity];
			GraphBuilder graphBuilder = new(VertexCapacity, ArcCount);

			HashSet<int> addedVertices = new(Math.Min(VertexCapacity, ArcCount * 2));

			for ( int i = 0; i < ArcCount; i++ )
			{
				int from = random.Next(VertexCapacity);
				int to = random.Next(VertexCapacity);

				graphBuilder.AddArcAndVertices(from, to);
				addedVertices.Add(from);
				addedVertices.Add(to);
			}

			_graph = graphBuilder.Build();

			_existingVertices = [.. addedVertices];
			_sources = new int[QueryCount];

			if ( _existingVertices.Length > 0 )
			{
				for ( int i = 0; i < QueryCount; i++ )
				{
					_sources[i] = _existingVertices[random.Next(_existingVertices.Length)];
				}
			}
		}

		[Benchmark(OperationsPerInvoke = QueryCount, Baseline = true)]
		public int GraphsNET_ShortestDistances ()
		{
			int checksum = 0;
			foreach ( int source in _sources )
			{
				_graph.GetShortestDistances(source, _distances.AsSpan());
				checksum += _distances[0];
			}
			return checksum;
		}
	}

	public abstract class Graph_ShortestDistances_WithNaive : Graph_ShortestDistances_Base
	{
		protected Dictionary<int, List<int>> _naiveGraph;

		public override void Setup ()
		{
			base.Setup();

			Random random = new(42);
			_naiveGraph = new(VertexCapacity);

			for ( int i = 0; i < ArcCount; i++ )
			{
				int from = random.Next(VertexCapacity);
				int to = random.Next(VertexCapacity);

				if ( !_naiveGraph.TryGetValue(from, out List<int> neighbors) )
				{
					neighbors = [];
					_naiveGraph.Add(from, neighbors);
				}
				if ( !_naiveGraph.ContainsKey(to) )
				{
					_naiveGraph.Add(to, []);
				}
				neighbors.Add(to);
			}
		}

		[Benchmark(OperationsPerInvoke = QueryCount)]
		public int Naive_ShortestDistances ()
		{
			int checksum = 0;
			foreach ( int source in _sources )
			{
				NaiveGraphExtensions.RunNaiveShortestDistances(_naiveGraph, source, _distances.AsSpan());
				checksum += _distances[0];
			}
			return checksum;
		}
	}

	public abstract class Graph_ShortestDistances_WithQuikGraph : Graph_ShortestDistances_WithNaive
	{
		protected AdjacencyGraph<int, Edge<int>> _quikGraph;

		public override void Setup ()
		{
			base.Setup();

			_quikGraph = new(true, VertexCapacity, ArcCount);

			foreach ( (int from, List<int> neighbors) in _naiveGraph )
			{
				foreach ( int to in neighbors )
				{
					_quikGraph.AddVerticesAndEdge(new Edge<int>(from, to));
				}
			}
		}

		[Benchmark(OperationsPerInvoke = QueryCount)]
		public int QuikGraph_ShortestDistances ()
		{
			int reachableCount = 0;
			foreach ( int source in _sources )
			{
				TryFunc<int, IEnumerable<Edge<int>>> func = _quikGraph.ShortestPathsDijkstra(_ => 1.0, source);
				foreach ( int target in _existingVertices )
				{
					if ( func(target, out _) )
					{
						reachableCount++;
					}
				}
			}
			return reachableCount;
		}
	}

	public class Small_Graph_ShortestDistances : Graph_ShortestDistances_WithQuikGraph
	{
		[Params(1, 10, 100)]
		public override int VertexCapacity { get; set; }

		[Params(1, 10, 100)]
		public override int ArcCount { get; set; }
	}

	public class Medium_Graph_ShortestDistances : Graph_ShortestDistances_WithQuikGraph
	{
		[Params(1_000, 5_000, 10_000)]
		public override int VertexCapacity { get; set; }

		[Params(1_000, 5_000, 10_000)]
		public override int ArcCount { get; set; }
	}

	public class Large_Graph_ShortestDistances : Graph_ShortestDistances_WithNaive
	{
		[Params(50_000, 100_000, 500_000)]
		public override int VertexCapacity { get; set; }

		[Params(50_000, 100_000, 500_000)]
		public override int ArcCount { get; set; }
	}

	public class Extreme_Graph_ShortestDistances : Graph_ShortestDistances_Base
	{
		[Params(1_000_000, 10_000_000, 100_000_000)]
		public override int VertexCapacity { get; set; }

		[Params(1_000_000, 10_000_000, 100_000_000)]
		public override int ArcCount { get; set; }
	}
}
