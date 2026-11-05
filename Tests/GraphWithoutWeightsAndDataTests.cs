using Graphs.NET.Core;
using Tests;

namespace GraphTests
{
	public class GraphWithoutWeightsAndDataTests
	{
		[Fact]
		public void EmptyGraphEqualityTests ()
		{
			Graph empty1 = Graph<Nothing, Nothing>.GetEmpty();
			Graph empty2 = Graph<Nothing, Nothing>.GetEmpty();

			ReferenceEquals(empty1, empty2);

			Graph empty3 = Graph<int, Nothing>.GetEmpty();
			Graph empty4 = Graph<int, Nothing>.GetEmpty();

			ReferenceEquals(empty3, empty4);

			Graph empty5 = Graph<Nothing, string>.GetEmpty();
			Graph empty6 = Graph<Nothing, string>.GetEmpty();

			ReferenceEquals(empty5, empty6);

			Graph empty7 = Graph<string, int>.GetEmpty();
			Graph empty8 = Graph<string, int>.GetEmpty();

			ReferenceEquals(empty7, empty8);
		}

		[Theory]
		[MemberData(nameof(GraphTestData.GetGraphsAndExpectedWithoutWeightsAndData), MemberType = typeof(GraphTestData))]
		public void VertexCountTests (Graph graph, ExpectedGraph expected)
		{
			Assert.Equal(expected.VertexCount, graph.VertexCount);
		}

		[Theory]
		[MemberData(nameof(GraphTestData.GetGraphsAndExpectedWithoutWeightsAndData), MemberType = typeof(GraphTestData))]
		public void ArcCountTests (Graph graph, ExpectedGraph expected)
		{
			Assert.Equal(expected.ArcCount, graph.ArcCount);
		}

		[Theory]
		[MemberData(nameof(GraphTestData.GetGraphsAndExpectedWithoutWeightsAndData), MemberType = typeof(GraphTestData))]
		public void AverageVertexDegreeTests (Graph graph, ExpectedGraph expected)
		{
			Assert.Equal(expected.AverageVertexDegree, graph.AverageVertexDegree);
		}

		[Theory]
		[MemberData(nameof(GraphTestData.GetGraphsAndExpectedWithoutWeightsAndData), MemberType = typeof(GraphTestData))]
		public void DuplicateArcCountTests (Graph graph, ExpectedGraph expected)
		{
			Assert.Equal(expected.DuplicateArcCount, graph.DuplicateArcCount);
		}

		[Theory]
		[MemberData(nameof(GraphTestData.GetGraphsAndExpectedWithoutWeightsAndData), MemberType = typeof(GraphTestData))]
		public void ReflexiveVertexCountTests (Graph graph, ExpectedGraph expected)
		{
			Assert.Equal(expected.ReflexiveArcsCount, graph.ReflexiveArcsCount);
		}

		[Theory]
		[MemberData(nameof(GraphTestData.GetGraphsAndExpectedWithoutWeightsAndData), MemberType = typeof(GraphTestData))]
		public void IsAcyclicTests (Graph graph, ExpectedGraph expected)
		{
			Assert.Equal(expected.IsAcyclic, graph.IsAcyclic);
		}

		[Theory]
		[MemberData(nameof(GraphTestData.GetGraphsAndExpectedWithoutWeightsAndData), MemberType = typeof(GraphTestData))]
		public void GetNeighborsTests (Graph graph, ExpectedGraph expected)
		{
			Assert.Equal(expected.Neighbors.Length, graph.VertexCount);

			for ( int vertex = 0; vertex < expected.Neighbors.Length; vertex++ )
			{
				int[] expectedNeighbors = expected.Neighbors[vertex];
				ReadOnlySpan<int> graphNeighbors = graph.GetNeighbors(vertex);

				Assert.Equal(expectedNeighbors.Length, graphNeighbors.Length);

				for ( int i = 0; i < expectedNeighbors.Length; i++ )
				{
					Assert.Equal(expectedNeighbors[i], graphNeighbors[i]);
				}
			}
		}

		[Theory]
		[MemberData(nameof(GraphTestData.GetGraphsAndExpectedWithoutWeightsAndData), MemberType = typeof(GraphTestData))]
		public void GetNeighborThrowsTests (Graph graph, ExpectedGraph expected)
		{
			Assert.Throws<ArgumentOutOfRangeException>(() => graph.GetNeighbors(-1));
			Assert.Throws<ArgumentOutOfRangeException>(() => graph.GetNeighbors(expected.Neighbors.Length));
		}

		[Theory]
		[MemberData(nameof(GraphTestData.GetGraphsAndExpectedWithoutWeightsAndData), MemberType = typeof(GraphTestData))]
		public void HasArcTests (Graph graph, ExpectedGraph expected)
		{
			for ( int vertex = 0; vertex < expected.Neighbors.Length; vertex++ )
			{
				int[] expectedNeighbors = expected.Neighbors[vertex];
				if ( expectedNeighbors.Length > 0 )
				{
					ReadOnlySpan<int> graphNeighbors = graph.GetNeighbors(vertex);

					for ( int i = 0; i < expectedNeighbors.Length; i++ )
					{
						int expectedNeighbor = expectedNeighbors[i];
						Assert.True(graph.HasArc(vertex, expectedNeighbor));
					}
				}
			}
		}
	}
}
