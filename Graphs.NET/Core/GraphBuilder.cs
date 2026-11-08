using System.Buffers;
using System.Numerics;
using System.Runtime.CompilerServices;

namespace Graphs.NET.Core
{
	public class GraphBuilder
	{
		protected int _vertexCapacity;
		protected int[] _heads;
		protected int[] _targets;
		protected int[] _nexts;

		public int VertexCount { get; protected set; }
		public int EdgeCapacity { get; protected set; }
		public int EdgeCount { get; protected set; }

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public GraphBuilder (int v = 0, int u = 0)
		{
			ArgumentOutOfRangeException.ThrowIfNegative(v);
			ArgumentOutOfRangeException.ThrowIfNegative(u);

			EnsureVertexCountOrResize(v);
			EnsureEdgeCapacityOrResize(u);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void AddArc (int from, int to)
		{
			ArgumentOutOfRangeException.ThrowIfNegative(from);
			ArgumentOutOfRangeException.ThrowIfNegative(to);

			EnsureVertexCountOrResize(Math.Max(from, to) + 1);
			EnsureEdgeCapacityOrResize(EdgeCount + 1);

			InsertArc(from, to);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void AddEdge (int v1, int v2)
		{
			ArgumentOutOfRangeException.ThrowIfNegative(v1);
			ArgumentOutOfRangeException.ThrowIfNegative(v2);

			EnsureVertexCountOrResize(Math.Max(v1, v2) + 1);
			EnsureEdgeCapacityOrResize(EdgeCount + 2);

			if ( v1 != v2 )
			{
				InsertArc(v2, v1);
			}

			InsertArc(v1, v2);
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
				for ( int vertex = 0; vertex < VertexCount; vertex++ )
				{
					int pointer = _heads[vertex];
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
				int[] targets = new int[totalEdgeCount];
				temporaryTargets.AsSpan(0, totalEdgeCount).CopyTo(targets.AsSpan());
				Graph g = new(offsets, targets, options);
				return g;
			}
			finally
			{
				pool.Return(temporaryTargets);
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
		protected void EnsureVertexCountOrResize (int count)
		{
			if ( count <= VertexCount )
			{
				return;
			}
			else if ( count <= _vertexCapacity )
			{
				VertexCount = count;
				return;
			}
			else
			{
				int newCapacity = (int) BitOperations.RoundUpToPowerOf2((uint) count);
				ResizeVertexes(newCapacity);
				_vertexCapacity = newCapacity;
				VertexCount = count;
			}
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		protected int InsertArc (int from, int to)
		{
			_targets[EdgeCount] = to;
			_nexts[EdgeCount] = _heads[from];
			_heads[from] = EdgeCount;
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
		protected virtual void ResizeVertexes (int newCapacity)
		{
			Array.Resize(ref _heads, newCapacity);
			_heads.AsSpan(VertexCount, newCapacity - VertexCount).Fill(-1);
		}
	}

	public class GraphBuilder<TEdgeWeight>
		: GraphBuilder
		where TEdgeWeight : IComparable<TEdgeWeight>
	{
		protected TEdgeWeight[] _weights;

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public GraphBuilder (int v = 0, int u = 0) : base(v, u)
		{

		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void AddArc (int from, int to, TEdgeWeight w)
		{
			ArgumentOutOfRangeException.ThrowIfNegative(from);
			ArgumentOutOfRangeException.ThrowIfNegative(to);
			
			if ( !typeof(TEdgeWeight).IsValueType )
			{
				ArgumentNullException.ThrowIfNull(w);
			}

			EnsureVertexCountOrResize(Math.Max(from, to) + 1);
			EnsureEdgeCapacityOrResize(EdgeCount + 1);

			InsertArc(from, to, w);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void AddEdge (int v1, int v2, TEdgeWeight w)
		{
			ArgumentOutOfRangeException.ThrowIfNegative(v1);
			ArgumentOutOfRangeException.ThrowIfNegative(v2);

			if ( !typeof(TEdgeWeight).IsValueType )
			{
				ArgumentNullException.ThrowIfNull(w);
			}

			EnsureEdgeCapacityOrResize(EdgeCount + 2);
			EnsureVertexCountOrResize(Math.Max(v1, v2) + 1);

			if ( v1 != v2 )
			{
				InsertArc(v2, v1, w);
			}

			InsertArc(v1, v2, w);
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
					int pointer = _heads[vertex];
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

				Graph<TEdgeWeight> g = new(offsets, targets, options, weights);
				return g;
			}
			finally
			{
				intPool.Return(temporaryTargets);
				weightPool.Return(temporaryWeights);
			}
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

		public GraphBuilder (int v = 0, int u = 0) : base(v, u)
		{
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void SetVertexData (int v, TVertexData d)
		{
			ArgumentOutOfRangeException.ThrowIfNegative(v);
			
			if ( !typeof(TVertexData).IsValueType )
			{
				ArgumentNullException.ThrowIfNull(d);
			}

			EnsureVertexCountOrResize(v + 1);
			_data[v] = d;
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
					int pointer = _heads[vertex];
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

				Graph<TEdgeWeight, TVertexData> g = new(offsets, targets, options, weights, data);
				return g;
			}
			finally
			{
				intPool.Return(temporaryTargets);
				weightPool.Return(temporaryWeights);
			}
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		protected override void ResizeVertexes (int newCapacity)
		{
			base.ResizeVertexes(newCapacity);
			Array.Resize(ref _data, newCapacity);
		}
	}
}