using Graphs.NET.Core;
using Tests.Extensions;

namespace Tests.Data
{
	public static class GraphTestData
	{
		public static IEnumerable<object[]> GetGraphsAndExpected ()
		{
			// 1) empty graph
			{
				Graph empty = Graph.Empty;

				yield return new object[] {
					empty,
					new ExpectedGraph {
						VertexCount = 0,
						ArcCount = 0,
						AverageVertexDegree = 0f,
						DuplicateArcCount = 0,
						ReflexiveArcsCount = 0,
						IsAcyclic = true,
						VerticesAndArcs = []
					}
				};
			}

			// 2) 1 vertex, 0 arcs
			{
				GraphBuilder builder = new();
				builder.AddVertex(0);

				yield return new object[] {
					builder.Build(),
					new ExpectedGraph {
						VertexCount = 1,
						ArcCount = 0,
						AverageVertexDegree = 0f,
						DuplicateArcCount = 0,
						ReflexiveArcsCount = 0,
						IsAcyclic = true,
						VerticesAndArcs = new()
						{
							{ 0, [] }
						}
					}
				};
			}

			// 3) 1 vertex, 1 reflexive arc
			{
				GraphBuilder builder = new();
				builder.AddArcAndVertices(0, 0);

				yield return new object[] {
					builder.Build(),
					new ExpectedGraph {
						VertexCount = 1,
						ArcCount = 1,
						AverageVertexDegree = 2f,
						DuplicateArcCount = 0,
						ReflexiveArcsCount = 1,
						IsAcyclic = false,
						VerticesAndArcs = new()
						{
							{ 0, [0] }
						}
					}
				};
			}

			// 4) 1 vertex, 2 reflexive arcs
			{
				GraphBuilder builder = new();
				builder.AddArcAndVertices(0, 0);
				builder.AddArcAndVertices(0, 0);

				yield return new object[] {
					builder.Build(),
					new ExpectedGraph {
						VertexCount = 1,
						ArcCount = 2,
						AverageVertexDegree = 4f,
						DuplicateArcCount = 1,
						ReflexiveArcsCount = 2,
						IsAcyclic = false,
						VerticesAndArcs = new()
						{
							{ 0, [0, 0] }
						}
					}
				};
			}

			// 5) 2 vertexes, 0 arcs
			{
				GraphBuilder builder = new();
				builder.AddVertices([0, 1]);

				yield return new object[] {
					builder.Build(),
					new ExpectedGraph {
						VertexCount = 2,
						ArcCount = 0,
						AverageVertexDegree = 0f,
						DuplicateArcCount = 0,
						ReflexiveArcsCount = 0,
						IsAcyclic = true,
						VerticesAndArcs = new()
						{
							{ 0, [] },
							{ 1, [] }
						}
					}
				};
			}

			// 6) 2 vertexes, 1 reflexive arc
			{
				GraphBuilder builder = new();
				builder.AddArcAndVertices(1, 1);
				builder.AddVertex(0);

				yield return new object[] {
					builder.Build(),
					new ExpectedGraph {
						VertexCount = 2,
						ArcCount = 1,
						AverageVertexDegree = 1f,
						DuplicateArcCount = 0,
						ReflexiveArcsCount = 1,
						IsAcyclic = false,
						VerticesAndArcs = new()
						{
							{ 0, [] },
							{ 1, [1] }
						}
					}
				};
			}

			// 7) 2 vertexes, 2 reflexive arcs
			{
				GraphBuilder builder = new();
				builder.AddArcAndVertices(0, 0);
				builder.AddArcAndVertices(1, 1);

				yield return new object[] {
					builder.Build(),
					new ExpectedGraph {
						VertexCount = 2,
						ArcCount = 2,
						AverageVertexDegree = 2f,
						DuplicateArcCount = 0,
						ReflexiveArcsCount = 2,
						IsAcyclic = false,
						VerticesAndArcs = new()
						{
							{ 0, [0] },
							{ 1, [1] }
						}
					}
				};
			}

			// 8) 2 vertexes, 3 reflexive arcs
			{
				GraphBuilder builder = new();
				builder.AddArcAndVertices(0, 0);
				builder.AddArcAndVertices(0, 0);
				builder.AddArcAndVertices(1, 1);

				yield return new object[] {
					builder.Build(),
					new ExpectedGraph {
						VertexCount = 2,
						ArcCount = 3,
						AverageVertexDegree = 3f,
						DuplicateArcCount = 1,
						ReflexiveArcsCount = 3,
						IsAcyclic = false,
						VerticesAndArcs = new()
						{
							{ 0, [0, 0] },
							{ 1, [1] }
						}
					}
				};
			}

			// 9) 2 vertexes, 1 arc
			{
				GraphBuilder builder = new();
				builder.AddArcAndVertices(0, 1);

				yield return new object[] {
					builder.Build(),
					new ExpectedGraph {
						VertexCount = 2,
						ArcCount = 1,
						AverageVertexDegree = 1f,
						DuplicateArcCount = 0,
						ReflexiveArcsCount = 0,
						IsAcyclic = true,
						VerticesAndArcs = new()
						{
							{ 0, [1] },
							{ 1, [] }
						}
					}
				};
			}

			// 10) 2 vertexes, 2 arcs
			{
				GraphBuilder builder = new();
				builder.AddArcAndVertices(0, 1);
				builder.AddArcAndVertices(1, 0);

				yield return new object[] {
					builder.Build(),
					new ExpectedGraph {
						VertexCount = 2,
						ArcCount = 2,
						AverageVertexDegree = 2f,
						DuplicateArcCount = 0,
						ReflexiveArcsCount = 0,
						IsAcyclic = false,
						VerticesAndArcs = new()
						{
							{ 0, [1] },
							{ 1, [0] }
						}
					}
				};
			}

			// 11) 2 vertexes, 3 arcs
			{
				GraphBuilder builder = new();
				builder.AddArcAndVertices(0, 1);
				builder.AddArcAndVertices(1, 0);
				builder.AddArcAndVertices(0, 1);

				yield return new object[] {
					builder.Build(),
					new ExpectedGraph {
						VertexCount = 2,
						ArcCount = 3,
						AverageVertexDegree = 3f,
						DuplicateArcCount = 1,
						ReflexiveArcsCount = 0,
						IsAcyclic = false,
						VerticesAndArcs = new()
						{
							{ 0, [1, 1] },
							{ 1, [0] }
						}
					}
				};
			}

			// 12) 2 vertexes, 4 arcs
			{
				GraphBuilder builder = new();
				builder.AddArcAndVertices(0, 1);
				builder.AddArcAndVertices(0, 1);
				builder.AddArcAndVertices(1, 0);
				builder.AddArcAndVertices(1, 0);

				yield return new object[] {
					builder.Build(),
					new ExpectedGraph {
						VertexCount = 2,
						ArcCount = 4,
						AverageVertexDegree = 4f,
						DuplicateArcCount = 2,
						ReflexiveArcsCount = 0,
						IsAcyclic = false,
						VerticesAndArcs = new()
						{
							{ 0, [1, 1] },
							{ 1, [0, 0] }
						}
					}
				};
			}

			// 13) 2 vertexes, 1 arc, 1 reflexive arc
			{
				GraphBuilder builder = new();
				builder.AddArcAndVertices(0, 1);
				builder.AddArcAndVertices(1, 1);

				yield return new object[] {
					builder.Build(),
					new ExpectedGraph {
						VertexCount = 2,
						ArcCount = 2,
						AverageVertexDegree = 2f,
						DuplicateArcCount = 0,
						ReflexiveArcsCount = 1,
						IsAcyclic = false,
						VerticesAndArcs = new()
						{
							{ 0, [1] },
							{ 1, [1] }
						}
					}
				};
			}

			// 14) 2 vertexes, 2 arcs, 2 reflexive arcs
			{
				GraphBuilder builder = new();
				builder.AddArcAndVertices(0, 0);
				builder.AddArcAndVertices(0, 1);
				builder.AddArcAndVertices(1, 1);
				builder.AddArcAndVertices(1, 0);

				yield return new object[] {
					builder.Build(),
					new ExpectedGraph {
						VertexCount = 2,
						ArcCount = 4,
						AverageVertexDegree = 4f,
						DuplicateArcCount = 0,
						ReflexiveArcsCount = 2,
						IsAcyclic = false,
						VerticesAndArcs = new()
						{
							{ 0, [0, 1] },
							{ 1, [0, 1] }
						}
					}
				};
			}

			// 15) 2 vertexes, 4 arcs, 4 reflexive arcs
			{
				GraphBuilder builder = new();
				builder.AddArcAndVertices(0, 0); builder.AddArcAndVertices(0, 0);
				builder.AddArcAndVertices(1, 1); builder.AddArcAndVertices(1, 1);
				builder.AddArcAndVertices(0, 1); builder.AddArcAndVertices(0, 1);
				builder.AddArcAndVertices(1, 0); builder.AddArcAndVertices(1, 0);

				yield return new object[] {
					builder.Build(),
					new ExpectedGraph {
						VertexCount = 2,
						ArcCount = 8,
						AverageVertexDegree = 8f,
						DuplicateArcCount = 4,
						ReflexiveArcsCount = 4,
						IsAcyclic = false,
						VerticesAndArcs = new()
						{
							{ 0, [0, 0, 1, 1] },
							{ 1, [0, 0, 1, 1] }
						}
					}
				};
			}

			// 16) 5 vertexes, 9 arcs, 2 reflexive arcs
			{
				GraphBuilder builder = new();

				// cycle: 0 -> 2 -> 4 -> 0
				builder.AddArcAndVertices(0, 2);
				builder.AddArcAndVertices(2, 4);
				builder.AddArcAndVertices(4, 0);

				// ordinal arcs
				builder.AddArcAndVertices(0, 1);
				builder.AddArcAndVertices(2, 1);
				builder.AddArcAndVertices(2, 3);
				builder.AddArcAndVertices(4, 3);

				// reflexive arcs
				builder.AddArcAndVertices(2, 2);
				builder.AddArcAndVertices(3, 3);

				yield return new object[] {
					builder.Build(),
					new ExpectedGraph {
						VertexCount = 5,
						ArcCount = 9,
						AverageVertexDegree = 3.6f,
						DuplicateArcCount = 0,
						ReflexiveArcsCount = 2,
						IsAcyclic = false,
						VerticesAndArcs = new()
						{
							{ 0, [1, 2] },
							{ 1, [] },
							{ 2, [1, 2, 3, 4] },
							{ 3, [3] },
							{ 4, [0, 3] }
						}
					}
				};
			}
		}

		public static IEnumerable<object[]> GetRandomGraphsAndExpected ()
		{
			// 17)
			yield return GenerateRandomGraphAndExpected(1, 1);
			// 18)
			yield return GenerateRandomGraphAndExpected(1, 10);
			// 19)
			yield return GenerateRandomGraphAndExpected(1, 100);
			// 20)
			yield return GenerateRandomGraphAndExpected(1, 1_000);
			// 21)
			yield return GenerateRandomGraphAndExpected(10, 1);
			// 22)
			yield return GenerateRandomGraphAndExpected(10, 10);
			// 23)
			yield return GenerateRandomGraphAndExpected(10, 100);
			// 24)
			yield return GenerateRandomGraphAndExpected(10, 1_000);
			// 25)
			yield return GenerateRandomGraphAndExpected(100, 1);
			// 26)
			yield return GenerateRandomGraphAndExpected(100, 10);
			// 27)
			yield return GenerateRandomGraphAndExpected(100, 100);
			// 28)
			yield return GenerateRandomGraphAndExpected(100, 1_000);
			// 29)
			yield return GenerateRandomGraphAndExpected(1_000, 1);
			// 30)
			yield return GenerateRandomGraphAndExpected(1_000, 10);
			// 31)
			yield return GenerateRandomGraphAndExpected(1_000, 100);
			// 32)
			yield return GenerateRandomGraphAndExpected(1_000, 1_000);
		}

		private static object[] GenerateRandomGraphAndExpected (int arcCount, int vertexCount)
		{
			GraphBuilder builder = new();
			Dictionary<int, List<int>> naive = new(vertexCount);
			Random r = new(42);

			int reflexive = 0, duplicate = 0;

			for ( int i = 0; i < arcCount; i++ )
			{
				int from = r.Next(0, vertexCount), to = r.Next(0, vertexCount);

				if ( from == to )
				{
					reflexive++;
				}

				builder.AddArcAndVertices(from, to);

				if ( naive.TryGetValue(from, out List<int> value) )
				{
					if ( value.Contains(to) )
					{
						duplicate++;
					}
				}
				else
				{
					value = [];
					naive.Add(from, value);
				}

				if ( !naive.ContainsKey(to) )
				{
					naive.Add(to, []);
				}

				value.Add(to);
			}

			Dictionary<int, int[]> VerticesAndArcs = new();
			foreach ( int vertex in naive.Keys )
			{
				int[] arr = naive[vertex].ToArray();
				arr.Sort();
				VerticesAndArcs.Add(vertex, arr);
			}

			return
			[
				builder.Build(),
				new ExpectedGraph
				{
					ArcCount = arcCount,
					VertexCount = naive.Count,
					AverageVertexDegree = 2f * arcCount / naive.Count,
					DuplicateArcCount = duplicate,
					ReflexiveArcsCount = reflexive,
					IsAcyclic = !NaiveGraphExtensions.RunNaiveCycleDetection(naive),
					VerticesAndArcs = VerticesAndArcs
				}
			];
		}
	}

	public class ExpectedGraph
	{
		public int ArcCount { get; set; }
		public int VertexCount { get; set; }
		public float AverageVertexDegree { get; set; }
		public int DuplicateArcCount { get; set; }
		public bool IsAcyclic { get; set; }
		public int ReflexiveArcsCount { get; set; }
		public Dictionary<int, int[]> VerticesAndArcs { get; set; }
	}

	public class ExpectedGraphWithData : ExpectedGraph
	{
		public int[] ValueData { get; set; }
		public string[] ReferenceData { get; set; }
	}

	public class ExpectedGraphWithWeightsAndData : ExpectedGraph
	{
		public int[][] ValueWeights { get; set; }
		public string[][] ReferenceWeights { get; set; }
		public int[] ValueData { get; set; }
		public string[] ReferenceData { get; set; }
	}
}
