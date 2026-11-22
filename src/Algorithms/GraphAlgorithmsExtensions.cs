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
		/// Attempts to find the shortest path (by edge count) between two vertices in the graph using breadth-first search.
		/// </summary>
		/// <param name="graph">The target graph.</param>
		/// <param name="start">The index of the starting vertex.</param>
		/// <param name="end">The index of the target vertex.</param>
		/// <param name="destination">
		/// The buffer to write the sequence of vertex indices forming the shortest path into, ordered from <paramref name="start"/> to <paramref name="end"/>.
		/// Must have a length of at least <c>foundPathLength + 1</c> (at least <c>1</c> when <paramref name="start"/> equals <paramref name="end"/>, and at least <c>2</c> otherwise).
		/// </param>
		/// <param name="foundPathLength">
		/// When this method returns, contains the length of the found path in edges (<c>0</c> if <paramref name="start"/> equals <paramref name="end"/>),
		/// or <c>-1</c> if no path exists between <paramref name="start"/> and <paramref name="end"/>.
		/// </param>
		/// <returns>
		/// <see langword="true"/> if a path exists between <paramref name="start"/> and <paramref name="end"/> and <paramref name="destination"/> was populated with the vertex sequence;
		/// <see langword="false"/> if no path exists (in which case <paramref name="destination"/> remains unmodified).
		/// </returns>
		/// <exception cref="ArgumentOutOfRangeException">
		/// Thrown when <paramref name="start"/> or <paramref name="end"/> is negative or does not exist in <paramref name="graph"/>.
		/// </exception>
		/// <exception cref="ArgumentException">
		/// Thrown when <paramref name="destination"/> is empty, has a length less than <c>2</c> when <paramref name="start"/> differs from <paramref name="end"/>,
		/// or is too short to hold the entire path.
		/// </exception>
		public static bool TryFindShortestPath (this Graph graph, int start, int end, Span<int> destination, out int foundPathLength)
		{
			ArgumentOutOfRangeException.ThrowIfNegative(start);
			ArgumentOutOfRangeException.ThrowIfNegative(end);
			ArgumentOutOfRangeException.ThrowIfGreaterThan(start, graph._maxVertexIndex);
			ArgumentOutOfRangeException.ThrowIfGreaterThan(end, graph._maxVertexIndex);

			if ( graph._activeVertices[start] == 0 )
			{
				throw new ArgumentOutOfRangeException(nameof(start), "the vertex does not exist in the given graph");
			}

			if ( graph._activeVertices[end] == 0 )
			{
				throw new ArgumentOutOfRangeException(nameof(end), "the vertex does not exist in the given graph");
			}

			if ( destination.IsEmpty )
			{
				throw new ArgumentException("destination span is empty", nameof(destination));
			}

			if ( start == end )
			{
				destination[0] = start;
				foundPathLength = 0;
				return true;
			}

			if ( destination.Length < 2 )
			{
				throw new ArgumentException("destination span is too short for path", nameof(destination));
			}

			if ( graph.HasArc(start, end) )
			{
				destination[0] = start;
				destination[1] = end;
				foundPathLength = 1;
				return true;
			}

			int queueStart = 0, queueEnd = 0;

			ArrayPool<int> pool = ArrayPool<int>.Shared;

			int[] queue = pool.Rent(graph.VertexCount);

			int[] previousVertex = pool.Rent(graph._maxVertexIndex + 1);

			int twoBitArrayLength = TwoBitArraySpan.GetRequiredLengthFor(graph._maxVertexIndex + 1);
			int[] visitedArray = pool.Rent(twoBitArrayLength);
			visitedArray.AsSpan(0, twoBitArrayLength).Clear();
			TwoBitArraySpan visited = new(visitedArray.AsSpan(0, twoBitArrayLength));

			try
			{
				bool pathExists = false;
				queue[queueEnd++] = start;
				while ( queueEnd > queueStart )
				{
					int vertex = queue[queueStart++];
					visited[vertex] = 2;
					ReadOnlySpan<int> neighbors = graph.GetNeighbors(vertex);
					foreach ( int neighbor in neighbors )
					{
						if ( neighbor == end )
						{
							previousVertex[neighbor] = vertex;
							pathExists = true;
							break;
						}
						if ( visited[neighbor] == 0 )
						{
							previousVertex[neighbor] = vertex;
							queue[queueEnd++] = neighbor;
							visited[neighbor] = 1;
						}
					}
					if ( pathExists )
					{
						break;
					}
				}
				if ( !pathExists )
				{
					foundPathLength = -1;
					return false;
				}

				int destinationPointer = destination.Length - 1;
				int currentVertex = end;
				while ( destinationPointer > 0
					&& currentVertex != start )
				{
					destination[destinationPointer--] = currentVertex;
					currentVertex = previousVertex[currentVertex];
				}
				if ( currentVertex != start )
				{
					throw new ArgumentException("destination span is too short for path", nameof(destination));
				}
				destination[destinationPointer] = start;
				int pathLengthVertices = destination.Length - destinationPointer;
				foundPathLength = pathLengthVertices - 1;
				if ( destinationPointer > 0 )
				{
					destination.Slice(destinationPointer, pathLengthVertices).CopyTo(destination.Slice(0, pathLengthVertices));
				}
				return true;
			}
			finally
			{
				pool.Return(queue);
				pool.Return(previousVertex);
				pool.Return(visitedArray);
			}
		}

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
