using Graphs.NET.Core;

namespace Tests.Data
{
	public class GraphTestData
	{

		public static IEnumerable<object[]> GetGraphsAndExpectedWithoutWeightsAndData ()
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
						Neighbors = []
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
						Neighbors = [[]]
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
						Neighbors = [[0]]
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
						Neighbors = [[0, 0]]
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
						Neighbors = [[], []]
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
						Neighbors = [[], [1]]
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
						Neighbors = [[0], [1]]
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
						Neighbors = [[0, 0], [1]]
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
						Neighbors = [[1], []]
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
						Neighbors = [[1], [0]]
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
						Neighbors = [[1, 1], [0]]
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
						Neighbors = [[1, 1], [0, 0]]
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
						Neighbors = [[1], [1]]
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
						Neighbors = [[0, 1], [0, 1]]
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
						Neighbors = [[0, 0, 1, 1], [0, 0, 1, 1]]
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
						Neighbors = [[1, 2], [], [1, 2, 3, 4], [3], [0, 3]]
					}
				};
			}
		}
	}

	public class ExpectedGraph
	{
		public int VertexCount { get; set; }
		public int ArcCount { get; set; }
		public float AverageVertexDegree { get; set; }
		public int DuplicateArcCount { get; set; }
		public int ReflexiveArcsCount { get; set; }
		public bool IsAcyclic { get; set; }
		public int[][] Neighbors { get; set; }
	}

	public class ExpectedGraphWithWeights : ExpectedGraph
	{
		public int[][] ValueWeights { get; set; }
		public string[][] ReferenceWeights { get; set; }
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
