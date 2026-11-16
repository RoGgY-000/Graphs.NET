using Graphs.NET.Core;
using Tests.Data;

namespace Tests.Graphs.NET.Core
{
	public class GraphBuilderWithWeightsTests : GraphBuilderTests
	{
		[Theory]
		[MemberData(nameof(GraphBuilderWithWeightTestData.GetGraphBuildersAndExpected), MemberType = typeof(GraphBuilderWithWeightTestData))]
		[MemberData(nameof(GraphBuilderWithWeightTestData.GetRandomGraphBuildersWithWeightAndExpected), MemberType = typeof(GraphBuilderWithWeightTestData))]
		public void AddArcWithWeight (GraphBuilder<string> builder, ExpectedGraphBuilder<string> expected)
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
						builder.AddArc(vertex, other, $"{vertex}->{other}");
						Assert.Equal($"{vertex}->{other}", builder.GetWeight(vertex, other));
						break;
					}
				}
			}
		}

		[Theory]
		[MemberData(nameof(GraphBuilderWithWeightTestData.GetGraphBuildersAndExpected), MemberType = typeof(GraphBuilderWithWeightTestData))]
		[MemberData(nameof(GraphBuilderWithWeightTestData.GetRandomGraphBuildersWithWeightAndExpected), MemberType = typeof(GraphBuilderWithWeightTestData))]
		public void AddArcAndVerticesWithWeight (GraphBuilder<string> builder, ExpectedGraphBuilder<string> expected)
		{
			int[] expectedVertices = expected.VerticesAndArcs.Keys.ToArray();
			int firstNewVertex = expectedVertices.Length > 0 ? expectedVertices.Max() + 1 : 0;

			for ( int vertexIndex = 0; vertexIndex < expectedVertices.Length; vertexIndex++ )
			{
				int vertex = expectedVertices[vertexIndex];
				int currentNewVertex = firstNewVertex + vertexIndex;

				builder.AddArcAndVertices(vertex, currentNewVertex, $"{vertex}->{currentNewVertex}");
				Assert.Equal($"{vertex}->{currentNewVertex}", builder.GetWeight(vertex, currentNewVertex));
			}
		}

		[Theory]
		[MemberData(nameof(GraphBuilderWithWeightTestData.GetGraphBuildersAndExpected), MemberType = typeof(GraphBuilderWithWeightTestData))]
		[MemberData(nameof(GraphBuilderWithWeightTestData.GetRandomGraphBuildersWithWeightAndExpected), MemberType = typeof(GraphBuilderWithWeightTestData))]
		public void AddEdgeWithWeight (GraphBuilder<string> builder, ExpectedGraphBuilder<string> expected)
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
						builder.AddEdge(vertex, other, $"{vertex}->{other}");
						Assert.Equal($"{vertex}->{other}", builder.GetWeight(vertex, other));
						Assert.Equal($"{vertex}->{other}", builder.GetWeight(other, vertex));
					}
				}
			}
		}

		[Theory]
		[MemberData(nameof(GraphBuilderWithWeightTestData.GetGraphBuildersAndExpected), MemberType = typeof(GraphBuilderWithWeightTestData))]
		[MemberData(nameof(GraphBuilderWithWeightTestData.GetRandomGraphBuildersWithWeightAndExpected), MemberType = typeof(GraphBuilderWithWeightTestData))]
		public void AddEdgeAndVerticesWithWeight (GraphBuilder<string> builder, ExpectedGraphBuilder<string> expected)
		{
			int[] expectedVertices = expected.VerticesAndArcs.Keys.ToArray();
			int firstNewVertex = expectedVertices.Length > 0 ? expectedVertices.Max() + 1 : 0;

			for ( int vertexIndex = 0; vertexIndex < expectedVertices.Length; vertexIndex++ )
			{
				int vertex = expectedVertices[vertexIndex];
				int currentNewVertex = firstNewVertex + vertexIndex;

				builder.AddEdgeAndVertices(vertex, currentNewVertex, $"{vertex}->{currentNewVertex}");
				Assert.Equal($"{vertex}->{currentNewVertex}", builder.GetWeight(vertex, currentNewVertex));
				Assert.Equal($"{vertex}->{currentNewVertex}", builder.GetWeight(currentNewVertex, vertex));
			}
		}

		[Theory]
		[MemberData(nameof(GraphBuilderWithWeightTestData.GetGraphBuildersAndExpected), MemberType = typeof(GraphBuilderWithWeightTestData))]
		[MemberData(nameof(GraphBuilderWithWeightTestData.GetRandomGraphBuildersWithWeightAndExpected), MemberType = typeof(GraphBuilderWithWeightTestData))]
		public void GetWeights (GraphBuilder<string> builder, ExpectedGraphBuilder<string> expected)
		{
			int[] expectedVertices = expected.VerticesAndArcs.Keys.ToArray();

			foreach (int vertex in  expectedVertices) 
			{
				foreach ( int neighbor in expected.Weights.Keys )
				{
					foreach ( string weight in expected.Weights[vertex][neighbor] )
					{
						Assert.Equal(weight, builder.GetWeight(vertex, neighbor));
					}
				}
			}
		}
	}
}
