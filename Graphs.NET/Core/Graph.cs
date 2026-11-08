using System.Buffers;
using System.Runtime.CompilerServices;
using Graphs.NET.Algorithms;

namespace Graphs.NET.Core
{
	public class Graph
	{
		internal bool _isAcyclicCalculated = false;

		protected readonly int[] _offsets;
		protected readonly int[] _targets;
		protected readonly GraphBuildingOptionsFixed _options;

		public static Graph Empty { get; } = new([0], [], GraphBuildingOptions.Default);

		public int VertexCount { get; }
		public int ArcCount { get; }

		public bool IsAllowingDuplicateArcs => _options.enableDuplicateArcs;
		public bool IsAllowingReflexiveArcs => _options.enableReflexiveArcs;

		public float AverageVertexDegree
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get => field > -1 ? field : field = CalculateAverageVertexDegree();
		} = -1f;
		public int DuplicateArcCount
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get => field > -1 ? field : field = CalculateDuplicateArcCount();
		} = -1;
		public int ReflexiveArcCount
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get => field > -1 ? field : field = CalculateReflexiveArcCount();
		} = -1;

		public bool IsAcyclic
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get
			{
				if ( _isAcyclicCalculated )
				{
					return field;
				}
				field = GraphAlgorithmsExtensions.CalculateIsAcyclic(this);
				_isAcyclicCalculated = true;
				return field;
			}
			internal set;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		internal Graph (int[] offsets, int[] targets, GraphBuildingOptionsFixed options, bool sortNeighbors = true)
		{
			_offsets = offsets;
			_targets = targets;

			VertexCount = offsets.Length - 1;
			ArcCount = targets.Length;

			_options = options;

			if ( sortNeighbors )
			{
				for ( int vertex = 0; vertex < VertexCount; vertex++ )
				{
					Span<int> neighbors = targets.AsSpan(offsets[vertex], offsets[vertex + 1] - offsets[vertex]);
					neighbors.Sort();
				}
			}
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public ReadOnlySpan<int> GetNeighbors (int vertex)
		{
			ArgumentOutOfRangeException.ThrowIfNegative(vertex);
			ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(vertex, VertexCount);

			return _targets.AsSpan(_offsets[vertex], _offsets[vertex + 1] - _offsets[vertex]);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool HasArc (int from, int to)
		{
			ArgumentOutOfRangeException.ThrowIfNegative(from);
			ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(from, VertexCount);
			ArgumentOutOfRangeException.ThrowIfNegative(to);
			ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(to, VertexCount);

			return _targets.AsSpan(_offsets[from], _offsets[from + 1] - _offsets[from]).BinarySearch(to) >= 0;
		}

		private float CalculateAverageVertexDegree ()
		{
			return VertexCount == 0 ? 0f : 2f * ArcCount / VertexCount;
		}

		private int CalculateDuplicateArcCount ()
		{
			int res = 0;
			for ( int vertex = 0; vertex < VertexCount; vertex++ )
			{
				int currentNeighbor = -1;
				ReadOnlySpan<int> neighbors = GetNeighbors(vertex);
				foreach ( int neighbor in neighbors )
				{
					if ( currentNeighbor != neighbor )
					{
						currentNeighbor = neighbor;
					}
					else
					{
						res++;
					}
				}
			}
			return res;
		}

		private int CalculateReflexiveArcCount ()
		{
			int res = 0;
			for ( int vertex = 0; vertex < VertexCount; vertex++ )
			{
				ReadOnlySpan<int> neighbors = GetNeighbors(vertex);
				int firstOccuredIndex = neighbors.BinarySearch(vertex);
				if ( firstOccuredIndex >= 0 )
				{
					ReadOnlySpan<int> startsWithReflexive = neighbors.Slice(firstOccuredIndex);
					int reflexiveCount = 0;
					while ( reflexiveCount < startsWithReflexive.Length
						&& startsWithReflexive[reflexiveCount] == vertex )
					{
						reflexiveCount++;
					}
					res += reflexiveCount;
				}
			}
			return res;
		}
	}

	public class Graph<TEdgeWeight>
		: Graph
		where TEdgeWeight : IComparable<TEdgeWeight>
	{
		protected readonly TEdgeWeight[] _weights;

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		internal Graph (int[] offsets, int[] targets, GraphBuildingOptionsFixed options,
			TEdgeWeight[] weights)
			: base(offsets, targets, options, false)
		{
			_weights = weights;

			for ( int vertex = 0; vertex < VertexCount; vertex++ )
			{
				Span<int> neighbors = targets.AsSpan(offsets[vertex], offsets[vertex + 1] - offsets[vertex]);
				Span<TEdgeWeight> vertexWeights = weights.AsSpan(offsets[vertex], offsets[vertex + 1] - offsets[vertex]);
				neighbors.Sort(vertexWeights);
			}
		}

		private static class EmptyGraph<TWeight>
			where TWeight : IComparable<TWeight>
		{
			internal static Graph<TWeight> Value = new([0], [], GraphBuildingOptions.Default, []);
		}

		public static new Graph<TEdgeWeight> Empty => EmptyGraph<TEdgeWeight>.Value;

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public ReadOnlySpan<TEdgeWeight> GetWeights (int vertex)
		{
			ArgumentOutOfRangeException.ThrowIfNegative(vertex);
			ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(vertex, VertexCount);

			return _weights.AsSpan(_offsets[vertex], _offsets[vertex + 1] - _offsets[vertex]);
		}
	}

	public class Graph<TEdgeWeight, TVertexData>
		: Graph<TEdgeWeight>
		where TEdgeWeight : IComparable<TEdgeWeight>
	{
		private readonly TVertexData[] _data;

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		internal Graph (int[] offsets, int[] targets, GraphBuildingOptionsFixed options,
			TEdgeWeight[] weights, TVertexData[] data) 
			: base(offsets, targets, options, weights)
		{
			_data = data;
		}

		private static class EmptyGraph<TWeight, TData>
			where TWeight : IComparable<TWeight>
		{
			internal static Graph<TWeight, TData> Value = new([0], [], GraphBuildingOptions.Default, [], []);
		}

		public static new Graph<TEdgeWeight, TVertexData> Empty => EmptyGraph<TEdgeWeight, TVertexData>.Value;

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public TVertexData GetVertexData (int vertex)
		{
			ArgumentOutOfRangeException.ThrowIfNegative(vertex);
			ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(vertex, VertexCount);

			return _data[vertex];
		}
	}
}