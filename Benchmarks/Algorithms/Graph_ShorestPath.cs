using System;
using System.Collections.Generic;
using BenchmarkDotNet.Attributes;
using Benchmarks.Extensions;
using Graphs.NET.Algorithms;
using Graphs.NET.Core;
using QuikGraph;
using QuikGraph.Algorithms;

namespace Benchmarks.Algorithms
{
	public abstract class Graph_ShortestPath_Base
	{
		protected const int QueryCount = 100;

		public virtual int VertexCapacity { get; set; }
		public virtual int ArcCount { get; set; }

		protected int[] _path;
		protected (int Start, int End)[] _queries;
		protected Graph _graph;

		[GlobalSetup]
		public virtual void Setup ()
		{
			Random random = new(42);

			_path = new int[VertexCapacity];
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

			int[] existingVertices = [.. addedVertices];
			_queries = new (int, int)[QueryCount];

			if ( existingVertices.Length > 0 )
			{
				for ( int i = 0; i < QueryCount; i++ )
				{
					int start = existingVertices[random.Next(existingVertices.Length)];
					int end = existingVertices[random.Next(existingVertices.Length)];
					_queries[i] = (start, end);
				}
			}
		}

		[Benchmark(OperationsPerInvoke = QueryCount, Baseline = true)]
		public int GraphsNET_ShortestPath ()
		{
			int totalLength = 0;
			foreach ( (int start, int end) in _queries )
			{
				if ( _graph.TryFindShortestPath(start, end, _path.AsSpan(), out int foundPath) )
				{
					totalLength += foundPath;
				}
			}
			return totalLength;
		}
	}

	public abstract class Graph_ShortestPath_WithNaive : Graph_ShortestPath_Base
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
		public int Naive_ShortestPath ()
		{
			int totalLength = 0;
			foreach ( (int start, int end) in _queries )
			{
				if ( NaiveGraphExtensions.TryRunNaiveShortestPath(_naiveGraph, start, end, _path.AsSpan(), out int foundPath) )
				{
					totalLength += foundPath;
				}
			}
			return totalLength;
		}
	}

	public abstract class Graph_ShortestPath_WithQuikGraph : Graph_ShortestPath_WithNaive
	{
		protected AdjacencyGraph<int, Edge<int>> _quikGraph;

		public override void Setup ()
		{
			base.Setup();

			_quikGraph = new(true, VertexCapacity, ArcCount);

			foreach ( (int from, List<int> neighbors) in _naiveGraph )
			{
				_quikGraph.AddVertex(from);
				foreach ( int to in neighbors )
				{
					_quikGraph.AddVerticesAndEdge(new Edge<int>(from, to));
				}
			}
		}

		[Benchmark(OperationsPerInvoke = QueryCount)]
		public int QuikGraph_ShortestPath ()
		{
			int totalLength = 0;
			foreach ( (int start, int end) in _queries )
			{
				TryFunc<int, IEnumerable<Edge<int>>> func = _quikGraph.ShortestPathsDijkstra(_ => 1.0, start);
				if ( func(end, out IEnumerable<Edge<int>> path) )
				{
					foreach ( Edge<int> _ in path )
					{
						totalLength++;
					}
				}
			}
			return totalLength;
		}
	}

	public class Small_Graph_ShortestPath : Graph_ShortestPath_WithQuikGraph
	{
		[Params(1, 10, 100)]
		public override int VertexCapacity { get; set; }

		[Params(1, 10, 100)]
		public override int ArcCount { get; set; }
	}

	public class Medium_Graph_ShortestPath : Graph_ShortestPath_WithQuikGraph
	{
		[Params(1_000, 5_000, 10_000)]
		public override int VertexCapacity { get; set; }

		[Params(1_000, 5_000, 10_000)]
		public override int ArcCount { get; set; }
	}

	public class Large_Graph_ShortestPath : Graph_ShortestPath_WithNaive
	{
		[Params(50_000, 100_000, 500_000)]
		public override int VertexCapacity { get; set; }

		[Params(50_000, 100_000, 500_000)]
		public override int ArcCount { get; set; }
	}

	public class Extreme_Graph_ShortestPath : Graph_ShortestPath_Base
	{
		[Params(1_000_000, 10_000_000, 100_000_000)]
		public override int VertexCapacity { get; set; }

		[Params(1_000_000, 10_000_000, 100_000_000)]
		public override int ArcCount { get; set; }
	}
}
