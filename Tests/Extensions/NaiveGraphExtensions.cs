namespace Tests.Extensions
{
	public static class NaiveGraphExtensions
	{
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
			foreach ( int vertex in graph.Keys.Order() )
			{
				if ( !visited.Contains(vertex) )
				{
					if ( HasCycleDfs(vertex, graph, visited, recStack) )
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

		public static bool TryRunNaiveTopologicalSort (Dictionary<int, int[]> graph, int vertexCount, Span<int> destination)
		{
			var visited = new HashSet<int>();
			var recStack = new HashSet<int>();
			int resultIndex = vertexCount - 1;

			foreach ( int vertex in graph.Keys.Order() )
			{
				if ( !visited.Contains(vertex) )
				{
					if ( !TopSortDfs(vertex, graph, visited, recStack, destination, ref resultIndex) )
					{
						return false; // Нашли цикл, сортировка невозможна
					}
				}
			}

			return true;
		}

		private static bool TopSortDfs (
			int u,
			Dictionary<int, int[]> graph,
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