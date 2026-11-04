using System.Buffers;
using System.Runtime.CompilerServices;

namespace Graphs.NET.Algorithms
{
	public static class GraphAlgorithmsExtensions
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool TryTopologicalSort (this Graph g, Span<int> destination)
		{
			if ( destination.IsEmpty || destination.Length < g.VertexCount )
			{
				throw new ArgumentException("destination span must be greater than or equal to graph vertex count");
			}

			if ( g.IsAcyclicCalculated && !g.IsAcyclic )
			{
				return false;
			}

			if ( g.IsAcyclicCalculated && g.IsAcyclic )
			{
				TopologicalSortInCertainlyAcyclicGraph(g, destination);
				return true;
			}

			bool success = TopologicalSortWithAcyclicityCheck(g, destination);
			g.IsAcyclic = success;
			g.IsAcyclicCalculated = true;

			return success;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		internal static void TopologicalSortInCertainlyAcyclicGraph (Graph g, Span<int> destination)
		{
			int resultIndex = g.VertexCount - 1;
			int stackEnd = 0;
			int[] vStack = ArrayPool<int>.Shared.Rent(g.VertexCount);
			int[] nStack = ArrayPool<int>.Shared.Rent(g.VertexCount);

			int TwoBitArrayLength = TwoBitArray.GetRequiredLengthFor(g.VertexCount);
			int[] visitedArray = ArrayPool<int>.Shared.Rent(TwoBitArrayLength);
			visitedArray.AsSpan(0, TwoBitArrayLength).Clear();
			TwoBitArray visited = new(visitedArray.AsSpan());

			try
			{
				for ( int i = 0; i < g.VertexCount; i++ )
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
							ReadOnlySpan<int> neighbors = g.GetNeighbors(curr);
							int nextNeighborIndex = nStack[stackEnd - 1];
							if ( nextNeighborIndex >= neighbors.Length )
							{
								stackEnd--;
								visited[curr] = 2;
								destination[resultIndex--] = curr;
							}
							else
							{
								int neighbor = neighbors[nextNeighborIndex];
								nStack[stackEnd - 1]++;
								if ( visited[neighbor] == 0 )
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
			}
			finally
			{
				ArrayPool<int>.Shared.Return(visitedArray);
				ArrayPool<int>.Shared.Return(vStack);
				ArrayPool<int>.Shared.Return(nStack);
			}
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		internal static bool TopologicalSortWithAcyclicityCheck (Graph g, Span<int> destination)
		{
			int resultIndex = g.VertexCount - 1;
			int stackEnd = 0;
			int[] vStack = ArrayPool<int>.Shared.Rent(g.VertexCount);
			int[] nStack = ArrayPool<int>.Shared.Rent(g.VertexCount);

			int TwoBitArrayLength = TwoBitArray.GetRequiredLengthFor(g.VertexCount);
			int[] visitedArray = ArrayPool<int>.Shared.Rent(TwoBitArrayLength);
			visitedArray.AsSpan(0, TwoBitArrayLength).Clear();
			TwoBitArray visited = new(visitedArray.AsSpan());

			try
			{
				for ( int i = 0; i < g.VertexCount; i++ )
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
							ReadOnlySpan<int> neighbors = g.GetNeighbors(curr);
							int nextNeighborIndex = nStack[stackEnd - 1];
							if ( nextNeighborIndex >= neighbors.Length )
							{
								stackEnd--;
								visited[curr] = 2;
								destination[resultIndex--] = curr;
							}
							else
							{
								int neighbor = neighbors[nextNeighborIndex];
								nStack[stackEnd - 1]++;
								if ( visited[neighbor] == 1 )
								{
									return false;
								}

								if ( visited[neighbor] == 0 )
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
}
