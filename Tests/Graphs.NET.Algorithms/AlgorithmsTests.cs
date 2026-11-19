using Graphs.NET.Algorithms;
using Graphs.NET.Core;
using Tests.Data;
using Tests.Extensions;

namespace Tests.Graphs.NET.Algorithms
{
	public class AlgorithmsTests
	{
		[Theory]
		[MemberData(nameof(GraphTestData.GetGraphsAndExpected), MemberType = typeof(GraphTestData))]
		[MemberData(nameof(GraphTestData.GetRandomGraphsAndExpected), MemberType = typeof(GraphTestData))]
		public void TopologicalSort (Graph graph, ExpectedGraph expected)
		{
			Span<int> spanForGraph = new int[expected.VertexCount].AsSpan();
			Span<int> spanForNaive = new int[expected.VertexCount].AsSpan();
			
			bool naiveSuccess = NaiveGraphExtensions.TryRunNaiveTopologicalSort(expected.VerticesAndArcs, expected.VertexCount, spanForNaive);
			bool graphSuccess = graph.TryTopologicalSort(spanForGraph);

			Assert.Equal(naiveSuccess, graphSuccess);

			if ( naiveSuccess )
			{
				Assert.True(spanForNaive.SequenceEqual(spanForGraph));
			}
		}

		[Theory]
		[MemberData(nameof(GraphTestData.GetGraphsAndExpected), MemberType = typeof(GraphTestData))]
		[MemberData(nameof(GraphTestData.GetRandomGraphsAndExpected), MemberType = typeof(GraphTestData))]
		public void TopologicalSortWithAcyclityCheck (Graph graph, ExpectedGraph expected)
		{
			graph._isAcyclicCalculated = false;

			Span<int> spanForGraph = new int[expected.VertexCount].AsSpan();
			Span<int> spanForNaive = new int[expected.VertexCount].AsSpan();

			bool naiveSuccess = NaiveGraphExtensions.TryRunNaiveTopologicalSort(expected.VerticesAndArcs, expected.VertexCount, spanForNaive);
			bool graphSuccess = graph.TryTopologicalSort(spanForGraph);

			Assert.Equal(naiveSuccess, graphSuccess);

			if ( naiveSuccess )
			{
				Assert.True(spanForNaive.SequenceEqual(spanForGraph));
			}
		}
	}
}
