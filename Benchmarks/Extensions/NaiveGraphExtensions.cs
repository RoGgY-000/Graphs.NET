using System;
using System.Collections.Generic;

namespace Benchmarks.Extensions
{
	public static class NaiveGraphExtensions
	{
		/// <summary>
		/// Полный обход всего графа в ширину (BFS).
		/// Итерируется по всем вершинам от 0 до N-1. Если вершина еще не посещена,
		/// запускает BFS от нее. Гарантирует выполнение action ровно один раз для каждой вершины.
		/// </summary>
		/// <param name="graph">Граф.</param>
		/// <param name="vertexCount">Общее количество вершин (чтобы знать диапазон индексов от 0 до N-1).</param>
		/// <param name="action">Делегат, выполняемый для каждой вершины.</param>
		public static void RunNaiveBfsFull (Dictionary<int, List<int>> graph, int vertexCount, Action<int> action)
		{
			// Выделяем память под посещенные вершины (типичная аллокация для наивного подхода)
			var visited = new HashSet<int>();
			var queue = new Queue<int>();

			// Проходим последовательно по всем возможным индексам вершин
			for ( int i = 0; i < vertexCount; i++ )
			{
				// Если вершина уже была посещена в ходе предыдущих запусков BFS — пропускаем
				if ( visited.Contains(i) ) continue;

				// Запускаем стандартный BFS для новой компоненты связности
				queue.Enqueue(i);
				visited.Add(i);

				while ( queue.Count > 0 )
				{
					int current = queue.Dequeue();

					// Вызываем твой делегат
					action(current);

					if ( graph.TryGetValue(current, out var neighbors) )
					{
						foreach ( int neighbor in neighbors )
						{
							if ( !visited.Contains(neighbor) )
							{
								visited.Add(neighbor);
								queue.Enqueue(neighbor);
							}
						}
					}
				}
			}
		}

		/// <summary>
		/// Проверка достижимости вершины 'to' из вершины 'from' с помощью BFS.
		/// Возвращает true при первом обнаружении целевой вершины (ранний выход).
		/// </summary>
		public static bool NaiveBFSIsReachable (Dictionary<int, List<int>> graph, int from, int to)
		{
			// Быстрые проверки на идентичность и существование стартовой точки
			if ( from == to ) return true;
			if ( !graph.ContainsKey(from) ) return false;

			var visited = new HashSet<int>();
			var queue = new Queue<int>();

			queue.Enqueue(from);
			visited.Add(from);

			while ( queue.Count > 0 )
			{
				int current = queue.Dequeue();

				if ( graph.TryGetValue(current, out var neighbors) )
				{
					foreach ( int neighbor in neighbors )
					{
						// Ранний выход: если нашли цель, сразу завершаем алгоритм
						if ( neighbor == to ) return true;

						if ( !visited.Contains(neighbor) )
						{
							visited.Add(neighbor);
							queue.Enqueue(neighbor);
						}
					}
				}
			}

			return false;
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