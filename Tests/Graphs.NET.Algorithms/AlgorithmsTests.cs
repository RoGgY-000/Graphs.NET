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
		public void ShortestPath (Graph graph, ExpectedGraph expected)
		{
			Span<int> spanForGraph = new int[expected.VertexCount].AsSpan();
			Span<int> spanForNaive = new int[expected.VertexCount].AsSpan();

			foreach ( int start in expected.VerticesAndArcs.Keys )
			{
				foreach ( int end in expected.VerticesAndArcs.Keys )
				{
					bool naiveSuccess = NaiveGraphExtensions.TryRunNaiveShortestPath(expected.VerticesAndArcs, start, end, spanForNaive, out int naiveLength);
					bool graphSuccess = graph.TryFindShortestPath(start, end, spanForGraph, out int graphLength);

					Assert.Equal(naiveSuccess, graphSuccess);
					Assert.Equal(naiveLength, graphLength);

					if ( naiveSuccess )
					{
						Assert.True(spanForNaive.Slice(0, naiveLength + 1).SequenceEqual(spanForGraph.Slice(0, naiveLength + 1)));
					}
				}
			}
		}

		[Theory]
		[MemberData(nameof(GraphTestData.GetGraphsAndExpected), MemberType = typeof(GraphTestData))]
		[MemberData(nameof(GraphTestData.GetRandomGraphsAndExpected), MemberType = typeof(GraphTestData))]
		public void DistanceTree (Graph graph, ExpectedGraph expected)
		{
			Span<int> spanForGraph = expected.VertexCount > 0 ? new int[expected.VerticesAndArcs.Keys.Max() + 1].AsSpan() : Span<int>.Empty;
			Span<int> spanForNaive = expected.VertexCount > 0 ? new int[expected.VerticesAndArcs.Keys.Max() + 1].AsSpan() : Span<int>.Empty;

			foreach ( int start in expected.VerticesAndArcs.Keys )
			{
				NaiveGraphExtensions.RunNaiveShortestDistances(expected.VerticesAndArcs, start, spanForNaive);
				graph.GetShortestDistances(start, spanForGraph);

				Assert.True(spanForNaive.SequenceEqual(spanForGraph));
			}
		}

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
