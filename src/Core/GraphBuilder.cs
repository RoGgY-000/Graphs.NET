using System.Buffers;
using System.Numerics;
using System.Runtime.CompilerServices;
using Graphs.NET.Additional;

namespace Graphs.NET.Core
{
	public class GraphBuilder
	{
		private protected OneBitArray _activeVertices;
		protected int[] _tails;
		protected int[] _targets;
		protected int[] _nexts;
		protected int _newArcIndex;

		public int ArcCapacity { get; protected set; }
		public int ArcCount { get; protected set; }
		public int VertexCapacity { get; protected set; }
		public int VertexCount { get; protected set; }

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public GraphBuilder (int vertexCapacity = 0, int arcCapacity = 0)
		{
			ArgumentOutOfRangeException.ThrowIfNegative(vertexCapacity);
			ArgumentOutOfRangeException.ThrowIfNegative(arcCapacity);
			EnsureVertexCapacityOrResize(vertexCapacity);
			EnsureArcCapacityOrResize(arcCapacity);
		}

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

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void AddVertex (int vertex)
		{
			ArgumentOutOfRangeException.ThrowIfNegative(vertex);

			EnsureVertexCapacityOrResize(vertex + 1);

			CreateVertexIfNotExists(vertex);
		}

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
				maxVertex = Math.Max(maxVertex, vertices[vertexIndex]);
			}

			EnsureVertexCapacityOrResize(maxVertex + 1);

			for ( int vertexIndex = 0; vertexIndex < vertices.Length; vertexIndex++ )
			{
				int vertex = vertices[vertexIndex];

				CreateVertexIfNotExists(vertex);
			}
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public virtual Graph Build () => Build(GraphBuildingOptions.Default);

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

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool HasVertex (int vertex)
		{
			ArgumentOutOfRangeException.ThrowIfNegative(vertex);

			return vertex < VertexCapacity && _activeVertices[vertex] == 1;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
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
					_nexts[pointer] = -1;
					ArcCount--;
					return;
				}
				previousPointer = pointer;
				pointer = _nexts[pointer];
			}
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
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
				removedArcs++;
				pointer = _nexts[pointer];
			}
			_tails[vertex] = -1;

			for ( int currentVertex = 0; currentVertex < VertexCapacity; currentVertex++ )
			{
				if ( _activeVertices[currentVertex] == 1 )
				{
					pointer = _tails[currentVertex];

					while ( pointer != -1 && _targets[pointer] == vertex )
					{
						pointer = _nexts[pointer];
						removedArcs++;
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
							_nexts[previousPointer] = nextPointer;
							removedArcs++;
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
		protected void EnsureArcCapacityOrResize (int capacity)
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
		protected void EnsureVertexCapacityOrResize (int capacity)
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
		protected int InsertArc (int from, int to)
		{
			_targets[_newArcIndex] = to;
			_nexts[_newArcIndex] = _tails[from];
			_tails[from] = _newArcIndex;

			ArcCount++;

			return _newArcIndex++;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		protected void CreateVertexIfNotExists (int vertex)
		{
			if ( _activeVertices[vertex] == 0 )
			{
				_activeVertices[vertex] = 1;
				VertexCount++;
			}
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		protected virtual void ResizeArcs (int newCapacity)
		{
			Array.Resize(ref _targets, newCapacity);
			Array.Resize(ref _nexts, newCapacity);
			_nexts.AsSpan(ArcCount, newCapacity - ArcCount).Fill(-1);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		protected virtual void ResizeVertices (int oldCapacity, int newCapacity)
		{
			OneBitArray.Resize(ref _activeVertices, newCapacity);

			Array.Resize(ref _tails, newCapacity);
			_tails.AsSpan(oldCapacity, newCapacity - oldCapacity).Fill(-1);
		}
	}
}