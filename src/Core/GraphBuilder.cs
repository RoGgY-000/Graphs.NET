using System.Buffers;
using System.Numerics;
using System.Runtime.CompilerServices;
using Graphs.NET.Additional;

namespace Graphs.NET.Core
{
	/// <summary>
	/// Provides a mutable, array-backed builder for constructing immutable Compressed Sparse Row (CSR) <see cref="Graph"/> instances.
	/// </summary>
	public class GraphBuilder
	{
		private protected OneBitArray _activeVertices = OneBitArray.Empty;
		private protected int[] _tails = [];
		private protected int[] _targets = [];
		private protected int[] _nexts = [];
		private protected int _freeListTail = -1;
		private protected int _newArcIndex;

		/// <summary>Gets the current allocated capacity for arcs.</summary>
		public int ArcCapacity { get; private protected set; }

		/// <summary>Gets the current number of active arcs in the builder.</summary>
		public int ArcCount { get; private protected set; }

		/// <summary>Gets the current allocated capacity for vertices.</summary>
		public int VertexCapacity { get; private protected set; }

		/// <summary>Gets the current number of active vertices in the builder.</summary>
		public int VertexCount { get; private protected set; }

		/// <summary>
		/// Initializes a new instance of <see cref="GraphBuilder"/> with optional initial vertex and arc capacities.
		/// </summary>
		/// <param name="vertexCapacity">The initial capacity for vertices. Rounded up to a power of two if positive.</param>
		/// <param name="arcCapacity">The initial capacity for arcs. Rounded up to a power of two if positive.</param>
		/// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="vertexCapacity"/> or <paramref name="arcCapacity"/> is negative.</exception>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public GraphBuilder (int vertexCapacity = 0, int arcCapacity = 0)
		{
			ArgumentOutOfRangeException.ThrowIfNegative(vertexCapacity);
			ArgumentOutOfRangeException.ThrowIfNegative(arcCapacity);
			EnsureVertexCapacityOrResize(vertexCapacity);
			EnsureArcCapacityOrResize(arcCapacity);
		}

		/// <summary>
		/// Adds a directed arc between two existing vertices.
		/// </summary>
		/// <param name="from">The source vertex index.</param>
		/// <param name="to">The target vertex index.</param>
		/// <exception cref="ArgumentOutOfRangeException">Thrown when either vertex index is negative or does not exist in the builder.</exception>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void AddArc (int from, int to)
		{
			ArgumentOutOfRangeException.ThrowIfNegative(from);
			ArgumentOutOfRangeException.ThrowIfNegative(to);

			ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(from, VertexCapacity);
			ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(to, VertexCapacity);

			if ( _activeVertices[from] == 0 || _activeVertices[to] == 0 )
			{
				throw new ArgumentOutOfRangeException();
			}

			EnsureArcCapacityOrResize(ArcCount + 1);

			InsertArc(from, to);
		}

		/// <summary>
		/// Adds a directed arc between two vertices, automatically creating either vertex if it does not yet exist.
		/// </summary>
		/// <param name="from">The source vertex index.</param>
		/// <param name="to">The target vertex index.</param>
		/// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="from"/> or <paramref name="to"/> is negative.</exception>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void AddArcAndVertices (int from, int to)
		{
			ArgumentOutOfRangeException.ThrowIfNegative(from);
			ArgumentOutOfRangeException.ThrowIfNegative(to);

			EnsureVertexCapacityOrResize(Math.Max(from, to) + 1);
			EnsureArcCapacityOrResize(ArcCount + 1);

			CreateVertexIfNotExists(from);
			CreateVertexIfNotExists(to);

			InsertArc(from, to);
		}

		/// <summary>
		/// Adds an undirected edge (two opposite directed arcs) between two existing vertices.
		/// </summary>
		/// <param name="v1">The first vertex index.</param>
		/// <param name="v2">The second vertex index.</param>
		/// <exception cref="ArgumentOutOfRangeException">Thrown when either vertex index is negative or does not exist in the builder.</exception>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void AddEdge (int v1, int v2)
		{
			ArgumentOutOfRangeException.ThrowIfNegative(v1);
			ArgumentOutOfRangeException.ThrowIfNegative(v2);

			ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(v1, VertexCapacity);
			ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(v2, VertexCapacity);

			if ( _activeVertices[v1] == 0 || _activeVertices[v2] == 0 )
			{
				throw new ArgumentOutOfRangeException();
			}

			EnsureArcCapacityOrResize(ArcCount + 2);

			InsertArc(v1, v2);
			InsertArc(v2, v1);
		}

		/// <summary>
		/// Adds an undirected edge (two opposite directed arcs) between two vertices, automatically creating either vertex if it does not yet exist.
		/// </summary>
		/// <param name="v1">The first vertex index.</param>
		/// <param name="v2">The second vertex index.</param>
		/// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="v1"/> or <paramref name="v2"/> is negative.</exception>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void AddEdgeAndVertices (int v1, int v2)
		{
			ArgumentOutOfRangeException.ThrowIfNegative(v1);
			ArgumentOutOfRangeException.ThrowIfNegative(v2);

			EnsureVertexCapacityOrResize(Math.Max(v1, v2) + 1);
			EnsureArcCapacityOrResize(ArcCount + 2);

			CreateVertexIfNotExists(v1);
			CreateVertexIfNotExists(v2);

			InsertArc(v1, v2);
			InsertArc(v2, v1);
		}

		/// <summary>
		/// Registers a vertex with the specified index if it does not already exist.
		/// </summary>
		/// <param name="vertex">The zero-based vertex index.</param>
		/// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="vertex"/> is negative.</exception>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void AddVertex (int vertex)
		{
			ArgumentOutOfRangeException.ThrowIfNegative(vertex);

			EnsureVertexCapacityOrResize(vertex + 1);

			CreateVertexIfNotExists(vertex);
		}

		/// <summary>
		/// Registers multiple vertices from the specified span.
		/// </summary>
		/// <param name="vertices">A span of zero-based vertex indices to add.</param>
		/// <exception cref="ArgumentOutOfRangeException">Thrown when any vertex index in <paramref name="vertices"/> is negative.</exception>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void AddVertices(ReadOnlySpan<int> vertices)
		{
			if ( vertices.IsEmpty )
			{
				return;
			}

			int maxVertex = -1;
			for ( int vertexIndex = 0; vertexIndex < vertices.Length; vertexIndex++ )
			{
				ArgumentOutOfRangeException.ThrowIfNegative(vertices[vertexIndex]);
				maxVertex = Math.Max(maxVertex, vertices[vertexIndex]);
			}

			EnsureVertexCapacityOrResize(maxVertex + 1);

			for ( int vertexIndex = 0; vertexIndex < vertices.Length; vertexIndex++ )
			{
				int vertex = vertices[vertexIndex];

				CreateVertexIfNotExists(vertex);
			}
		}

		/// <summary>
		/// Builds an immutable <see cref="Graph"/> using <see cref="GraphBuildingOptions.Default"/>.
		/// </summary>
		/// <returns>A compact CSR <see cref="Graph"/> instance.</returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public virtual Graph Build () => Build(GraphBuildingOptions.Default);

		/// <summary>
		/// Builds an immutable <see cref="Graph"/> using the specified <paramref name="options"/>.
		/// </summary>
		/// <param name="options">The immutable options controlling reflexive and duplicate arc filtering.</param>
		/// <returns>A compact CSR <see cref="Graph"/> instance.</returns>
		public virtual Graph Build (GraphBuildingOptionsFixed options)
		{
			if ( VertexCount == 0 )
			{
				return Graph.Empty;
			}

			int totalEdgeCount = 0;
			ArrayPool<int> pool = ArrayPool<int>.Shared;
			int[] temporaryOffsets = pool.Rent(VertexCapacity + 1);
			int[] temporaryTargets = pool.Rent(ArcCount);

			try
			{
				temporaryOffsets[0] = 0;
				int maxVertexIndex = 0;
				for ( int vertex = 0; vertex < VertexCapacity; vertex++ )
				{
					if ( _activeVertices[vertex] == 1 )
					{
						maxVertexIndex = vertex;

						int pointer = _tails[vertex];
						int currentVertexOffset = 0;
						while ( pointer != -1 )
						{
							int target = _targets[pointer];
							bool addArc = true;
							if ( target == vertex
								&& !options.enableReflexiveArcs )
							{
								addArc = false;
							}
							if ( !options.enableDuplicateArcs
								&& temporaryTargets.AsSpan(temporaryOffsets[vertex], currentVertexOffset).Contains(target) ) // maybe instead use a OneBitArray and mark existing arcs, make a benchmark and decide
							{
								addArc = false;
							}

							if ( addArc )
							{
								temporaryTargets[temporaryOffsets[vertex] + currentVertexOffset++] = target;
							}
							pointer = _nexts[pointer];
						}
						temporaryOffsets[vertex + 1] = temporaryOffsets[vertex] + currentVertexOffset;
						Span<int> neighbors = temporaryTargets.AsSpan(temporaryOffsets[vertex], currentVertexOffset);
						neighbors.Sort();
						totalEdgeCount += currentVertexOffset;
					}
					else
					{
						temporaryOffsets[vertex + 1] = temporaryOffsets[vertex];
					}
				}
				int[] offsets = new int[maxVertexIndex + 2];
				temporaryOffsets.AsSpan(0, maxVertexIndex + 2).CopyTo(offsets.AsSpan());
				int[] targets = new int[totalEdgeCount];
				temporaryTargets.AsSpan(0, totalEdgeCount).CopyTo(targets.AsSpan());
				OneBitArray active = new(_activeVertices);
				Graph g = new(VertexCount, maxVertexIndex, totalEdgeCount, offsets, targets, active, options);
				return g;
			}
			finally
			{
				pool.Return(temporaryOffsets);
				pool.Return(temporaryTargets);
			}
		}

		/// <summary>
		/// Determines whether a directed arc exists from <paramref name="from"/> to <paramref name="to"/>.
		/// </summary>
		/// <param name="from">The source vertex index.</param>
		/// <param name="to">The target vertex index.</param>
		/// <returns><see langword="true"/> if the arc exists; otherwise, <see langword="false"/>.</returns>
		/// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="from"/> or <paramref name="to"/> is negative.</exception>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool HasArc (int from, int to)
		{
			ArgumentOutOfRangeException.ThrowIfNegative(from);
			ArgumentOutOfRangeException.ThrowIfNegative(to);

			if ( from >= VertexCapacity || to >= VertexCapacity
				|| _activeVertices[from] == 0 || _activeVertices[to] == 0 )
			{
				return false;
			}

			int pointer = _tails[from];
			while ( pointer != -1 )
			{
				if ( _targets[pointer] == to )
				{
					return true;
				}
				pointer = _nexts[pointer];
			}
			return false;

		}

		/// <summary>
		/// Determines whether a vertex with the specified index exists in the builder.
		/// </summary>
		/// <param name="vertex">The vertex index to check.</param>
		/// <returns><see langword="true"/> if the vertex is active; otherwise, <see langword="false"/>.</returns>
		/// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="vertex"/> is negative.</exception>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool HasVertex (int vertex)
		{
			ArgumentOutOfRangeException.ThrowIfNegative(vertex);

			return vertex < VertexCapacity && _activeVertices[vertex] == 1;
		}

		/// <summary>
		/// Removes a single directed arc from <paramref name="from"/> to <paramref name="to"/> if it exists.
		/// </summary>
		/// <param name="from">The source vertex index.</param>
		/// <param name="to">The target vertex index.</param>
		/// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="from"/> or <paramref name="to"/> is negative.</exception>
		public virtual void RemoveArc (int from, int to)
		{
			ArgumentOutOfRangeException.ThrowIfNegative(from);
			ArgumentOutOfRangeException.ThrowIfNegative(to);

			if ( from >= VertexCapacity || to >= VertexCapacity
				|| _activeVertices[from] == 0 || _activeVertices[to] == 0 )
			{
				return;
			}

			int pointer = _tails[from];

			if ( pointer == -1 )
			{
				return;
			}

			if ( _targets[pointer] == to )
			{
				_tails[from] = _nexts[pointer];
				UpdateFreeList(pointer);
				ArcCount--;
				return;
			}

			int previousPointer = pointer;
			pointer = _nexts[pointer];

			while ( pointer != -1 )
			{
				if ( _targets[pointer] == to )
				{
					_nexts[previousPointer] = _nexts[pointer];
					UpdateFreeList(pointer);
					ArcCount--;
					return;
				}
				previousPointer = pointer;
				pointer = _nexts[pointer];
			}
		}

		/// <summary>
		/// Removes the specified <paramref name="vertex"/> and all incident incoming and outgoing arcs.
		/// </summary>
		/// <param name="vertex">The vertex index to remove.</param>
		/// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="vertex"/> is negative.</exception>
		public virtual void RemoveVertex (int vertex)
		{
			ArgumentOutOfRangeException.ThrowIfNegative(vertex);

			if ( vertex >= VertexCapacity || _activeVertices[vertex] == 0 )
			{
				return;
			}

			_activeVertices[vertex] = 0;
			VertexCount--;

			int removedArcs = 0;

			int pointer = _tails[vertex];
			while ( pointer != -1 )
			{
				int next = _nexts[pointer];
				UpdateFreeList(pointer);
				removedArcs++;
				pointer = next;
			}
			_tails[vertex] = -1;

			for ( int currentVertex = 0; currentVertex < VertexCapacity; currentVertex++ )
			{
				if ( _activeVertices[currentVertex] == 1 )
				{
					pointer = _tails[currentVertex];

					while ( pointer != -1 && _targets[pointer] == vertex )
					{
						int next = _nexts[pointer];
						UpdateFreeList(pointer);
						removedArcs++;
						pointer = next;
					}

					_tails[currentVertex] = pointer;

					if ( pointer == -1 )
					{
						continue;
					}

					int previousPointer = pointer;
					pointer = _nexts[pointer];
					while ( pointer != -1 )
					{
						int nextPointer = _nexts[pointer];

						if ( _targets[pointer] == vertex )
						{
							UpdateFreeList(pointer);
							removedArcs++;
							_nexts[previousPointer] = nextPointer;
						}
						else
						{
							previousPointer = pointer;
						}
						pointer = nextPointer;
					}
				}
			}
			ArcCount -= removedArcs;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private protected void EnsureArcCapacityOrResize (int capacity)
		{
			if ( capacity <= ArcCapacity )
			{
				return;
			}
			else
			{
				int newCapacity = (int) BitOperations.RoundUpToPowerOf2((uint) capacity);
				ResizeArcs(newCapacity);
				ArcCapacity = newCapacity;
			}
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private protected void EnsureVertexCapacityOrResize (int capacity)
		{
			if ( capacity <= VertexCapacity )
			{
				return;
			}
			else
			{
				int newCapacity = (int) BitOperations.RoundUpToPowerOf2((uint) capacity);
				ResizeVertices(VertexCapacity, newCapacity);
				VertexCapacity = newCapacity;
			}
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private protected int InsertArc (int from, int to)
		{
			int arcIndex;
			if ( _freeListTail != -1 )
			{
				arcIndex = _freeListTail;
				_freeListTail = _nexts[_freeListTail];
			}
			else
			{
				arcIndex = _newArcIndex++;
			}

			_targets[arcIndex] = to;
			_nexts[arcIndex] = _tails[from];
			_tails[from] = arcIndex;

			ArcCount++;

			return arcIndex;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private protected void CreateVertexIfNotExists (int vertex)
		{
			if ( _activeVertices[vertex] == 0 )
			{
				_activeVertices[vertex] = 1;
				VertexCount++;
			}
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private protected virtual void ResizeArcs (int newCapacity)
		{
			Array.Resize(ref _targets, newCapacity);
			Array.Resize(ref _nexts, newCapacity);
			_nexts.AsSpan(_newArcIndex, newCapacity - _newArcIndex).Fill(-1);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private protected virtual void ResizeVertices (int oldCapacity, int newCapacity)
		{
			OneBitArray.Resize(ref _activeVertices, newCapacity);

			Array.Resize(ref _tails, newCapacity);
			_tails.AsSpan(oldCapacity, newCapacity - oldCapacity).Fill(-1);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private protected void UpdateFreeList (int newTail)
		{
			_nexts[newTail] = _freeListTail;
			_freeListTail = newTail;
		}
	}
}