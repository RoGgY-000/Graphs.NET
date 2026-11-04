using System;
using System.Buffers;
using QuikGraph; // Убедись, что пространство имен совпадает

public static class QuikGraphExtensions
{
	public static void RunQuikGraphBfsFull (CompressedSparseRowGraph<int> graph, int vertexCount, Action<int> action)
	{
		int qStart = 0, qEnd = 0;

		// Используем стандартный ArrayPool для честного выделения памяти без аллокаций в GC
		int[] queue = ArrayPool<int>.Shared.Rent(vertexCount);
		int[] visited = ArrayPool<int>.Shared.Rent(vertexCount);

		// Очищаем visited, так как ArrayPool возвращает замусоренные массивы
		visited.AsSpan(0, vertexCount).Clear();

		try
		{
			for ( int i = 0; i < vertexCount; i++ )
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

							// Проверяем, есть ли исходящие ребра, чтобы не плодить энумераторы на пустых вершинах
							if ( !graph.IsOutEdgesEmpty(curr) )
							{
								// OutEdges возвращает IEnumerable или кастомную коллекцию ребер Edge<int>
								foreach ( var edge in graph.OutEdges(curr) )
								{
									// У QuikGraph Edge<int> хранит Source и Target. Нам нужен Target.
									int neighbor = edge.Target;

									if ( visited[neighbor] == 0 )
									{
										visited[neighbor] = 1;
										queue[qEnd++] = neighbor;
									}
								}
							}
						}
					}
				}
			}
		}
		finally
		{
			ArrayPool<int>.Shared.Return(queue);
			ArrayPool<int>.Shared.Return(visited);
		}
	}
}
