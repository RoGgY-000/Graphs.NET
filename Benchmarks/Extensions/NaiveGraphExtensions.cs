using System;
using System.Collections.Generic;

namespace Benchmarks.Extensions
{
	public static class NaiveGraphExtensions
	{
		/// <summary>
		/// Наивный поиск кратчайшего пути в графе с использованием Queue, HashSet и Dictionary.
		/// </summary>
		/// <param name="graph">Граф в виде Dictionary.</param>
		/// <param name="start">Начальная вершина.</param>
		/// <param name="end">Целевая вершина.</param>
		/// <param name="destination">Буфер для записи найденного пути.</param>
		/// <param name="foundPathLength">Длина найденного пути в ребрах (или -1, если путь не найден).</param>
		/// <returns>True, если путь существует; иначе False.</returns>
		public static bool TryRunNaiveShortestPath (
			Dictionary<int, List<int>> graph,
			int start,
			int end,
			Span<int> destination,
			out int foundPathLength)
		{
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

			// Выделяем память в куче под стандартные коллекции: очередь, посещенные вершины и карту предков
			var queue = new Queue<int>();
			var visited = new HashSet<int>();
			var previousVertex = new Dictionary<int, int>();

			queue.Enqueue(start);
			visited.Add(start);

			bool pathExists = false;

			while ( queue.Count > 0 )
			{
				int vertex = queue.Dequeue();

				// Поиск списка смежности по хеш-таблице — лишний промах по кэшу процессора
				if ( graph.TryGetValue(vertex, out var neighbors) )
				{
					foreach ( int neighbor in neighbors )
					{
						if ( neighbor == end )
						{
							previousVertex[neighbor] = vertex;
							pathExists = true;
							break;
						}

						if ( visited.Add(neighbor) )
						{
							// Динамическое расширение Dictionary и Queue приводит к дополнительным аллокациям
							previousVertex[neighbor] = vertex;
							queue.Enqueue(neighbor);
						}
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

			// Классическое восстановление пути через промежуточный List с последующим разворотом
			var path = new List<int>();
			int currentVertex = end;

			while ( currentVertex != start )
			{
				path.Add(currentVertex);
				currentVertex = previousVertex[currentVertex];
			}

			path.Add(start);

			if ( path.Count > destination.Length )
			{
				throw new ArgumentException("destination span is too short for path", nameof(destination));
			}

			// Разворачиваем список и копируем в целевой буфер
			path.Reverse();
			for ( int i = 0; i < path.Count; i++ )
			{
				destination[i] = path[i];
			}

			foundPathLength = path.Count - 1;
			return true;
		}

		/// <summary>
		/// Наивный поиск циклов в ориентированном графе с использованием HashSet и рекурсии.
		/// </summary>
		/// <param name="graph">Граф в виде Dictionary.</param>
		/// <param name="vertexCount">Общее количество вершин (от 0 до N-1).</param>
		/// <returns>True, если в графе есть хотя бы один цикл; иначе False.</returns>
		public static bool RunNaiveCycleDetection (Dictionary<int, List<int>> graph, int vertexCount)

		{
			// Выделяем память под глобальный список посещенных и стек текущей рекурсии
			var visited = new HashSet<int>();
			var recStack = new HashSet<int>();

			// Перебираем абсолютно все вершины, чтобы найти циклы даже в изолированных компонентах
			for ( int i = 0; i < vertexCount; i++ )
			{
				if ( !visited.Contains(i) )
				{
					if ( HasCycleDfs(i, graph, visited, recStack) )
					{
						return true;
					}
				}
			}

			return false;
		}

		private static bool HasCycleDfs (
			int u,
			Dictionary<int, List<int>> graph,
			HashSet<int> visited,
			HashSet<int> recStack)
		{
			// Если вершина уже находится в текущем стеке рекурсии — мы нашли цикл!
			if ( recStack.Contains(u) ) return true;

			// Если она уже была полностью обработана ранее в другом обходе — тут цикла точно нет
			if ( visited.Contains(u) ) return false;

			// Помечаем вершину как посещенную глобально и добавляем в текущий путь рекурсии
			visited.Add(u);
			recStack.Add(u);

			// Проверяем всех соседей
			if ( graph.TryGetValue(u, out var neighbors) )
			{
				// foreach по List<T> — стандартный проход, бьющий по кэшу процессора
				foreach ( int neighbor in neighbors )
				{
					if ( HasCycleDfs(neighbor, graph, visited, recStack) )
					{
						return true;
					}
				}
			}

			// Перед выходом из ветки рекурсии обязательно убираем вершину из текущего пути
			recStack.Remove(u);

			return false;
		}

		public static bool TryRunNaiveTopologicalSort (Dictionary<int, List<int>> graph, int vertexCount, Span<int> destination)
		{
			var visited = new HashSet<int>();
			var recStack = new HashSet<int>();
			int resultIndex = vertexCount - 1;

			for ( int i = 0; i < vertexCount; i++ )
			{
				if ( !visited.Contains(i) )
				{
					if ( !TopSortDfs(i, graph, visited, recStack, destination, ref resultIndex) )
					{
						return false; // Нашли цикл, сортировка невозможна
					}
				}
			}

			return true;
		}

		private static bool TopSortDfs (
			int u,
			Dictionary<int, List<int>> graph,
			HashSet<int> visited,
			HashSet<int> recStack,
			Span<int> destination,
			ref int resultIndex)
		{
			if ( recStack.Contains(u) ) return false; // Цикл!
			if ( visited.Contains(u) ) return true;

			visited.Add(u);
			recStack.Add(u);

			if ( graph.TryGetValue(u, out var neighbors) )
			{
				foreach ( int neighbor in neighbors )
				{
					if ( !TopSortDfs(neighbor, graph, visited, recStack, destination, ref resultIndex) )
					{
						return false;
					}
				}
			}

			recStack.Remove(u);
			destination[resultIndex--] = u; // Запись с конца
			return true;
		}
	}
}