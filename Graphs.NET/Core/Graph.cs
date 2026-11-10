using System.Buffers;
using System.Runtime.CompilerServices;
using Graphs.NET.Additional;
using Graphs.NET.Algorithms;

namespace Graphs.NET.Core
{
	public class Graph
	{
		internal bool _isAcyclicCalculated = false;

		private protected readonly OneBitArray _removedVertices;
		protected readonly int[] _offsets;
		protected readonly int[] _targets;
		protected readonly int _maxVertexIndexPlusOne;
		protected readonly GraphBuildingOptionsFixed _options;

		public static Graph Empty { get; } = new(0, 0, [0], [], OneBitArray.Empty, GraphBuildingOptions.Default);

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
		internal Graph (int vertexCount, int arcCount, int[] offsets, int[] targets, OneBitArray _removed, GraphBuildingOptionsFixed options, bool sortNeighbors = true)
		{
			VertexCount = vertexCount;
			ArcCount = arcCount;

			_offsets = offsets;
			_targets = targets;

			_removedVertices = _removed;

			_options = options;

			_maxVertexIndexPlusOne = _offsets.Length - 1;

			if ( arcCount > 0
				&& sortNeighbors )
			{
				int reallyMaxIndex = 0;
				for ( int vertex = 0; vertex < _maxVertexIndexPlusOne; vertex++ )
				{
					if ( _removedVertices[vertex] == 0 )
					{
						reallyMaxIndex = vertex;
						Span<int> neighbors = targets.AsSpan(offsets[vertex], offsets[vertex + 1] - offsets[vertex]);
						neighbors.Sort();
					}
				}
				_maxVertexIndexPlusOne = reallyMaxIndex + 1;
			}
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public ReadOnlySpan<int> GetNeighbors (int vertex)
		{
			ArgumentOutOfRangeException.ThrowIfNegative(vertex);
			ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(vertex, _maxVertexIndexPlusOne);

			return _removedVertices[vertex] == 1
				? throw new ArgumentException()
				: (ReadOnlySpan<int>) _targets.AsSpan(_offsets[vertex], _offsets[vertex + 1] - _offsets[vertex]);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool HasArc (int from, int to)
		{
			ArgumentOutOfRangeException.ThrowIfNegative(from);
			ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(from, _maxVertexIndexPlusOne);
			ArgumentOutOfRangeException.ThrowIfNegative(to);
			ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(to, _maxVertexIndexPlusOne);

			return _removedVertices[from] == 1 || _removedVertices[to] == 1
				? throw new ArgumentException()
				:_targets.AsSpan(_offsets[from], _offsets[from + 1] - _offsets[from]).BinarySearch(to) >= 0;
		}

		private float CalculateAverageVertexDegree ()
		{
			return VertexCount == 0 ? 0f : 2f * ArcCount / VertexCount;
		}

		private int CalculateDuplicateArcCount ()
		{
			int res = 0;
			for ( int vertex = 0; vertex < _maxVertexIndexPlusOne; vertex++ )
			{
				if ( _removedVertices[vertex] == 0 )
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
			}
			return res;
		}

		private int CalculateReflexiveArcCount ()
		{
			int res = 0;
			for ( int vertex = 0; vertex < _maxVertexIndexPlusOne; vertex++ )
			{
				if ( _removedVertices[vertex] == 0 )
				{
					ReadOnlySpan<int> neighbors = GetNeighbors(vertex);
					int firstOccuredIndex = neighbors.BinarySearch(vertex);
					
					if ( firstOccuredIndex >= 0 )
					{
						while ( firstOccuredIndex > 0
							&& neighbors[firstOccuredIndex - 1] == vertex )
						{
							firstOccuredIndex--;
						}
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
		internal Graph (int vertexCount, int arcCount, int[] offsets, int[] targets, OneBitArray _removed, GraphBuildingOptionsFixed options,
			TEdgeWeight[] weights)
			: base(vertexCount, arcCount, offsets, targets, _removed, options, false)
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
			internal static Graph<TWeight> Value = new(0, 0, [0], [], OneBitArray.Empty, GraphBuildingOptions.Default, []);
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
		internal Graph (int vertexCount, int arcCount, int[] offsets, int[] targets, OneBitArray _removed, GraphBuildingOptionsFixed options,
			TEdgeWeight[] weights, TVertexData[] data) 
			: base(vertexCount, arcCount, offsets, targets, _removed, options, weights)
		{
			_data = data;
		}

		private static class EmptyGraph<TWeight, TData>
			where TWeight : IComparable<TWeight>
		{
			internal static Graph<TWeight, TData> Value = new(0, 0, [0], [], OneBitArray.Empty, GraphBuildingOptions.Default, [], []);
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