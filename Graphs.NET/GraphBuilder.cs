using System.Buffers;
using System.Runtime.CompilerServices;
namespace Graphs.NET
{
	public class GraphBuilder
	{
		protected const int MinCapacity = 4;

		public int VertexCount { get; protected set; }
		public int EdgeCount { get; protected set; }
		public int EdgeCapacity { get; protected set; }

		protected int _vertexCapacity;
		protected int[] _heads;
		protected int[] _targets;
		protected int[] _nexts;

		public GraphBuilder (int v = 0, int u = 0)
		{
			ArgumentOutOfRangeException.ThrowIfNegative(v);
			ArgumentOutOfRangeException.ThrowIfNegative(u);

			EnsureVertexCount(v);
			EnsureEdgeCapacity(u);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void AddArc (int from, int to)
		{
			ArgumentOutOfRangeException.ThrowIfNegative(from);
			ArgumentOutOfRangeException.ThrowIfNegative(to);

			EnsureVertexCount(Math.Max(from, to) + 1);
			EnsureEdgeCapacity(EdgeCount + 1);

			InsertArc(from, to);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void AddEdge (int v1, int v2)
		{
			ArgumentOutOfRangeException.ThrowIfNegative(v1);
			ArgumentOutOfRangeException.ThrowIfNegative(v2);

			EnsureVertexCount(Math.Max(v1, v2) + 1);
			EnsureEdgeCapacity(EdgeCount + 2);

			InsertArc(v1, v2);

			if ( v1 != v2 )
			{
				InsertArc(v2, v1);
			}
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public Graph Build ()
		{
			return Build(GraphBuildingOptions.Default);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public Graph Build (GraphBuildingOptionsFixed options)
		{
			if ( VertexCount == 0 )
			{
				return Graph.Empty;
			}

			int[] offsets = new int[VertexCount + 1];
			int[] targets = new int[EdgeCount];
			
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
					&& offsets.AsSpan(offsets[vertex], currentVertexOffset).Contains(target) )
					{
						add = false;
					}

					if ( add )
					{
						targets[offsets[vertex] + currentVertexOffset++] = _targets[pointer];
					}
					pointer = _nexts[pointer];
				}
				offsets[vertex+1] = offsets[vertex] + currentVertexOffset;
			}
			Graph g = new(offsets, targets, options);
			return g;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		protected virtual void EnsureEdgeCapacity (int capacity)
		{
			if ( capacity <= EdgeCapacity )
			{
				return;
			}
			else
			{
				capacity--;
				capacity |= capacity >> 1;
				capacity |= capacity >> 2;
				capacity |= capacity >> 4;
				capacity |= capacity >> 8;
				capacity |= capacity >> 16;
				capacity++;

				EdgeCapacity = Math.Max(MinCapacity, capacity);

				Array.Resize(ref _targets, EdgeCapacity);
				Array.Resize(ref _nexts, EdgeCapacity);
				_nexts.AsSpan(EdgeCount, EdgeCapacity - EdgeCount).Fill(-1);
			}
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		protected virtual void EnsureVertexCount (int count)
		{
			if ( count <= VertexCount )
			{
				return;
			}
			else if ( count <= _vertexCapacity )
			{
				VertexCount = count;
			}
			else
			{
				int capacity = count;
				capacity--;
				capacity |= capacity >> 1;
				capacity |= capacity >> 2;
				capacity |= capacity >> 4;
				capacity |= capacity >> 8;
				capacity |= capacity >> 16;
				capacity++;

				_vertexCapacity = Math.Max(MinCapacity, capacity);

				Array.Resize(ref _heads, _vertexCapacity);
				_heads.AsSpan(VertexCount, _vertexCapacity - VertexCount).Fill(-1);
				VertexCount = count;
			}
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private void InsertArc (int from, int to)
		{
			_targets[EdgeCount] = to;
			_nexts[EdgeCount] = _heads[from];
			_heads[from] = EdgeCount;
			EdgeCount++;
		}
	}

	public class GraphBuilder<TEdgeWeight, TVertexData>
		: GraphBuilder
		where TEdgeWeight : IComparable<TEdgeWeight>
		where TVertexData : IComparable<TVertexData>
	{

		public bool HasWeight { get; }
		public bool HasVertexData { get; }

		private TEdgeWeight[]? _weights;
		private TVertexData[]? _data;

		public GraphBuilder (int v = 0, int u = 0) : base(v, u)
		{
			HasVertexData = typeof(TVertexData) != typeof(Nothing);
			HasWeight = typeof(TEdgeWeight) != typeof(Nothing);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void AddArc (int from, int to, TEdgeWeight w)
		{
			ArgumentOutOfRangeException.ThrowIfNegative(from);
			ArgumentOutOfRangeException.ThrowIfNegative(to);
			ArgumentNullException.ThrowIfNull(w);

			if ( !HasWeight
				|| _weights == null )
			{
				throw new InvalidOperationException();
			}

			EnsureVertexCount(Math.Max(from, to) + 1);
			EnsureEdgeCapacity(EdgeCount + 1);

            InsertArc(from, to, w);
        }

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void AddEdge (int v1, int v2, TEdgeWeight w)
        {
			ArgumentOutOfRangeException.ThrowIfNegative(v1);
			ArgumentOutOfRangeException.ThrowIfNegative(v2);
            ArgumentNullException.ThrowIfNull(w);

            if ( !HasWeight
                || _weights == null )
            {
                throw new InvalidOperationException();
            }

            EnsureVertexCount(Math.Max(v1, v2) + 1);
            EnsureEdgeCapacity(EdgeCount + 2);

			InsertArc(v1, v2, w);
			InsertArc(v2, v1, w);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void SetVertexData (int v, TVertexData d)
		{
			ArgumentOutOfRangeException.ThrowIfNegative(v);
			ArgumentNullException.ThrowIfNull(d);
			if ( !HasVertexData
				|| _data == null )
			{
				throw new InvalidOperationException();
			}
			EnsureVertexCount(v + 1);
			_data[v] = d;
		}

		public Graph<TEdgeWeight, TVertexData> BuildWithEdgeWeightsAndVertexData ()
		{
			return BuildWithEdgeWeightsAndVertexData(GraphBuildingOptions.Default);
		}

		public Graph<TEdgeWeight, TVertexData> BuildWithEdgeWeightsAndVertexData (GraphBuildingOptionsFixed options)
		{
			if ( VertexCount == 0 )
			{
				return Graph<TEdgeWeight, TVertexData>.GetEmpty();
			}

			int[] offsets = new int[VertexCount + 1];
			int[] targets = new int[EdgeCount];
			offsets[VertexCount] = EdgeCount;

			TEdgeWeight[]? newWeights = null;
			if ( HasWeight )
			{
				newWeights = new TEdgeWeight[EdgeCount];
			}

			int pointer = _heads[0];
			int currentVertexOffset = 0;
			while ( pointer != -1 )
			{
				int target = _targets[pointer];
				bool add = true;
				if ( target == 0 )
				{
					if ( !options.enableReflexiveArcs )
					{
						add = false;
					}
				}
				if ( !options.enableDuplicateArcs
					&& offsets.AsSpan(0, currentVertexOffset).Contains(target) )
				{
					add = false;
				}

				if ( add )
				{
					if ( HasWeight )
					{
						newWeights![currentVertexOffset] = _weights![pointer];
					}
					targets[currentVertexOffset++] = _targets[pointer];
				}
				pointer = _nexts[pointer];
			}

			offsets[1] = currentVertexOffset;

			for ( int vertex = 1; vertex < VertexCount; vertex++ )
			{
				pointer = _heads[vertex];
				currentVertexOffset = 0;
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
					&& offsets.AsSpan(offsets[vertex], currentVertexOffset).Contains(target) )
					{
						add = false;
					}

					if ( add )
					{
						if ( HasWeight )
						{
							newWeights![offsets[vertex] + currentVertexOffset] = _weights![pointer];
						}
						targets[offsets[vertex] + currentVertexOffset++] = _targets[pointer];
					}
					pointer = _nexts[pointer];
				}
				offsets[vertex + 1] = offsets[vertex] + currentVertexOffset;
			}
			Graph<TEdgeWeight, TVertexData> g = new(offsets, targets, options, newWeights, _data);
			return g;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		protected override void EnsureEdgeCapacity (int capacity)
		{
			if ( capacity <= EdgeCapacity )
			{
				return;
			}
			else
			{
				capacity--;
				capacity |= capacity >> 1;
				capacity |= capacity >> 2;
				capacity |= capacity >> 4;
				capacity |= capacity >> 8;
				capacity |= capacity >> 16;
				capacity++;

				EdgeCapacity = Math.Max(MinCapacity, capacity);

				Array.Resize(ref _targets, EdgeCapacity);
				Array.Resize(ref _nexts, EdgeCapacity);
				_nexts.AsSpan(EdgeCount, EdgeCapacity - EdgeCount).Fill(-1);

				if ( HasWeight )
				{
					Array.Resize(ref _weights, EdgeCapacity);
				}
			}
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		protected override void EnsureVertexCount (int count)
		{
			if ( count <= VertexCount )
			{
				return;
			}
			else if ( count <= _vertexCapacity )
			{
				VertexCount = count;
			}
			else
			{
				int capacity = count;
				capacity--;
				capacity |= capacity >> 1;
				capacity |= capacity >> 2;
				capacity |= capacity >> 4;
				capacity |= capacity >> 8;
				capacity |= capacity >> 16;
				capacity++;

				_vertexCapacity = Math.Max(MinCapacity, capacity);

				Array.Resize(ref _heads, _vertexCapacity);
				_heads.AsSpan(VertexCount, _vertexCapacity - VertexCount).Fill(-1);

				if ( HasVertexData )
				{
					Array.Resize(ref _data, VertexCount);
				}
				VertexCount = count;
			}
        }

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private void InsertArc (int from, int to, TEdgeWeight w)
        {
			_targets[EdgeCount] = to;
			_nexts[EdgeCount] = _heads[from];
			_heads[from] = EdgeCount;
            _weights![EdgeCount] = w;
			EdgeCount++;
		}
    }
}