using Graphs.NET.Core;
using Tests.Data;

namespace Tests.Graphs.NET.Core
{
	public class GraphBuilderWithWeightAndDataTests : GraphBuilderWithWeightsTests
	{
		[Theory]
		[MemberData(nameof(GraphBuilderWithWeightAndDataTestData.GetGraphBuildersAndExpected), MemberType = typeof(GraphBuilderWithWeightAndDataTestData))]
		[MemberData(nameof(GraphBuilderWithWeightAndDataTestData.GetRandomGraphBuildersAndExpected), MemberType = typeof(GraphBuilderWithWeightAndDataTestData))]
		public void AddVertexWithData (GraphBuilder<string, object> builder, ExpectedGraphBuilder<string, object> expected)
		{
			int newVertexIndex = builder.VertexCount > 0 ? expected.VerticesAndArcs.Keys.Max() + 1 : 0;
			object data = new();

			builder.AddVertex(newVertexIndex, data);

			Assert.Same(builder.GetVertexData(newVertexIndex), data);
		}
	}
}
