using System.Runtime.CompilerServices;
using Graphs.NET.Additional;
using Graphs.NET.Algorithms;

namespace Graphs.NET.Core
{
	/// <summary>
	/// Represents an immutable, cache-friendly directed graph stored in Compressed Sparse Row (CSR) format.
	/// </summary>
	public class Graph
	{
		internal bool _isAcyclicCalculated = false;

		internal readonly OneBitArray _activeVertices;
		internal readonly int _maxVertexIndex;
		private protected readonly int[] _offsets;
		private protected readonly int[] _targets;
		private protected readonly GraphBuildingOptionsFixed _options;

		/// <summary>Gets a singleton empty <see cref="Graph"/> instance containing zero vertices and arcs.</summary>
		public static Graph Empty { get; } = new(0, -1, 0, [0], [], OneBitArray.Empty, GraphBuildingOptions.Default);

		/// <summary>Gets the total number of directed arcs in the graph.</summary>
		public int ArcCount { get; }

		/// <summary>Gets the total number of active vertices in the graph.</summary>
		public int VertexCount { get; }

		/// <summary>Gets a value indicating whether duplicate parallel arcs were allowed when building the graph.</summary>
		public bool IsAllowingDuplicateArcs => _options.enableDuplicateArcs;

		/// <summary>Gets a value indicating whether reflexive arcs (self-loops) were allowed when building the graph.</summary>
		public bool IsAllowingReflexiveArcs => _options.enableReflexiveArcs;

		/// <summary>
		/// Gets the average total degree (in-degree + out-degree) per vertex, computed lazily and cached.
		/// </summary>
		public float AverageVertexDegree
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get => field > -1 ? field : field = CalculateAverageVertexDegree();
		} = -1f;

		/// <summary>
		/// Gets the number of duplicate parallel arcs in the graph, computed lazily and cached.
		/// </summary>
		public int DuplicateArcCount
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get => field > -1 ? field : field = CalculateDuplicateArcCount();
		} = -1;

		/// <summary>
		/// Gets a value indicating whether the directed graph contains no cycles, computed lazily and cached.
		/// </summary>
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

		/// <summary>
		/// Gets the number of reflexive arcs (self-loops) in the graph, computed lazily and cached.
		/// </summary>
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

		/// <summary>
		/// Returns a read-only span of sorted target vertex indices adjacent to <paramref name="vertex"/>. Time complexity: O(1).
		/// </summary>
		/// <param name="vertex">The source vertex index.</param>
		/// <returns>A sorted <see cref="ReadOnlySpan{T}"/> of adjacent target vertex indices.</returns>
		/// <exception cref="ArgumentOutOfRangeException">
		/// Thrown when <paramref name="vertex"/> is negative, exceeds the maximum vertex index, or does not exist in the graph.
		/// </exception>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public ReadOnlySpan<int> GetNeighbors (int vertex)
		{
			ArgumentOutOfRangeException.ThrowIfNegative(vertex);
			ArgumentOutOfRangeException.ThrowIfGreaterThan(vertex, _maxVertexIndex);

			return _activeVertices[vertex] == 1
				? (ReadOnlySpan<int>) _targets.AsSpan(_offsets[vertex], _offsets[vertex + 1] - _offsets[vertex])
				: throw new ArgumentOutOfRangeException();
		}

		/// <summary>
		/// Determines whether a directed arc exists from <paramref name="from"/> to <paramref name="to"/>. Time complexity: O(log(deg(from))).
		/// </summary>
		/// <param name="from">The source vertex index.</param>
		/// <param name="to">The target vertex index.</param>
		/// <returns><see langword="true"/> if the arc exists; otherwise, <see langword="false"/>.</returns>
		/// <exception cref="ArgumentOutOfRangeException">
		/// Thrown when <paramref name="from"/> or <paramref name="to"/> is negative or exceeds the maximum vertex index.
		/// </exception>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool HasArc (int from, int to)
		{
			ArgumentOutOfRangeException.ThrowIfNegative(from);
			ArgumentOutOfRangeException.ThrowIfGreaterThan(from, _maxVertexIndex);
			ArgumentOutOfRangeException.ThrowIfNegative(to);
			ArgumentOutOfRangeException.ThrowIfGreaterThan(to, _maxVertexIndex);

			return _activeVertices[from] == 1 
				&& _activeVertices[to] == 1 
				&& _targets.AsSpan(_offsets[from], _offsets[from + 1] - _offsets[from]).BinarySearch(to) >= 0;
		}

		/// <summary>
		/// Determines whether a vertex with the specified index exists in the graph. Time complexity: O(1).
		/// </summary>
		/// <param name="vertex">The vertex index to check.</param>
		/// <returns><see langword="true"/> if the vertex is active; otherwise, <see langword="false"/>.</returns>
		/// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="vertex"/> is negative.</exception>
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
}