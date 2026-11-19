using Graphs.NET.Core;
using Tests.Data;

namespace Tests.Graphs.NET.Core
{
	public class GraphWithWeightsTests : GraphTests
	{
		[Theory]
		[MemberData(nameof(GraphWithWeightTestData.GetGraphsAndExpected), MemberType = typeof(GraphWithWeightTestData))]
		[MemberData(nameof(GraphWithWeightTestData.GetRandomGraphsAndExpected), MemberType = typeof(GraphWithWeightTestData))]
		public void GetWeightTests (Graph<string> graph, ExpectedGraphWithWeights<string> expected)
		{
			foreach ( int vertex in expected.Weights.Keys )
			{
				Dictionary<int, string[]> expectedWeights = expected.Weights[vertex];
				string[] allExpectedWeightsArray = expectedWeights
					.OrderBy(kvp => kvp.Key)
					.SelectMany(kvp => kvp.Value)
					.ToArray();
				ReadOnlySpan<string> graphWeights = graph.GetWeights(vertex);

				Assert.Equal(allExpectedWeightsArray.Length, graphWeights.Length);

				Assert.True(allExpectedWeightsArray.SequenceEqual(graphWeights));
			}
		}
	}
}