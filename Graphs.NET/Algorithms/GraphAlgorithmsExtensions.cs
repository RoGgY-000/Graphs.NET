using System.Buffers;
using System.Runtime.CompilerServices;
using Graphs.NET.Additional;
using Graphs.NET.Core;

namespace Graphs.NET.Algorithms
{
	public static class GraphAlgorithmsExtensions
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool TryTopologicalSort (this Graph g, Span<int> destination)
		{
			if ( (destination.IsEmpty && g.VertexCount > 0) || destination.Length < g.VertexCount )
			{
				throw new ArgumentException("destination span must be greater than or equal to graph vertex count");
			}

			if ( g._isAcyclicCalculated && !g.IsAcyclic )
			{
				return false;
			}

			if ( g._isAcyclicCalculated && g.IsAcyclic )
			{
				TopologicalSortInCertainlyAcyclicGraph(g, destination);
				return true;
			}

			bool success = TopologicalSortWithAcyclicityCheck(g, destination);
			g.IsAcyclic = success;
			g._isAcyclicCalculated = true;

			return success;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		internal static void TopologicalSortInCertainlyAcyclicGraph (Graph g, Span<int> destination)
		{
			if ( g.VertexCount == 0 )
			{
				return;
			}
			else if ( g.VertexCount == 1 )
			{
				destination[0] = 0;
				return;
			}

			int resultIndex = g.VertexCount - 1;
			int stackEnd = 0;
			int[] vStack = ArrayPool<int>.Shared.Rent(g.VertexCount);
			int[] nStack = ArrayPool<int>.Shared.Rent(g.VertexCount);

			int TwoBitArrayLength = TwoBitArraySpan.GetRequiredLengthFor(g.VertexCount);
			int[] visitedArray = ArrayPool<int>.Shared.Rent(TwoBitArrayLength);
			visitedArray.AsSpan(0, TwoBitArrayLength).Clear();
			TwoBitArraySpan visited = new(visitedArray.AsSpan());

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
			if ( g.VertexCount == 0 )
			{
				return true;
			}
			else if ( g.VertexCount == 1 )
			{
				destination[0] = 0;
				return !g.HasArc(0, 0);
			}

			int resultIndex = g.VertexCount - 1;
			int stackEnd = 0;
			int[] vStack = ArrayPool<int>.Shared.Rent(g.VertexCount);
			int[] nStack = ArrayPool<int>.Shared.Rent(g.VertexCount);

			int TwoBitArrayLength = TwoBitArraySpan.GetRequiredLengthFor(g.VertexCount);
			int[] visitedArray = ArrayPool<int>.Shared.Rent(TwoBitArrayLength);
			visitedArray.AsSpan(0, TwoBitArrayLength).Clear();
			TwoBitArraySpan visited = new(visitedArray.AsSpan());

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

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		internal static bool CalculateIsAcyclic (this Graph g)
		{
			if ( g.VertexCount == 0 )
			{
				return true;
			}

			int stackEnd = 0;
			int[] vertexStack = ArrayPool<int>.Shared.Rent(g.VertexCount);
			int[] neighborStack = ArrayPool<int>.Shared.Rent(g.VertexCount);

			int TwoBitArrayLength = TwoBitArraySpan.GetRequiredLengthFor(g.VertexCount);
			int[] visitedArray = ArrayPool<int>.Shared.Rent(TwoBitArrayLength);
			visitedArray.AsSpan(0, TwoBitArrayLength).Clear();
			TwoBitArraySpan visited = new(visitedArray.AsSpan());

			try
			{
				for ( int vertex = 0; vertex < g.VertexCount; vertex++ )
				{
					if ( visited[vertex] == 0 )
					{
						vertexStack[stackEnd] = vertex;
						neighborStack[stackEnd] = 0;
						visited[vertex] = 1;
						stackEnd++;

						while ( stackEnd > 0 )
						{
							int currentVertex = vertexStack[stackEnd - 1];
							ReadOnlySpan<int> neighbors = g.GetNeighbors(currentVertex);
							int nextNeighborIndex = neighborStack[stackEnd - 1];
							if ( nextNeighborIndex >= neighbors.Length )
							{
								stackEnd--;
								visited[currentVertex] = 2;
							}
							else
							{
								int neighbor = neighbors[nextNeighborIndex];
								neighborStack[stackEnd - 1]++;
								if ( visited[neighbor] == 1 )
								{
									return false;
								}
								else if ( visited[neighbor] == 0 )
								{
									visited[neighbor] = 1;
									vertexStack[stackEnd] = neighbor;
									neighborStack[stackEnd] = 0;
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
				ArrayPool<int>.Shared.Return(vertexStack);
				ArrayPool<int>.Shared.Return(neighborStack);
			}
		}
	}
}
