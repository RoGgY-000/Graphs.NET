using Graphs.NET.Core;
using Tests.Data;

namespace Tests.Graphs.NET.Core
{
	public class GraphBuilderTests
	{
		[Theory]
		[MemberData(nameof(GraphBuilderTestData.GetGraphBuildersAndExpected), MemberType = typeof(GraphBuilderTestData))]
		[MemberData(nameof(GraphBuilderTestData.GetRandomGraphBuildersAndExpected), MemberType = typeof(GraphBuilderTestData))]
		public void ArcCapacity (GraphBuilder builder, ExpectedGraphBuilder expected)
		{
			Assert.Equal(expected.ArcCapacity, builder.ArcCapacity);
		}

		[Theory]
		[MemberData(nameof(GraphBuilderTestData.GetGraphBuildersAndExpected), MemberType = typeof(GraphBuilderTestData))]
		[MemberData(nameof(GraphBuilderTestData.GetRandomGraphBuildersAndExpected), MemberType = typeof(GraphBuilderTestData))]
		public void ArcCount (GraphBuilder builder, ExpectedGraphBuilder expected)
		{
			Assert.Equal(expected.ArcCount, builder.ArcCount);
		}

		[Theory]
		[MemberData(nameof(GraphBuilderTestData.GetGraphBuildersAndExpected), MemberType = typeof(GraphBuilderTestData))]
		[MemberData(nameof(GraphBuilderTestData.GetRandomGraphBuildersAndExpected), MemberType = typeof(GraphBuilderTestData))]
		public void VertexCapacity (GraphBuilder builder, ExpectedGraphBuilder expected)
		{
			Assert.Equal(expected.VertexCapacity, builder.VertexCapacity);
		}

		[Theory]
		[MemberData(nameof(GraphBuilderTestData.GetGraphBuildersAndExpected), MemberType = typeof(GraphBuilderTestData))]
		[MemberData(nameof(GraphBuilderTestData.GetRandomGraphBuildersAndExpected), MemberType = typeof(GraphBuilderTestData))]
		public void VertexCount (GraphBuilder builder, ExpectedGraphBuilder expected)
		{
			Assert.Equal(expected.VertexCount, builder.VertexCount);
		}

		[Theory]
		[MemberData(nameof(GraphBuilderTestData.GetGraphBuildersAndExpected), MemberType = typeof(GraphBuilderTestData))]
		[MemberData(nameof(GraphBuilderTestData.GetRandomGraphBuildersAndExpected), MemberType = typeof(GraphBuilderTestData))]
		public void AddArc (GraphBuilder builder, ExpectedGraphBuilder expected)
		{
			int[] expectedVertices = expected.VerticesAndArcs.Keys.ToArray();

			for ( int vertexIndex = 0; vertexIndex < expectedVertices.Length; vertexIndex++ )
			{
				int vertex = expectedVertices[vertexIndex];

				for ( int otherIndex = 0; otherIndex < expectedVertices.Length; otherIndex++ )
				{
					int other = expectedVertices[otherIndex];

					if ( !expected.VerticesAndArcs[vertex].Contains(other) )
					{
						builder.AddArc(vertex, other);
						Assert.True(builder.HasArc(vertex, other));
						break;
					}
				}
			}
		}

		[Theory]
		[MemberData(nameof(GraphBuilderTestData.GetGraphBuildersAndExpected), MemberType = typeof(GraphBuilderTestData))]
		[MemberData(nameof(GraphBuilderTestData.GetRandomGraphBuildersAndExpected), MemberType = typeof(GraphBuilderTestData))]
		public void AddArcAndVertices (GraphBuilder builder, ExpectedGraphBuilder expected)
		{
			int[] expectedVertices = expected.VerticesAndArcs.Keys.ToArray();
			int firstNewVertex = expectedVertices.Length > 0 ? expectedVertices.Max() + 1 : 0;

			for ( int vertexIndex = 0; vertexIndex < expectedVertices.Length; vertexIndex++ )
			{
				int vertex = expectedVertices[vertexIndex];
				int currentNewVertex = firstNewVertex + vertexIndex;

				builder.AddArcAndVertices(vertex, currentNewVertex);

				Assert.True(builder.HasArc(vertex, currentNewVertex));
			}
		}

		[Theory]
		[MemberData(nameof(GraphBuilderTestData.GetGraphBuildersAndExpected), MemberType = typeof(GraphBuilderTestData))]
		[MemberData(nameof(GraphBuilderTestData.GetRandomGraphBuildersAndExpected), MemberType = typeof(GraphBuilderTestData))]
		public void AddEdge (GraphBuilder builder, ExpectedGraphBuilder expected)
		{
			int[] expectedVertices = expected.VerticesAndArcs.Keys.ToArray();

			for ( int vertexIndex = 0; vertexIndex < expectedVertices.Length; vertexIndex++ )
			{
				int vertex = expectedVertices[vertexIndex];

				for ( int otherIndex = 0; otherIndex < expectedVertices.Length; otherIndex++ )
				{
					int other = expectedVertices[otherIndex];

					if ( !expected.VerticesAndArcs[vertex].Contains(other)
						&& expected.VerticesAndArcs[other].Contains(vertex) )
					{
						builder.AddEdge(vertex, other);
						Assert.True(builder.HasArc(vertex, other));
						Assert.True(builder.HasArc(other, vertex));
					}
				}
			}
		}

		[Theory]
		[MemberData(nameof(GraphBuilderTestData.GetGraphBuildersAndExpected), MemberType = typeof(GraphBuilderTestData))]
		[MemberData(nameof(GraphBuilderTestData.GetRandomGraphBuildersAndExpected), MemberType = typeof(GraphBuilderTestData))]
		public void AddEdgeAndVertices (GraphBuilder builder, ExpectedGraphBuilder expected)
		{
			int[] expectedVertices = expected.VerticesAndArcs.Keys.ToArray();
			int firstNewVertex = expectedVertices.Length > 0 ? expectedVertices.Max() + 1 : 0;

			for ( int vertexIndex = 0; vertexIndex < expectedVertices.Length; vertexIndex++ )
			{
				int vertex = expectedVertices[vertexIndex];
				int currentNewVertex = firstNewVertex + vertexIndex;

				builder.AddEdgeAndVertices(vertex, currentNewVertex);

				Assert.True(builder.HasArc(vertex, currentNewVertex));
				Assert.True(builder.HasArc(currentNewVertex, vertex));
			}
		}

		[Theory]
		[MemberData(nameof(GraphBuilderTestData.GetGraphBuildersAndExpected), MemberType = typeof(GraphBuilderTestData))]
		[MemberData(nameof(GraphBuilderTestData.GetRandomGraphBuildersAndExpected), MemberType = typeof(GraphBuilderTestData))]
		public void AddVertex (GraphBuilder builder, ExpectedGraphBuilder expected)
		{
			int[] expectedVertices = expected.VerticesAndArcs.Keys.ToArray();
			int maxVertex = expectedVertices.Length > 0 ? expectedVertices.Max() : 0;

			for ( int vertex = 0; vertex < maxVertex + 1; vertex++ )
			{
				if ( expected.VerticesAndArcs.TryGetValue(vertex, out int[]? neighbors)
					&& neighbors == null )
				{
					Assert.False(builder.HasVertex(vertex));

					builder.AddVertex(vertex);

					Assert.True(builder.HasVertex(vertex));
				}
			}
		}

		[Theory]
		[MemberData(nameof(GraphBuilderTestData.GetGraphBuildersAndExpected), MemberType = typeof(GraphBuilderTestData))]
		[MemberData(nameof(GraphBuilderTestData.GetRandomGraphBuildersAndExpected), MemberType = typeof(GraphBuilderTestData))]
		public void AddVertices (GraphBuilder builder, ExpectedGraphBuilder expected)
		{
			int[] expectedVertices = expected.VerticesAndArcs.Keys.ToArray();

			List<int> unExistingVertices = [];

			for ( int vertex = 0; vertex < expectedVertices.Length + 1; vertex++ )
			{
				if ( expected.VerticesAndArcs.TryGetValue(vertex, out int[]? neighbors)
					&& neighbors == null )
				{
					Assert.False(builder.HasVertex(vertex));

					unExistingVertices.Add(vertex);
				}
			}

			builder.AddVertices(unExistingVertices.ToArray().AsSpan());

			foreach ( int newVertex in unExistingVertices )
			{
				Assert.True(builder.HasVertex(newVertex));
			}
		}

		[Theory]
		[MemberData(nameof(GraphBuilderTestData.GetGraphBuildersAndExpected), MemberType = typeof(GraphBuilderTestData))]
		[MemberData(nameof(GraphBuilderTestData.GetRandomGraphBuildersAndExpected), MemberType = typeof(GraphBuilderTestData))]
		public void HasArc (GraphBuilder builder, ExpectedGraphBuilder expected)
		{
			int[] expectedVertices = expected.VerticesAndArcs.Keys.ToArray();

			foreach ( int vertex in expectedVertices )
			{
				foreach ( int neighbor in expected.VerticesAndArcs[vertex] )
				{
					Assert.True(builder.HasArc(vertex, neighbor));
				}
			}
		}

		[Theory]
		[MemberData(nameof(GraphBuilderTestData.GetGraphBuildersAndExpected), MemberType = typeof(GraphBuilderTestData))]
		[MemberData(nameof(GraphBuilderTestData.GetRandomGraphBuildersAndExpected), MemberType = typeof(GraphBuilderTestData))]
		public void HasVertex (GraphBuilder builder, ExpectedGraphBuilder expected)
		{
			int[] expectedVertices = expected.VerticesAndArcs.Keys.ToArray();

			foreach ( int vertex in expectedVertices )
			{
				Assert.True(builder.HasVertex(vertex));
			}
		}

		[Theory]
		[MemberData(nameof(GraphBuilderTestData.GetGraphBuildersAndExpected), MemberType = typeof(GraphBuilderTestData))]
		[MemberData(nameof(GraphBuilderTestData.GetRandomGraphBuildersAndExpected), MemberType = typeof(GraphBuilderTestData))]
		public void RemoveArc (GraphBuilder builder, ExpectedGraphBuilder expected)
		{
			int[] vertices = expected.VerticesAndArcs.Keys.ToArray();

			int removedArcs = 0;

			foreach ( int vertex in vertices )
			{
				if ( expected.VerticesAndArcs.TryGetValue(vertex, out int[]? neighbors)
					&& neighbors != null )
				{
					Dictionary<int, int> counts = neighbors.GroupBy(n => n).ToDictionary(g => g.Key, g => g.Count());

					foreach ( int neighbor in neighbors )
					{
						Assert.True(builder.HasArc(vertex, neighbor));

						builder.RemoveArc(vertex, neighbor);
						removedArcs++;

						if ( --counts[neighbor] == 0 )
						{
							Assert.False(builder.HasArc(vertex, neighbor));
						}

						Assert.Equal(expected.ArcCount - removedArcs, builder.ArcCount);
					}
				}
			}

			foreach ( int vertex in vertices )
			{
				if ( expected.VerticesAndArcs.TryGetValue(vertex, out int[]? neighbors)
					&& neighbors != null )
				{
					foreach ( int neighbor in neighbors )
					{
						Assert.False(builder.HasArc(vertex, neighbor));
					}
				}
			}
		}

		[Theory]
		[MemberData(nameof(GraphBuilderTestData.GetGraphBuildersAndExpected), MemberType = typeof(GraphBuilderTestData))]
		[MemberData(nameof(GraphBuilderTestData.GetRandomGraphBuildersAndExpected), MemberType = typeof(GraphBuilderTestData))]
		public void RemoveVertex (GraphBuilder builder, ExpectedGraphBuilder expected)
		{
			int[] vertices = expected.VerticesAndArcs.Keys.ToArray();

			int removedVertices = 0;

			foreach ( int vertex in vertices )
			{
				builder.RemoveVertex(vertex);
				removedVertices++;

				Assert.False(builder.HasVertex(vertex));

				if ( expected.VerticesAndArcs.TryGetValue(vertex, out int[]? neighbors)
					&& neighbors != null )
				{
					foreach ( int neighbor in neighbors )
					{
						Assert.False(builder.HasArc(vertex, neighbor));
					}
				}
			}

			Assert.Equal(0, expected.VertexCount - removedVertices);
			Assert.Equal(0, builder.VertexCount);
			Assert.Equal(0, builder.ArcCount);

			foreach ( int vertex in vertices )
			{
				if ( expected.VerticesAndArcs.TryGetValue(vertex, out int[]? neighbors)
					&& neighbors != null )
				{
					Assert.False(builder.HasVertex(vertex));

					foreach ( int neighbor in neighbors )
					{
						Assert.False(builder.HasArc(vertex, neighbor));
					}
				}
			}
		}
	}
}
