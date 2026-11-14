using Graphs.NET.Core;
using Tests.Data;

namespace Tests.Graphs.NET.Core
{
	public class GraphWithWeightsTests
	{
		[Theory]
		[MemberData(nameof(GraphTestData.GetGraphsAndExpected), MemberType = typeof(GraphTestData))]
		[MemberData(nameof(GraphTestData.GetRandomGraphsAndExpected), MemberType = typeof(GraphTestData))]
		public void ArcCountTests (Graph graph, ExpectedGraph expected)
		{
			Assert.Equal(expected.ArcCount, graph.ArcCount);
		}
	}
}