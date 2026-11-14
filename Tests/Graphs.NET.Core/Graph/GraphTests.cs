using Graphs.NET.Core;
using Tests.Data;

namespace Tests.Graphs.NET.Core
{
	public class GraphTests
	{
		[Theory]
		[MemberData(nameof(GraphTestData.GetGraphsAndExpected), MemberType = typeof(GraphTestData))]
		[MemberData(nameof(GraphTestData.GetRandomGraphsAndExpected), MemberType = typeof(GraphTestData))]
		public void ArcCountTests (Graph graph, ExpectedGraph expected)
		{
			Assert.Equal(expected.ArcCount, graph.ArcCount);
		}

		[Theory]
		[MemberData(nameof(GraphTestData.GetGraphsAndExpected), MemberType = typeof(GraphTestData))]
		[MemberData(nameof(GraphTestData.GetRandomGraphsAndExpected), MemberType = typeof(GraphTestData))]
		public void VertexCountTests (Graph graph, ExpectedGraph expected)
		{
			Assert.Equal(expected.VertexCount, graph.VertexCount);
		}

		[Theory]
		[MemberData(nameof(GraphTestData.GetGraphsAndExpected), MemberType = typeof(GraphTestData))]
		[MemberData(nameof(GraphTestData.GetRandomGraphsAndExpected), MemberType = typeof(GraphTestData))]
		public void AverageVertexDegreeTests (Graph graph, ExpectedGraph expected)
		{
			Assert.Equal(expected.AverageVertexDegree, graph.AverageVertexDegree);
		}

		[Theory]
		[MemberData(nameof(GraphTestData.GetGraphsAndExpected), MemberType = typeof(GraphTestData))]
		[MemberData(nameof(GraphTestData.GetRandomGraphsAndExpected), MemberType = typeof(GraphTestData))]
		public void DuplicateArcCountTests (Graph graph, ExpectedGraph expected)
		{
			Assert.Equal(expected.DuplicateArcCount, graph.DuplicateArcCount);
		}

		[Theory]
		[MemberData(nameof(GraphTestData.GetGraphsAndExpected), MemberType = typeof(GraphTestData))]
		[MemberData(nameof(GraphTestData.GetRandomGraphsAndExpected), MemberType = typeof(GraphTestData))]
		public void IsAcyclicTests (Graph graph, ExpectedGraph expected)
		{
			Assert.Equal(expected.IsAcyclic, graph.IsAcyclic);
		}

		[Theory]
		[MemberData(nameof(GraphTestData.GetGraphsAndExpected), MemberType = typeof(GraphTestData))]
		[MemberData(nameof(GraphTestData.GetRandomGraphsAndExpected), MemberType = typeof(GraphTestData))]
		public void ReflexiveVertexCountTests (Graph graph, ExpectedGraph expected)
		{
			Assert.Equal(expected.ReflexiveArcsCount, graph.ReflexiveArcCount);
		}

		[Theory]
		[MemberData(nameof(GraphTestData.GetGraphsAndExpected), MemberType = typeof(GraphTestData))]
		[MemberData(nameof(GraphTestData.GetRandomGraphsAndExpected), MemberType = typeof(GraphTestData))]
		public void GetNeighborsTests (Graph graph, ExpectedGraph expected)
		{
			Assert.Equal(expected.VerticesAndArcs.Count, graph.VertexCount);

			foreach ( int vertex in expected.VerticesAndArcs.Keys )
			{
				int[] expectedNeighbors = expected.VerticesAndArcs[vertex];
				ReadOnlySpan<int> graphNeighbors = graph.GetNeighbors(vertex);

				Assert.Equal(expectedNeighbors.Length, graphNeighbors.Length);

				for ( int i = 0; i < expectedNeighbors.Length; i++ )
				{
					Assert.Equal(expectedNeighbors[i], graphNeighbors[i]);
				}
			}
		}

		[Theory]
		[MemberData(nameof(GraphTestData.GetGraphsAndExpected), MemberType = typeof(GraphTestData))]
		[MemberData(nameof(GraphTestData.GetRandomGraphsAndExpected), MemberType = typeof(GraphTestData))]
		public void GetNeighborThrowsTests (Graph graph, ExpectedGraph expected)
		{
			Assert.Throws<ArgumentOutOfRangeException>(() => graph.GetNeighbors(-1));
			Assert.Throws<ArgumentOutOfRangeException>(() => graph.GetNeighbors(int.MaxValue));
		}

		[Theory]
		[MemberData(nameof(GraphTestData.GetGraphsAndExpected), MemberType = typeof(GraphTestData))]
		[MemberData(nameof(GraphTestData.GetRandomGraphsAndExpected), MemberType = typeof(GraphTestData))]
		public void HasArcTests (Graph graph, ExpectedGraph expected)
		{
			foreach ( int vertex in expected.VerticesAndArcs.Keys )
			{
				int[] expectedNeighbors = expected.VerticesAndArcs[vertex];

				foreach ( int expectedNeighbor in expectedNeighbors )
				{
					Assert.True(graph.HasArc(vertex, expectedNeighbor));
				}

				foreach ( int otherVertex in expected.VerticesAndArcs.Keys )
				{
					if ( !expectedNeighbors.Contains(otherVertex) )
					{
						Assert.False(graph.HasArc(vertex, otherVertex));
					}
				}
			}
		}

		[Theory]
		[MemberData(nameof(GraphTestData.GetGraphsAndExpected), MemberType = typeof(GraphTestData))]
		[MemberData(nameof(GraphTestData.GetRandomGraphsAndExpected), MemberType = typeof(GraphTestData))]
		public void HasArcThrowsTests (Graph graph, ExpectedGraph expected)
		{
			Assert.Throws<ArgumentOutOfRangeException>(() => graph.HasArc(-1, 0));
			Assert.Throws<ArgumentOutOfRangeException>(() => graph.HasArc(0, -1));
			Assert.Throws<ArgumentOutOfRangeException>(() => graph.HasArc(int.MaxValue, 0));
			Assert.Throws<ArgumentOutOfRangeException>(() => graph.HasArc(0, int.MaxValue));

			foreach ( int vertex in expected.VerticesAndArcs.Keys )
			{
				Assert.Throws<ArgumentOutOfRangeException>(() => graph.HasArc(vertex, int.MaxValue));
				Assert.Throws<ArgumentOutOfRangeException>(() => graph.HasArc(vertex, int.MaxValue));
			}
		}

		[Theory]
		[MemberData(nameof(GraphTestData.GetGraphsAndExpected), MemberType = typeof(GraphTestData))]
		[MemberData(nameof(GraphTestData.GetRandomGraphsAndExpected), MemberType = typeof(GraphTestData))]
		public void HasVertexTests (Graph graph, ExpectedGraph expected)
		{
			foreach ( int expectedVertex in expected.VerticesAndArcs.Keys )
			{
				Assert.True(graph.HasVertex(expectedVertex));
				Assert.False(graph.HasVertex(int.MaxValue - expectedVertex));
			}
		}

		[Theory]
		[MemberData(nameof(GraphTestData.GetGraphsAndExpected), MemberType = typeof(GraphTestData))]
		[MemberData(nameof(GraphTestData.GetRandomGraphsAndExpected), MemberType = typeof(GraphTestData))]
		public void HasVertexThrowsTests (Graph graph, ExpectedGraph expected)
		{
			Assert.Throws<ArgumentOutOfRangeException>(() => graph.HasVertex(-1));
		}
	}
}
