using System.Runtime.CompilerServices;
using Graphs.NET.Additional;
using Graphs.NET.Algorithms;

namespace Graphs.NET.Core
{
	public class Graph
	{
		internal bool _isAcyclicCalculated = false;

		internal readonly OneBitArray _activeVertices;
		protected readonly int[] _offsets;
		protected readonly int[] _targets;
		internal readonly int _maxVertexIndex;
		protected readonly GraphBuildingOptionsFixed _options;

		public static Graph Empty { get; } = new(0, -1, 0, [0], [], OneBitArray.Empty, GraphBuildingOptions.Default);

		public int ArcCount { get; }
		public int VertexCount { get; }

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
		public int ReflexiveArcCount
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get => field > -1 ? field : field = CalculateReflexiveArcCount();
		} = -1;

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		internal Graph (int vertexCount, int maxVertexIndex, int arcCount, int[] offsets, int[] targets, OneBitArray _active, GraphBuildingOptionsFixed options)
		{
			VertexCount = vertexCount;
			ArcCount = arcCount;

			_offsets = offsets;
			_targets = targets;

			_activeVertices = _active;

			_options = options;

			_maxVertexIndex = maxVertexIndex;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public ReadOnlySpan<int> GetNeighbors (int vertex)
		{
			ArgumentOutOfRangeException.ThrowIfNegative(vertex);
			ArgumentOutOfRangeException.ThrowIfGreaterThan(vertex, _maxVertexIndex);

			return _activeVertices[vertex] == 1
				? (ReadOnlySpan<int>) _targets.AsSpan(_offsets[vertex], _offsets[vertex + 1] - _offsets[vertex])
				: throw new ArgumentOutOfRangeException();
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool HasArc (int from, int to)
		{
			ArgumentOutOfRangeException.ThrowIfNegative(from);
			ArgumentOutOfRangeException.ThrowIfGreaterThan(from, _maxVertexIndex);
			ArgumentOutOfRangeException.ThrowIfNegative(to);
			ArgumentOutOfRangeException.ThrowIfGreaterThan(to, _maxVertexIndex);

			return _activeVertices[from] == 1 && _activeVertices[to] == 1
				? _targets.AsSpan(_offsets[from], _offsets[from + 1] - _offsets[from]).BinarySearch(to) >= 0
				: throw new ArgumentOutOfRangeException();
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool HasVertex (int vertex)
		{
			ArgumentOutOfRangeException.ThrowIfNegative(vertex);

			return vertex <= _maxVertexIndex && _activeVertices[vertex] == 1;
		}

		private float CalculateAverageVertexDegree ()
		{
			return VertexCount == 0 ? 0f : 2f * ArcCount / VertexCount;
		}

		private int CalculateDuplicateArcCount ()
		{
			if ( !_options.enableDuplicateArcs || ArcCount < 2 )
			{
				return 0;
			}

			int res = 0;
			for ( int vertex = 0; vertex <= _maxVertexIndex; vertex++ )
			{
				if ( _activeVertices[vertex] == 1 )
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
			if ( !_options.enableReflexiveArcs || ArcCount == 0 )
			{
				return 0;
			}

			int res = 0;
			for ( int vertex = 0; vertex <= _maxVertexIndex; vertex++ )
			{
				if ( _activeVertices[vertex] == 1 )
				{
					ReadOnlySpan<int> neighbors = GetNeighbors(vertex);
					int firstReflexiveOccuredIndex = neighbors.BinarySearch(vertex);
					
					if ( firstReflexiveOccuredIndex >= 0 )
					{
						while ( firstReflexiveOccuredIndex > 0
							&& neighbors[firstReflexiveOccuredIndex - 1] == vertex )
						{
							firstReflexiveOccuredIndex--;
						}
						ReadOnlySpan<int> startsWithReflexive = neighbors.Slice(firstReflexiveOccuredIndex);
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

		public static new Graph<TEdgeWeight> Empty { get; } = new(0, -1, 0, [0], [], OneBitArray.Empty, GraphBuildingOptions.Default, []);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		internal Graph (int vertexCount, int maxVertexIndex, int arcCount, int[] offsets, int[] targets, OneBitArray _active, GraphBuildingOptionsFixed options, TEdgeWeight[] weights)
			: base(vertexCount, maxVertexIndex, arcCount, offsets, targets, _active, options)
		{
			_weights = weights;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public ReadOnlySpan<TEdgeWeight> GetWeights (int vertex)
		{
			ArgumentOutOfRangeException.ThrowIfNegative(vertex);
			ArgumentOutOfRangeException.ThrowIfGreaterThan(vertex, _maxVertexIndex);

			return _activeVertices[vertex] == 1
				? _weights.AsSpan(_offsets[vertex], _offsets[vertex + 1] - _offsets[vertex])
				: throw new ArgumentOutOfRangeException();
		}
	}

	public class Graph<TEdgeWeight, TVertexData>
		: Graph<TEdgeWeight>
		where TEdgeWeight : IComparable<TEdgeWeight>
	{
		private readonly TVertexData[] _data;

		public static new Graph<TEdgeWeight, TVertexData> Empty { get; } = new(0, -1, 0, [0], [], OneBitArray.Empty, GraphBuildingOptions.Default, [], []);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		internal Graph (int vertexCount,int maxVertexIndex, int arcCount, int[] offsets, int[] targets, OneBitArray _active, GraphBuildingOptionsFixed options, TEdgeWeight[] weights, TVertexData[] data) 
			: base(vertexCount, maxVertexIndex, arcCount, offsets, targets, _active, options, weights)
		{
			_data = data;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public TVertexData GetVertexData (int vertex)
		{
			ArgumentOutOfRangeException.ThrowIfNegative(vertex);
			ArgumentOutOfRangeException.ThrowIfGreaterThan(vertex, _maxVertexIndex);

			return _activeVertices[vertex] == 1
				? _data[vertex]
				: throw new ArgumentOutOfRangeException();
		}
	}
}