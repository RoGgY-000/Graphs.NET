using System.Buffers;
using System.Numerics;
using System.Runtime.CompilerServices;
using Graphs.NET.Additional;

namespace Graphs.NET.Core
{
	public class GraphBuilder
	{
		private protected OneBitArray _removedVertices;
		protected int[] _tails;
		protected int[] _targets;
		protected int[] _nexts;

		public int EdgeCapacity { get; protected set; }
		public int EdgeCount { get; protected set; }
		public int VertexCapacity { get; protected set; }
		public int VertexCount { get; protected set; }

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public GraphBuilder (int vertexCapacity = 0, int edgeCapacity = 0)
		{
			ArgumentOutOfRangeException.ThrowIfNegative(vertexCapacity);
			ArgumentOutOfRangeException.ThrowIfNegative(edgeCapacity);
			EnsureVertexCapacityOrResize(vertexCapacity);
			EnsureEdgeCapacityOrResize(edgeCapacity);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void AddArc (int from, int to)
		{
			ArgumentOutOfRangeException.ThrowIfNegative(from);
			ArgumentOutOfRangeException.ThrowIfNegative(to);

			ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(from, VertexCapacity);
			ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(to, VertexCapacity);

			if ( _removedVertices[from] == 1 || _removedVertices[to] == 1 )
			{
				throw new ArgumentOutOfRangeException();
			}

			EnsureEdgeCapacityOrResize(EdgeCount + 1);

			InsertArc(from, to);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void AddArcAndVertices (int from, int to)
		{
			ArgumentOutOfRangeException.ThrowIfNegative(from);
			ArgumentOutOfRangeException.ThrowIfNegative(to);

			EnsureVertexCapacityOrResize(Math.Max(from, to) + 1);
			EnsureEdgeCapacityOrResize(EdgeCount + 1);

			if ( _removedVertices[from] == 1 )
			{
				_removedVertices[from] = 0;
				VertexCount++;
			}
			if ( _removedVertices[to] == 1 )
			{
				_removedVertices[to] = 0;
				VertexCount++;
			}

			InsertArc(from, to);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void AddEdge (int v1, int v2)
		{
			ArgumentOutOfRangeException.ThrowIfNegative(v1);
			ArgumentOutOfRangeException.ThrowIfNegative(v2);

			ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(v1, VertexCapacity);
			ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(v2, VertexCapacity);

			if ( _removedVertices[v1] == 1 || _removedVertices[v2] == 1 )
			{
				throw new ArgumentOutOfRangeException();
			}

			EnsureEdgeCapacityOrResize(EdgeCount + 2);

			if ( v1 != v2 )
			{
				InsertArc(v2, v1);
			}

			InsertArc(v1, v2);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void AddEdgeAndVertices (int v1, int v2)
		{
			ArgumentOutOfRangeException.ThrowIfNegative(v1);
			ArgumentOutOfRangeException.ThrowIfNegative(v2);

			EnsureVertexCapacityOrResize(Math.Max(v1, v2) + 1);
			EnsureEdgeCapacityOrResize(EdgeCount + 2);

			if ( _removedVertices[v1] == 1 )
			{
				_removedVertices[v1] = 0;
				VertexCount++;
			}
			if ( _removedVertices[v2] == 1 )
			{
				_removedVertices[v2] = 0;
				VertexCount++;
			}

			if ( v1 != v2 )
			{
				InsertArc(v2, v1);
			}

			InsertArc(v1, v2);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void AddVertex (int vertex)
		{
			ArgumentOutOfRangeException.ThrowIfNegative(vertex);

			EnsureVertexCapacityOrResize(vertex + 1);

			if ( _removedVertices[vertex] == 1 )
			{
				_removedVertices[vertex] = 0;
				VertexCount++;
			}
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void AddVertices(ReadOnlySpan<int> vertices)
		{
			for ( int i = 0; i < vertices.Length; i++ )
			{
				AddVertex(vertices[i]);
			}
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public virtual Graph Build () => Build(GraphBuildingOptions.Default);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public virtual Graph Build (GraphBuildingOptionsFixed options)
		{
			if ( VertexCount == 0 )
			{
				return Graph.Empty;
			}

			int[] offsets = new int[VertexCount + 1];

			int totalEdgeCount = 0;
			ArrayPool<int> pool = ArrayPool<int>.Shared;
			int[] temporaryTargets = pool.Rent(EdgeCount);

			try
			{
				for ( int vertex = 0; vertex < VertexCapacity; vertex++ )
				{
					if ( _removedVertices[vertex] == 0 )
					{
						int pointer = _tails[vertex];
						int currentVertexOffset = 0;
						while ( pointer != -1 )
						{
							int target = _targets[pointer];
							bool add = true;
							if ( target == vertex )
							{
								if ( !options.enableReflexiveArcs )
								{
									add = false;
								}
							}
							if ( !options.enableDuplicateArcs
							&& temporaryTargets.AsSpan(offsets[vertex], currentVertexOffset).Contains(target) )
							{
								add = false;
							}

							if ( add )
							{
								temporaryTargets[offsets[vertex] + currentVertexOffset++] = target;
							}
							pointer = _nexts[pointer];
						}
						offsets[vertex + 1] = offsets[vertex] + currentVertexOffset;
						totalEdgeCount += currentVertexOffset;
					}
				}
				int[] targets = new int[totalEdgeCount];
				temporaryTargets.AsSpan(0, totalEdgeCount).CopyTo(targets.AsSpan());
				OneBitArray removed = new(_removedVertices);
				Graph g = new(VertexCount, totalEdgeCount, offsets, targets, removed, options);
				return g;
			}
			finally
			{
				pool.Return(temporaryTargets);
			}
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool HasArc (int from, int to)
		{
			ArgumentOutOfRangeException.ThrowIfNegative(from);
			ArgumentOutOfRangeException.ThrowIfNegative(to);

			ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(from, VertexCapacity);
			ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(to, VertexCapacity);

			if ( _removedVertices[from] == 1 || _removedVertices[to] == 1 )
			{
				throw new ArgumentOutOfRangeException();
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

			return vertex < VertexCapacity && _removedVertices[vertex] == 0;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void RemoveArc (int from, int to)
		{
			ArgumentOutOfRangeException.ThrowIfNegative(from);
			ArgumentOutOfRangeException.ThrowIfNegative(to);

			if ( from >= VertexCapacity || to >= VertexCapacity
				|| _removedVertices[from] == 1 || _removedVertices[to] == 1 )
			{
				return;
			}

			int pointer = _tails[from];
			while ( pointer != -1 )
			{
				if ( _targets[pointer] == to )
				{
					int currentPosition = pointer;
					int nextPosition = _nexts[currentPosition];
					while ( nextPosition != -1 )
					{
						_nexts[currentPosition] = _nexts[nextPosition];
						currentPosition = nextPosition;
						nextPosition = _nexts[nextPosition];
					}
				}
			}
			return;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void RemoveVertex (int vertex)
		{
			ArgumentOutOfRangeException.ThrowIfNegative(vertex);

			if ( vertex >= VertexCapacity )
			{
				return;
			}
			else if ( _removedVertices[vertex] == 0 )
			{
				int pointer = _tails[vertex], next;
				while ( pointer != -1 )
				{
					next = _nexts[pointer];
					_nexts[pointer] = -1;
					pointer = next;
				}
				_removedVertices[vertex] = 1;
				VertexCount--;
			}
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		protected void EnsureEdgeCapacityOrResize (int capacity)
		{
			if ( capacity <= EdgeCapacity )
			{
				return;
			}
			else
			{
				int newCapacity = (int) BitOperations.RoundUpToPowerOf2((uint) capacity);
				ResizeEdges(newCapacity);
				EdgeCapacity = newCapacity;
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
				ResizeVertices(newCapacity);
				VertexCapacity = newCapacity;
			}
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		protected int InsertArc (int from, int to)
		{
			_targets[EdgeCount] = to;
			_nexts[EdgeCount] = _tails[from];
			_tails[from] = EdgeCount;
			return EdgeCount++;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		protected virtual void ResizeEdges (int newCapacity)
		{
			Array.Resize(ref _targets, newCapacity);
			Array.Resize(ref _nexts, newCapacity);
			_nexts.AsSpan(EdgeCount, newCapacity - EdgeCount).Fill(-1);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		protected virtual void ResizeVertices (int newCapacity)
		{
			OneBitArray.Resize(ref _removedVertices, newCapacity);
			_removedVertices.SetRange(VertexCount, newCapacity - VertexCount, 1);

			Array.Resize(ref _tails, newCapacity);
			_tails.AsSpan(VertexCount, newCapacity - VertexCount).Fill(-1);
		}
	}

	public class GraphBuilder<TEdgeWeight>
		: GraphBuilder
		where TEdgeWeight : IComparable<TEdgeWeight>
	{
		protected TEdgeWeight[] _weights;

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public GraphBuilder (int vertexCapacity = 0, int edgeCapacity = 0) : base(vertexCapacity, edgeCapacity)
		{

		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void AddArc (int from, int to, TEdgeWeight weight)
		{
			if ( !typeof(TEdgeWeight).IsValueType )
			{
				ArgumentNullException.ThrowIfNull(weight);
			}

			ArgumentOutOfRangeException.ThrowIfNegative(from);
			ArgumentOutOfRangeException.ThrowIfNegative(to);
			
			ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(from, VertexCapacity);
			ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(to, VertexCapacity);

			if ( _removedVertices[from] == 1 || _removedVertices[to] == 1 )
			{
				throw new ArgumentOutOfRangeException();
			}

			EnsureEdgeCapacityOrResize(EdgeCount + 1);

			InsertArc(from, to, weight);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void AddArcAndVertices (int from, int to, TEdgeWeight weight)
		{
			if ( !typeof(TEdgeWeight).IsValueType )
			{
				ArgumentNullException.ThrowIfNull(weight);
			}

			ArgumentOutOfRangeException.ThrowIfNegative(from);
			ArgumentOutOfRangeException.ThrowIfNegative(to);

			EnsureVertexCapacityOrResize(Math.Max(from, to) + 1);
			EnsureEdgeCapacityOrResize(EdgeCount + 1);

			if ( _removedVertices[from] == 1 )
			{
				_removedVertices[from] = 0;
				VertexCount++;
			}
			if ( _removedVertices[to] == 1 )
			{
				_removedVertices[to] = 0;
				VertexCount++;
			}

			InsertArc(from, to, weight);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void AddEdge (int v1, int v2, TEdgeWeight weight)
		{
			if ( !typeof(TEdgeWeight).IsValueType )
			{
				ArgumentNullException.ThrowIfNull(weight);
			}

			ArgumentOutOfRangeException.ThrowIfNegative(v1);
			ArgumentOutOfRangeException.ThrowIfNegative(v2);

			ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(v1, VertexCapacity);
			ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(v2, VertexCapacity);

			if ( _removedVertices[v1] == 1 || _removedVertices[v2] == 1 )
			{
				throw new ArgumentOutOfRangeException();
			}

			EnsureEdgeCapacityOrResize(EdgeCount + 2);

			if ( v1 != v2 )
			{
				InsertArc(v2, v1, weight);
			}

			InsertArc(v1, v2, weight);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void AddEdgeAndVertices (int v1, int v2, TEdgeWeight weight)
		{
			if ( !typeof(TEdgeWeight).IsValueType )
			{
				ArgumentNullException.ThrowIfNull(weight);
			}

			ArgumentOutOfRangeException.ThrowIfNegative(v1);
			ArgumentOutOfRangeException.ThrowIfNegative(v2);

			EnsureVertexCapacityOrResize(Math.Max(v1, v2) + 1);
			EnsureEdgeCapacityOrResize(EdgeCount + 2);

			if ( _removedVertices[v1] == 1 )
			{
				_removedVertices[v1] = 0;
				VertexCount++;
			}
			if ( _removedVertices[v2] == 1 )
			{
				_removedVertices[v2] = 0;
				VertexCount++;
			}

			if ( v1 != v2 )
			{
				InsertArc(v2, v1, weight);
			}

			InsertArc(v1, v2, weight);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public override Graph<TEdgeWeight> Build () => Build(GraphBuildingOptions.Default);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public override Graph<TEdgeWeight> Build (GraphBuildingOptionsFixed options)
		{
			if ( VertexCount == 0 )
			{
				return Graph<TEdgeWeight>.Empty;
			}

			int[] offsets = new int[VertexCount + 1];

			int totalEdgeCount = 0;
			ArrayPool<int> intPool = ArrayPool<int>.Shared;
			int[] temporaryTargets = intPool.Rent(EdgeCount);

			ArrayPool<TEdgeWeight> weightPool = ArrayPool<TEdgeWeight>.Shared;
			TEdgeWeight[] temporaryWeights = weightPool.Rent(EdgeCount);

			try
			{
				for ( int vertex = 0; vertex < VertexCount; vertex++ )
				{
					int pointer = _tails[vertex];
					int currentVertexOffset = 0;
					while ( pointer != -1 )
					{
						int target = _targets[pointer];
						bool add = true;
						if ( target == vertex )
						{
							if ( !options.enableReflexiveArcs )
							{
								add = false;
							}
						}
						if ( !options.enableDuplicateArcs
							&& temporaryTargets.AsSpan(offsets[vertex], currentVertexOffset).Contains(target) )
						{
							add = false;
						}

						if ( add )
						{
							temporaryTargets[offsets[vertex] + currentVertexOffset] = target;
							temporaryWeights[offsets[vertex] + currentVertexOffset++] = _weights[pointer];
						}
						pointer = _nexts[pointer];
					}
					offsets[vertex + 1] = offsets[vertex] + currentVertexOffset;
					totalEdgeCount += currentVertexOffset;
				}
				int[] targets = new int[totalEdgeCount];
				temporaryTargets.AsSpan(0, totalEdgeCount).CopyTo(targets.AsSpan());

				TEdgeWeight[] weights = new TEdgeWeight[totalEdgeCount];
				temporaryWeights.AsSpan(0, totalEdgeCount).CopyTo(weights.AsSpan());

				OneBitArray removed = new(_removedVertices);

				Graph<TEdgeWeight> g = new(VertexCount, totalEdgeCount, offsets, targets, removed, options, weights);
				return g;
			}
			finally
			{
				intPool.Return(temporaryTargets);
				weightPool.Return(temporaryWeights);
			}
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public TEdgeWeight GetWeight (int from, int to)
		{
			ArgumentOutOfRangeException.ThrowIfNegative(from);
			ArgumentOutOfRangeException.ThrowIfNegative(to);

			ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(from, VertexCapacity);
			ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(to, VertexCapacity);

			if ( _removedVertices[from] == 1 || _removedVertices[to] == 1 )
			{
				throw new ArgumentOutOfRangeException();
			}

			int pointer = _tails[from];
			while ( pointer != -1 )
			{
				if ( _targets[pointer] == to )
				{
					return _weights[pointer];
				}
				pointer = _nexts[pointer];
			}

			throw new InvalidOperationException();
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		protected void InsertArc (int from, int to, TEdgeWeight w)
		{
			int edgeCount = InsertArc(from, to);
			_weights[edgeCount] = w;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		protected override void ResizeEdges (int newCapacity)
		{
			base.ResizeEdges(newCapacity);
			Array.Resize(ref _weights, newCapacity);
		}
	}

	public class GraphBuilder<TEdgeWeight, TVertexData>
		: GraphBuilder<TEdgeWeight>
		where TEdgeWeight : IComparable<TEdgeWeight>
	{
		private TVertexData[] _data;

		public GraphBuilder (int vertexCapacity = 0, int edgeCapacity = 0) : base(vertexCapacity, edgeCapacity)
		{
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void AddVertex (int vertex, TVertexData data)
		{
			if ( !typeof(TVertexData).IsValueType )
			{
				ArgumentNullException.ThrowIfNull(data);
			}

			ArgumentOutOfRangeException.ThrowIfNegative(vertex);

			EnsureVertexCapacityOrResize(vertex + 1);

			if ( _removedVertices[vertex] == 1 )
			{
				_removedVertices[vertex] = 0;
				VertexCount++;
			}

			_data[vertex] = data;

		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void SetVertexData (int vertex, TVertexData data)
		{
			if ( !typeof(TVertexData).IsValueType )
			{
				ArgumentNullException.ThrowIfNull(data);
			}

			ArgumentOutOfRangeException.ThrowIfNegative(vertex);
			ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(vertex, VertexCapacity);

			if ( _removedVertices[vertex] == 1 )
			{
				throw new ArgumentOutOfRangeException();
			}
			
			EnsureVertexCapacityOrResize(vertex + 1);

			_data[vertex] = data;
		}

		public override Graph<TEdgeWeight, TVertexData> Build () => Build(GraphBuildingOptions.Default);

		public override Graph<TEdgeWeight, TVertexData> Build (GraphBuildingOptionsFixed options)
		{
			if ( VertexCount == 0 )
			{
				return Graph<TEdgeWeight, TVertexData>.Empty;
			}

			int[] offsets = new int[VertexCount + 1];

			int totalEdgeCount = 0;
			ArrayPool<int> intPool = ArrayPool<int>.Shared;
			int[] temporaryTargets = intPool.Rent(EdgeCount);

			ArrayPool<TEdgeWeight> weightPool = ArrayPool<TEdgeWeight>.Shared;
			TEdgeWeight[] temporaryWeights = weightPool.Rent(EdgeCount);

			try
			{
				for ( int vertex = 0; vertex < VertexCount; vertex++ )
				{
					int pointer = _tails[vertex];
					int currentVertexOffset = 0;
					while ( pointer != -1 )
					{
						int target = _targets[pointer];
						bool add = true;
						if ( target == vertex )
						{
							if ( !options.enableReflexiveArcs )
							{
								add = false;
							}
						}
						if ( !options.enableDuplicateArcs
						&& temporaryTargets.AsSpan(offsets[vertex], currentVertexOffset).Contains(target) )
						{
							add = false;
						}

						if ( add )
						{
							temporaryTargets[offsets[vertex] + currentVertexOffset] = target;
							temporaryWeights[offsets[vertex] + currentVertexOffset++] = _weights[pointer];
						}
						pointer = _nexts[pointer];
					}
					offsets[vertex + 1] = offsets[vertex] + currentVertexOffset;
					totalEdgeCount += currentVertexOffset;
				}
				int[] targets = new int[totalEdgeCount];
				temporaryTargets.AsSpan(0, totalEdgeCount).CopyTo(targets.AsSpan());

				TEdgeWeight[] weights = new TEdgeWeight[totalEdgeCount];
				temporaryWeights.AsSpan(0, totalEdgeCount).CopyTo(weights.AsSpan());

				TVertexData[] data = new TVertexData[VertexCount];
				_data.AsSpan(0, VertexCount).CopyTo(data.AsSpan());

				OneBitArray removed = new(_removedVertices);

				Graph<TEdgeWeight, TVertexData> g = new(VertexCount, totalEdgeCount, offsets, targets, removed, options, weights, data);
				return g;
			}
			finally
			{
				intPool.Return(temporaryTargets);
				weightPool.Return(temporaryWeights);
			}
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		protected override void ResizeVertices (int newCapacity)
		{
			base.ResizeVertices(newCapacity);
			Array.Resize(ref _data, newCapacity);
		}
	}
}