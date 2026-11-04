using System.Buffers;
using System.Runtime.CompilerServices;
using Graphs.NET.Algorithms;

namespace Graphs.NET
{
	public class Graph : IDisposable
	{
		public static readonly Graph Empty = new([0], [], GraphBuildingOptions.Default);

		internal bool IsAcyclicCalculated = false;

		protected readonly int[] _offsets;
		protected readonly int[] _targets;

		protected readonly ArrayPool<int> _intPool = ArrayPool<int>.Shared;

		protected readonly GraphBuildingOptionsFixed _options;

		public int VertexCount { get; }
		public int ArcCount { get; }

		public bool IsAllowingDuplicateArcs => _options.enableDuplicateArcs;
		public bool IsAllowingReflexiveArcs => _options.enableReflexiveArcs;
		public bool IsAlwaysReflexive => _options.alwaysReflexiveArcs;

		public float AverageVertexDegree
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get
			{
				if ( field > -1f )
				{
					return field;
				}
				else if ( VertexCount == 0 )
				{
					field = 0f;
					return field;
				}
				field = 2f * ArcCount / VertexCount;
				return field;
			}
		} = -1f;
		public int DuplicateArcCount
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get
			{
				if ( field > -1 )
				{
					return field;
				}
				int res = 0;
				int currentNeighbor = -1;
				for ( int vertex = 0; vertex < VertexCount; vertex++ )
				{
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
				field = res;
				return field;
			}
		} = -1;
		public int ReflexiveVertexCount
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get
			{
				if ( field > -1 )
				{
					return field;
				}
				int res = 0;
				for ( int vertex = 0; vertex < VertexCount; vertex++ )
				{
					ReadOnlySpan<int> neighbors = GetNeighbors(vertex);
					int reflexiveCount = neighbors.Count(vertex);
					res += reflexiveCount;
				}
				field = res;
				return field;
			}
		} = -1;

		public bool IsAcyclic
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get
			{
				if ( IsAcyclicCalculated )
				{
					return field;
				}
				field = CalculateIsAcyclic();
				IsAcyclicCalculated = true;
				return field;
			}
			internal set;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		internal Graph (int[] offsets, int[] targets, GraphBuildingOptionsFixed options, bool sortNeighbors = true)
		{
			ArgumentNullException.ThrowIfNull(offsets);
			ArgumentNullException.ThrowIfNull(targets);

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
		public void Dispose ()
		{
			_intPool.Return(_offsets);
			_intPool.Return(_targets);
			GC.SuppressFinalize(this);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public ReadOnlySpan<int> GetNeighbors (int v)
		{
			ArgumentOutOfRangeException.ThrowIfNegative(v);
			ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(v, VertexCount);

			return _targets.AsSpan(_offsets[v], _offsets[v + 1] - _offsets[v]);
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

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		internal bool BFSIsReachable (int start, int end)
		{
			ArgumentOutOfRangeException.ThrowIfNegative(start);
			ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(start, VertexCount);
			ArgumentOutOfRangeException.ThrowIfNegative(end);
			ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(end, VertexCount);

			if ( IsAlwaysReflexive
				&& start == end )
			{
				return true;
			}

			int qStart = 0, qEnd = 0;
			int[] queue = _intPool.Rent(VertexCount);

			int TwoBitArrayLength = TwoBitArray.GetRequiredLengthFor(VertexCount);
			int[] visitedArray = _intPool.Rent(TwoBitArrayLength);
			visitedArray.AsSpan(0, TwoBitArrayLength).Clear();
			TwoBitArray visited = new(visitedArray.AsSpan());

			try
			{
				Enqueue(start);
				visited[start] = 1;
				while ( qEnd - qStart > 0 )
				{
					int curr = Dequeue();
					ReadOnlySpan<int> neighbors = GetNeighbors(curr);
					if ( neighbors.BinarySearch(end) >= 0 )
					{
						return true;
					}
					for ( int i = 0; i < neighbors.Length; i++ )
					{
						int neighbor = neighbors[i];

						if ( visited[neighbor] == 0 )
						{
							Enqueue(neighbor);
							visited[neighbor] = 1;
						}
					}
				}
			}
			finally
			{
				_intPool.Return(visitedArray);
				_intPool.Return(queue);
			}
			return false;

			void Enqueue (int el)
			{
				queue[qEnd++] = el;
			}
			int Dequeue ()
			{
				return queue[qStart++];
			}
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		internal Graph BFSGetReachableFrom (int v)
		{
			ArgumentOutOfRangeException.ThrowIfNegative(v);
			ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(v, VertexCount);

			int qStart = 0, qEnd = 0, newVertexCounter = 0;
			int[] queue = _intPool.Rent(VertexCount);
			int[] visited = _intPool.Rent(VertexCount);
			int[] newVertexes = _intPool.Rent(VertexCount);
			queue.AsSpan(0, VertexCount).Clear();
			visited.AsSpan(0, VertexCount).Clear();
			newVertexes.AsSpan(0, VertexCount).Clear();

			try
			{
				GraphBuilder<Nothing, Nothing> builder = new(1, ArcCount);
				Enqueue(v);
				while ( qEnd > qStart )
				{
					int curr = Dequeue();
					visited[curr] = 1;
					ReadOnlySpan<int> neighbors = GetNeighbors(curr);
					for ( int i = 0; i < neighbors.Length; i++ )
					{
						if ( visited[neighbors[i]] == 0 )
						{
							newVertexes[neighbors[i]] = ++newVertexCounter;
							Enqueue(neighbors[i]);
							visited[neighbors[i]] = 1;
						}

						builder.AddArc(newVertexes[curr], newVertexes[neighbors[i]]);
					}
				}
				return builder.Build(_options);
			}
			finally
			{
				_intPool.Return(queue);
				_intPool.Return(visited);
				_intPool.Return(newVertexes);
			}

			void Enqueue (int el)
			{
				queue[qEnd++] = el;
			}
			int Dequeue ()
			{
				return queue[qStart++];
			}
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		internal void BFSAll (Action<int> action)
		{
			int visitedCount = 0, qStart = 0, qEnd = 0;
			int[] queue = _intPool.Rent(VertexCount);
			int visitedTwoBitArrayLength = TwoBitArray.GetRequiredLengthFor(VertexCount);
			int[] visitedArray = _intPool.Rent(visitedTwoBitArrayLength);
			visitedArray.AsSpan(0, visitedTwoBitArrayLength).Clear();
			TwoBitArray visited = new(visitedArray.AsSpan());
			try
			{
				for ( int i = 0; i < VertexCount; i++ )
				{
					if ( visited[i] == 0 )
					{
						queue[qEnd++] = i;
						while ( qEnd > qStart )
						{
							int curr = queue[qStart++];
							if ( visited[curr] < 2 )
							{
								action(curr);
								visited[curr] = 2;
								visitedCount++;
								ReadOnlySpan<int> neighbors = GetNeighbors(curr);
								for ( int j = 0; j < neighbors.Length; j++ )
								{
									if ( visited[neighbors[j]] == 0 )
									{
										visited[neighbors[j]] = 1;
										queue[qEnd++] = neighbors[j];
									}
								}
							}
						}
					}
				}
			}
			finally
			{
				_intPool.Return(queue);
				_intPool.Return(visitedArray);
			}
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		internal bool DFSIsReachable (int start, int end)
		{
			ArgumentOutOfRangeException.ThrowIfNegative(start);
			ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(start, VertexCount);
			ArgumentOutOfRangeException.ThrowIfNegative(end);
			ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(end, VertexCount);

			if ( IsAlwaysReflexive
				&& start == end )
			{
				return true;
			}

			int stackEnd = 0;
			int[] stack = _intPool.Rent(VertexCount);

			int TwoBitArrayLength = TwoBitArray.GetRequiredLengthFor(VertexCount);
			int[] visitedArray = _intPool.Rent(TwoBitArrayLength);
			visitedArray.AsSpan(0, TwoBitArrayLength).Clear();
			TwoBitArray visited = new(visitedArray.AsSpan());

			try
			{
				Push(start);
				visited[start] = 1;
				while ( stackEnd > 0 )
				{
					int curr = Pop();
					visited[curr] = 1;
					ReadOnlySpan<int> neighbors = GetNeighbors(curr);
					if ( neighbors.BinarySearch(end) >= 0 )
					{
						return true;
					}
					for ( int i = 0; i < neighbors.Length; i++ )
					{
						int neighbor = neighbors[i];

						if ( visited[neighbor] == 0 )
						{
							Push(neighbor);
							visited[neighbor] = 1;
						}
					}
				}
				return false;
			}
			finally
			{
				_intPool.Return(visitedArray);
				_intPool.Return(stack);
			}

			void Push (int el)
			{
				stack[stackEnd++] = el;
			}
			int Pop ()
			{
				return stack[--stackEnd];
			}
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		internal bool CalculateIsAcyclic ()
		{
			if ( VertexCount == 0 )
			{
				return true;
			}
			if ( IsAlwaysReflexive )
			{
				return false;
			}

			int stackEnd = 0;
			int[] vStack = ArrayPool<int>.Shared.Rent(VertexCount);
			int[] nStack = ArrayPool<int>.Shared.Rent(VertexCount);

			int TwoBitArrayLength = TwoBitArray.GetRequiredLengthFor(VertexCount);
			int[] visitedArray = ArrayPool<int>.Shared.Rent(TwoBitArrayLength);
			visitedArray.AsSpan(0, TwoBitArrayLength).Clear();
			TwoBitArray visited = new(visitedArray.AsSpan());

			try
			{
				for ( int i = 0; i < VertexCount; i++ )
				{
					if ( visited[i] == 0 )
					{
						vStack[stackEnd] = i;
						nStack[stackEnd] = 0;
						visited[i] = 1;
						stackEnd++;

						while ( stackEnd > 0 )
						{
							int curr = vStack[stackEnd - 1];
							ReadOnlySpan<int> neighbors = GetNeighbors(curr);
							int nextNeighborIndex = nStack[stackEnd - 1];
							if ( nextNeighborIndex >= neighbors.Length )
							{
								stackEnd--;
								visited[curr] = 2;
							}
							else
							{
								int neighbor = neighbors[nextNeighborIndex];
								nStack[stackEnd - 1]++;
								if ( visited[neighbor] == 1 )
								{
									return false;
								}
								else if ( visited[neighbor] == 0 )
								{
									visited[neighbor] = 1;
									vStack[stackEnd] = neighbor;
									nStack[stackEnd] = 0;
									stackEnd++;
								}
							}
						}
					}
				}
				return true;
			}
			finally
			{
				ArrayPool<int>.Shared.Return(visitedArray);
				ArrayPool<int>.Shared.Return(vStack);
				ArrayPool<int>.Shared.Return(nStack);
			}
		}
	}

	public class Graph<TEdgeWeight, TVertexData>
		: Graph
		where TEdgeWeight : IComparable<TEdgeWeight>
		where TVertexData : IComparable<TVertexData>
	{
		public static Graph<TEdgeWeight, TVertexData> GetEmpty () => new([0], [], GraphBuildingOptions.Default);

		private readonly TEdgeWeight[]? _weights;
		private readonly TVertexData[]? _data;

		public bool HasWeight { get; }
		public bool HasVertexData { get; }

		public int MaxVertexDegree
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get
			{
				if ( field != -1f )
				{
					return field;
				}
				int max = 0;
				for ( int vertex = 0; vertex < VertexCount; vertex++ )
				{
					ReadOnlySpan<int> neighbors = GetNeighbors(vertex);
					int reflexiveCount = neighbors.Count(vertex);
					max = Math.Max(max, neighbors.Length + reflexiveCount);
				}
				field = max;
				return field;
			}
		} = -1;
		public float Density
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get
			{
				if ( field != -1f )
				{
					return field;
				}
				else if (VertexCount < 2)
				{
					field = 0f;
					return field;
				}
				field = (float) (ArcCount - ReflexiveVertexCount - DuplicateArcCount) / (VertexCount * (VertexCount - 1));
				return field;
			}
		} = -1f;

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		internal Graph (int[] offsets, int[] targets, GraphBuildingOptionsFixed options,
			TEdgeWeight[]? weights = null, TVertexData[]? data = null) 
			: base(offsets, targets, options, false)
		{
			if ( typeof(TEdgeWeight) != typeof(Nothing) )
			{
				ArgumentNullException.ThrowIfNull(weights);
				ArgumentOutOfRangeException.ThrowIfNotEqual(targets.Length, weights.Length);
				HasWeight = true;
				_weights = weights;
			}

			if ( typeof(TVertexData) != typeof(Nothing) )
			{
				ArgumentNullException.ThrowIfNull(data);
				ArgumentOutOfRangeException.ThrowIfNotEqual(offsets.Length, data.Length + 1);
				HasVertexData = true;
				_data = data;
			}

			for ( int vertex = 0; vertex < VertexCount; vertex++ )
			{
				Span<int> neighbors = targets.AsSpan(offsets[vertex], offsets[vertex + 1] - offsets[vertex]);
				if ( HasWeight )
				{
					Span<TEdgeWeight> neighborWeights = weights.AsSpan(offsets[vertex], offsets[vertex + 1] - offsets[vertex]);
					neighbors.Sort(neighborWeights);
				}
				else
				{
					neighbors.Sort();
				}
			}
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public ReadOnlySpan<TEdgeWeight> GetWeights (int v)
		{
			ArgumentOutOfRangeException.ThrowIfNegative(v);
			ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(v, VertexCount);

			if ( !HasWeight )
			{
				throw new InvalidOperationException("Graph does not has weights");
			}

			int start = _offsets[v];
			int end = _offsets[v + 1];
			ReadOnlySpan<TEdgeWeight> span = _weights.AsSpan(start, end - start);
			return span;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public TVertexData GetVertexData (int v)
		{
			ArgumentOutOfRangeException.ThrowIfNegative(v);
			ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(v, VertexCount);

			return HasVertexData ? _data![v] : throw new InvalidOperationException();
		}
	}
}