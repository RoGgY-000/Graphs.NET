using System.Numerics;
using Graphs.NET.Core;

namespace Tests.Data
{
	public static class GraphBuilderTestData
	{
		public static IEnumerable<object[]> GetGraphBuildersAndExpected ()
		{
			// 1) empty graph
			{
				GraphBuilder builder = new();

				yield return new object[] {
					builder,
					new ExpectedGraphBuilder {
						ArcCapacity = 0,
						ArcCount = 0,
						VertexCapacity = 0,
						VertexCount = 0,
						VerticesAndArcs = []
					}
				};
			}

			// 2) 1 vertex, 0 arcs
			{
				GraphBuilder builder = new();
				builder.AddVertex(0);

				yield return new object[] {
					builder,
					new ExpectedGraphBuilder {
						ArcCapacity = 0,
						ArcCount = 0,
						VertexCapacity = 1,
						VertexCount = 1,
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
				builder.AddVertex(0);
				builder.AddArc(0, 0);

				yield return new object[] {
					builder,
					new ExpectedGraphBuilder {
						ArcCapacity = 1,
						ArcCount = 1,
						VertexCapacity = 1,
						VertexCount = 1,
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
					builder,
					new ExpectedGraphBuilder {
						ArcCapacity = 2,
						ArcCount = 2,
						VertexCapacity = 1,
						VertexCount = 1,
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
					builder,
					new ExpectedGraphBuilder {
						ArcCapacity = 0,
						ArcCount = 0,
						VertexCapacity = 2,
						VertexCount = 2,
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
					builder,
					new ExpectedGraphBuilder {
						ArcCapacity = 1,
						ArcCount = 1,
						VertexCapacity = 2,
						VertexCount = 2,
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
					builder,
					new ExpectedGraphBuilder {
						ArcCapacity = 2,
						ArcCount = 2,
						VertexCapacity = 2,
						VertexCount = 2,
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
					builder,
					new ExpectedGraphBuilder {
						ArcCapacity = 4,
						ArcCount = 3,
						VertexCapacity = 2,
						VertexCount = 2,
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
					builder,
					new ExpectedGraphBuilder {
						ArcCapacity = 1,
						ArcCount = 1,
						VertexCapacity = 2,
						VertexCount = 2,
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
					builder,
					new ExpectedGraphBuilder {
						ArcCapacity = 2,
						ArcCount = 2,
						VertexCapacity = 2,
						VertexCount = 2,
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
					builder,
					new ExpectedGraphBuilder {
						ArcCapacity = 4,
						ArcCount = 3,
						VertexCapacity = 2,
						VertexCount = 2,
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
					builder,
					new ExpectedGraphBuilder {
						ArcCapacity = 4,
						ArcCount = 4,
						VertexCapacity = 2,
						VertexCount = 2,
						VerticesAndArcs = new()
						{
							{ 0, [1, 1] },
							{ 1, [0, 0] }
						}
					}
				};
			}

			// 13) 2 vertexes, 2 arcs, 1 reflexive arc
			{
				GraphBuilder builder = new();
				builder.AddArcAndVertices(0, 1);
				builder.AddArcAndVertices(1, 1);

				yield return new object[] {
					builder,
					new ExpectedGraphBuilder {
						ArcCapacity = 2,
						ArcCount = 2,
						VertexCapacity = 2,
						VertexCount = 2,
						VerticesAndArcs = new()
						{
							{ 0, [1] },
							{ 1, [1] }
						}
					}
				};
			}

			// 14) 2 vertexes, 4 arcs, 2 reflexive arcs
			{
				GraphBuilder builder = new();
				builder.AddArcAndVertices(0, 0);
				builder.AddArcAndVertices(0, 1);
				builder.AddArcAndVertices(1, 0);
				builder.AddArcAndVertices(1, 1);

				yield return new object[] {
					builder,
					new ExpectedGraphBuilder {
						ArcCapacity = 4,
						ArcCount = 4,
						VertexCapacity = 2,
						VertexCount = 2,
						VerticesAndArcs = new()
						{
							{ 0, [0, 1] },
							{ 1, [0, 1] }
						}
					}
				};
			}

			// 15) 2 vertexes, 8 arcs, 4 reflexive arcs
			{
				GraphBuilder builder = new();
				builder.AddArcAndVertices(0, 0); builder.AddArcAndVertices(0, 0);
				builder.AddArcAndVertices(0, 1); builder.AddArcAndVertices(0, 1);
				builder.AddArcAndVertices(1, 0); builder.AddArcAndVertices(1, 0);
				builder.AddArcAndVertices(1, 1); builder.AddArcAndVertices(1, 1);

				yield return new object[] {
					builder,
					new ExpectedGraphBuilder {
						ArcCapacity = 8,
						ArcCount = 8,
						VertexCapacity = 2,
						VertexCount = 2,
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
					builder,
					new ExpectedGraphBuilder {
						ArcCapacity = 16,
						ArcCount = 9,
						VertexCapacity = 8,
						VertexCount = 5,
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

		public static IEnumerable<object[]> GetRandomGraphBuildersAndExpected ()
		{
			// 17)
			yield return GenerateRandomGraphBuilderAndExpected(1, 1);
			// 18)
			yield return GenerateRandomGraphBuilderAndExpected(1, 10);
			// 19)
			yield return GenerateRandomGraphBuilderAndExpected(1, 100);
			// 20)
			yield return GenerateRandomGraphBuilderAndExpected(1, 1_000);
			// 21)
			yield return GenerateRandomGraphBuilderAndExpected(10, 1);
			// 22)
			yield return GenerateRandomGraphBuilderAndExpected(10, 10);
			// 23)
			yield return GenerateRandomGraphBuilderAndExpected(10, 100);
			// 24)
			yield return GenerateRandomGraphBuilderAndExpected(10, 1_000);
			// 25)
			yield return GenerateRandomGraphBuilderAndExpected(100, 1);
			// 26)
			yield return GenerateRandomGraphBuilderAndExpected(100, 10);
			// 27)
			yield return GenerateRandomGraphBuilderAndExpected(100, 100);
			// 28)
			yield return GenerateRandomGraphBuilderAndExpected(100, 1_000);
			// 29)
			yield return GenerateRandomGraphBuilderAndExpected(1_000, 1);
			// 30)
			yield return GenerateRandomGraphBuilderAndExpected(1_000, 10);
			// 31)
			yield return GenerateRandomGraphBuilderAndExpected(1_000, 100);
			// 32)
			yield return GenerateRandomGraphBuilderAndExpected(1_000, 1_000);
		}

		private static object[] GenerateRandomGraphBuilderAndExpected (int ArcCount, int vertexCount)
		{
			GraphBuilder builder = new();
			Dictionary<int, List<int>> naive = new(vertexCount);
			Random r = new(42);

			int reflexive = 0, duplicate = 0;

			for ( int i = 0; i < ArcCount; i++ )
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

			Dictionary<int, int[]> VertiсesAndArcs = new();
			foreach ( int vertex in naive.Keys )
			{
				int[] arr = naive[vertex].ToArray();
				arr.Sort();
				VertiсesAndArcs.Add(vertex, arr);
			}

			return
			[
				builder,
				new ExpectedGraphBuilder
				{
					ArcCapacity = (int) BitOperations.RoundUpToPowerOf2( (uint) ArcCount),
					ArcCount = ArcCount,
					VertexCapacity = (int) BitOperations.RoundUpToPowerOf2( (uint) naive.Keys.Max() + 1),
					VertexCount = naive.Count,
					VerticesAndArcs = VertiсesAndArcs
				}
			];
		}
	}

	public class ExpectedGraphBuilder
	{
		public int ArcCapacity { get; set; }
		public int ArcCount { get; set; }
		public int VertexCapacity { get; set; }
		public int VertexCount { get; set; }
		public Dictionary<int, int[]> VerticesAndArcs { get; set; }
	}
}
