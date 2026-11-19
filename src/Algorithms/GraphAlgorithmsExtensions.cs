using System.Buffers;
using System.Runtime.CompilerServices;
using Graphs.NET.Additional;
using Graphs.NET.Core;

namespace Graphs.NET.Algorithms
{
	/// <summary>
	/// Provides extension methods for running graph algorithms on <see cref="Graph"/> instances.
	/// </summary>
	public static class GraphAlgorithmsExtensions
	{
		/// <summary>
		/// Attempts to compute a topological ordering of the vertices in the directed graph.
		/// </summary>
		/// <param name="graph">The target directed graph.</param>
		/// <param name="destination">
		/// The buffer to write the topologically sorted vertex indices into. Must have a length of at least <see cref="Graph.VertexCount"/>.
		/// </param>
		/// <returns>
		/// <see langword="true"/> if the graph is acyclic and <paramref name="destination"/> was populated with a valid topological order;
		/// <see langword="false"/> if the graph contains a cycle (in which case the contents of <paramref name="destination"/> are undefined).
		/// </returns>
		/// <exception cref="ArgumentException">
		/// Thrown when <paramref name="destination"/> length is less than <see cref="Graph.VertexCount"/>.
		/// </exception>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool TryTopologicalSort (this Graph graph, Span<int> destination)
		{
			if ( destination.Length < graph.VertexCount )
			{
				throw new ArgumentException("destination span must be greater than or equal to graph vertex count", nameof(destination));
			}

			if ( graph._isAcyclicCalculated && !graph.IsAcyclic )
			{
				return false;
			}

			if ( graph._isAcyclicCalculated && graph.IsAcyclic )
			{
				TopologicalSortInCertainlyAcyclicGraph(graph, destination);
				return true;
			}

			bool success = TopologicalSortWithAcyclicityCheck(graph, destination);
			graph.IsAcyclic = success;
			graph._isAcyclicCalculated = true;

			return success;
		}

		internal static void TopologicalSortInCertainlyAcyclicGraph (Graph g, Span<int> destination)
		{
			if ( g.VertexCount == 0 )
			{
				return;
			}

			int resultIndex = g.VertexCount - 1;
			int stackEnd = 0;
			int[] vStack = ArrayPool<int>.Shared.Rent(g._maxVertexIndex + 1);
			int[] nStack = ArrayPool<int>.Shared.Rent(g._maxVertexIndex + 1);

			int twoBitArrayLength = TwoBitArraySpan.GetRequiredLengthFor(g._maxVertexIndex + 1);
			int[] visitedArray = ArrayPool<int>.Shared.Rent(twoBitArrayLength);
			visitedArray.AsSpan(0, twoBitArrayLength).Clear();
			TwoBitArraySpan visited = new(visitedArray.AsSpan());

			try
			{
				for ( int vertex = 0; vertex <= g._maxVertexIndex; vertex++ )
				{
					if ( g._activeVertices[vertex] == 1
						&& visited[vertex] == 0 )
					{
						vStack[stackEnd] = vertex;
						nStack[stackEnd] = 0;
						visited[vertex] = 1;
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

		internal static bool TopologicalSortWithAcyclicityCheck (Graph g, Span<int> destination)
		{
			if ( g.VertexCount == 0 )
			{
				return true;
			}

			int resultIndex = g.VertexCount - 1;
			int stackEnd = 0;
			int[] vStack = ArrayPool<int>.Shared.Rent(g._maxVertexIndex + 1);
			int[] nStack = ArrayPool<int>.Shared.Rent(g._maxVertexIndex + 1);

			int twoBitArrayLength = TwoBitArraySpan.GetRequiredLengthFor(g._maxVertexIndex + 1);
			int[] visitedArray = ArrayPool<int>.Shared.Rent(twoBitArrayLength);
			visitedArray.AsSpan(0, twoBitArrayLength).Clear();
			TwoBitArraySpan visited = new(visitedArray.AsSpan());

			try
			{
				for ( int vertex = 0; vertex <= g._maxVertexIndex; vertex++ )
				{
					if ( g._activeVertices[vertex] == 1
						&& visited[vertex] == 0 )
					{
						vStack[stackEnd] = vertex;
						nStack[stackEnd] = 0;
						visited[vertex] = 1;
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

		internal static bool CalculateIsAcyclic (this Graph g)
		{
			if ( g.ArcCount == 0 )
			{
				return true;
			}
			else if ( !g.IsAllowingReflexiveArcs
				&& g.ArcCount < 2 )
			{
				return true;
			}

			int stackEnd = 0;
			int[] vertexStack = ArrayPool<int>.Shared.Rent(g._maxVertexIndex + 1);
			int[] neighborStack = ArrayPool<int>.Shared.Rent(g._maxVertexIndex + 1);

			int TwoBitArrayLength = TwoBitArraySpan.GetRequiredLengthFor(g._maxVertexIndex + 1);
			int[] visitedArray = ArrayPool<int>.Shared.Rent(TwoBitArrayLength);
			visitedArray.AsSpan(0, TwoBitArrayLength).Clear();
			TwoBitArraySpan visited = new(visitedArray.AsSpan());

			try
			{
				for ( int vertex = 0; vertex <= g._maxVertexIndex; vertex++ )
				{
					if ( g._activeVertices[vertex] == 1
						&& visited[vertex] == 0 )
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
