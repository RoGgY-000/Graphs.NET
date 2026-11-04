using Graphs.NET;

namespace GraphUnitTests
{
	public class GraphWithoutWeightsAndDataTests
	{
		[Fact]
		public void EmptyGraphTests ()
		{
			Graph empty = Graph.Empty;

			Assert.Equal(0, empty.VertexCount);
			Assert.Equal(0, empty.ArcCount);
			Assert.Equal(0f, empty.AverageVertexDegree);
			//Assert.Equal(0f, empty.Density);
			Assert.Equal(0, empty.DuplicateArcCount);
			//Assert.Equal(0, empty.MaxVertexDegree);
			Assert.Equal(0, empty.ReflexiveVertexCount);

			Assert.Throws<ArgumentOutOfRangeException>(() => empty.GetNeighbors(-1));
			Assert.Throws<ArgumentOutOfRangeException>(() => empty.GetNeighbors(0));
			Assert.Throws<ArgumentOutOfRangeException>(() => empty.GetNeighbors(1));

			Assert.Throws<ArgumentOutOfRangeException>(() => empty.HasArc(-1, -1));
			Assert.Throws<ArgumentOutOfRangeException>(() => empty.HasArc(-1, 0));
			Assert.Throws<ArgumentOutOfRangeException>(() => empty.HasArc(-1, 1));
			Assert.Throws<ArgumentOutOfRangeException>(() => empty.HasArc(0, -1));
			Assert.Throws<ArgumentOutOfRangeException>(() => empty.HasArc(0, 0));
			Assert.Throws<ArgumentOutOfRangeException>(() => empty.HasArc(0, 1));
			Assert.Throws<ArgumentOutOfRangeException>(() => empty.HasArc(1, -1));
			Assert.Throws<ArgumentOutOfRangeException>(() => empty.HasArc(1, 0));
			Assert.Throws<ArgumentOutOfRangeException>(() => empty.HasArc(1, 1));
		}

		[Fact]
		public void OneVertexZeroArcsGraphTests ()
		{
			GraphBuilder builder = new(1);

			Assert.Equal(1, builder.VertexCount);
			Assert.Equal(0, builder.EdgeCount);

			Graph graph = builder.Build();

			Assert.Equal(1, graph.VertexCount);
			Assert.Equal(0, graph.ArcCount);
			Assert.Equal(0f, graph.AverageVertexDegree);
			//Assert.Equal(0f, graph.Density);
			Assert.Equal(0, graph.DuplicateArcCount);
			//Assert.Equal(0, graph.MaxVertexDegree);
			Assert.Equal(0, graph.ReflexiveVertexCount);

			Assert.Equal(0, graph.GetNeighbors(0).Length);
			Assert.False(graph.HasArc(0, 0));

			Assert.Throws<ArgumentOutOfRangeException>(() => graph.GetNeighbors(-1));
			Assert.Throws<ArgumentOutOfRangeException>(() => graph.GetNeighbors(1));

			Assert.Throws<ArgumentOutOfRangeException>(() => graph.HasArc(-1, -1));
			Assert.Throws<ArgumentOutOfRangeException>(() => graph.HasArc(-1, 0));
			Assert.Throws<ArgumentOutOfRangeException>(() => graph.HasArc(-1, 1));
			Assert.Throws<ArgumentOutOfRangeException>(() => graph.HasArc(0, -1));
			Assert.Throws<ArgumentOutOfRangeException>(() => graph.HasArc(0, 1));
			Assert.Throws<ArgumentOutOfRangeException>(() => graph.HasArc(1, -1));
			Assert.Throws<ArgumentOutOfRangeException>(() => graph.HasArc(1, 0));
			Assert.Throws<ArgumentOutOfRangeException>(() => graph.HasArc(1, 1));
		}

		[Fact]
		public void OneVertexOneArcGraphTests ()
		{
			GraphBuilder builder = new(1);
			builder.AddArc(0, 0);

			Assert.Equal(1, builder.VertexCount);
			Assert.Equal(1, builder.EdgeCount);

			Graph graph = builder.Build();

			Assert.Equal(1, graph.VertexCount);
			Assert.Equal(1, graph.ArcCount);
			Assert.Equal(2f, graph.AverageVertexDegree);
			//Assert.Equal(0f, graph.Density);
			Assert.Equal(0, graph.DuplicateArcCount);
			//Assert.Equal(1, graph.MaxVertexDegree);
			Assert.Equal(1, graph.ReflexiveVertexCount);

			Assert.Equal(1, graph.GetNeighbors(0).Length);
			Assert.True(graph.HasArc(0, 0));

			Assert.Throws<ArgumentOutOfRangeException>(() => graph.GetNeighbors(-1));
			Assert.Throws<ArgumentOutOfRangeException>(() => graph.GetNeighbors(1));

			Assert.Throws<ArgumentOutOfRangeException>(() => graph.HasArc(-1, -1));
			Assert.Throws<ArgumentOutOfRangeException>(() => graph.HasArc(-1, 0));
			Assert.Throws<ArgumentOutOfRangeException>(() => graph.HasArc(-1, 1));
			Assert.Throws<ArgumentOutOfRangeException>(() => graph.HasArc(0, -1));
			Assert.Throws<ArgumentOutOfRangeException>(() => graph.HasArc(0, 1));
			Assert.Throws<ArgumentOutOfRangeException>(() => graph.HasArc(1, -1));
			Assert.Throws<ArgumentOutOfRangeException>(() => graph.HasArc(1, 0));
			Assert.Throws<ArgumentOutOfRangeException>(() => graph.HasArc(1, 1));
		}

		[Fact]
		public void OneVertexTwoArcsGraphTests ()
		{
			GraphBuilder builder = new(1);
			builder.AddArc(0, 0);
			builder.AddArc(0, 0);

			Assert.Equal(1, builder.VertexCount);
			Assert.Equal(2, builder.EdgeCount);

			Graph graph = builder.Build();

			Assert.Equal(1, graph.VertexCount);
			Assert.Equal(2, graph.ArcCount);
			Assert.Equal(4f, graph.AverageVertexDegree);
			//Assert.Equal(0f, graph.Density);
			Assert.Equal(1, graph.DuplicateArcCount);
			//Assert.Equal(4, graph.MaxVertexDegree);
			Assert.Equal(2, graph.ReflexiveVertexCount);

			Assert.Equal(2, graph.GetNeighbors(0).Length);
			Assert.True(graph.HasArc(0, 0));

			Assert.Throws<ArgumentOutOfRangeException>(() => graph.GetNeighbors(-1));
			Assert.Throws<ArgumentOutOfRangeException>(() => graph.GetNeighbors(1));

			Assert.Throws<ArgumentOutOfRangeException>(() => graph.HasArc(-1, -1));
			Assert.Throws<ArgumentOutOfRangeException>(() => graph.HasArc(-1, 0));
			Assert.Throws<ArgumentOutOfRangeException>(() => graph.HasArc(-1, 1));
			Assert.Throws<ArgumentOutOfRangeException>(() => graph.HasArc(0, -1));
			Assert.Throws<ArgumentOutOfRangeException>(() => graph.HasArc(0, 1));
			Assert.Throws<ArgumentOutOfRangeException>(() => graph.HasArc(1, -1));
			Assert.Throws<ArgumentOutOfRangeException>(() => graph.HasArc(1, 0));
			Assert.Throws<ArgumentOutOfRangeException>(() => graph.HasArc(1, 1));
		}

		[Fact]
		public void TwoVertexesZeroArcsGraphTests ()
		{
			GraphBuilder builder = new(2);

			Assert.Equal(2, builder.VertexCount);
			Assert.Equal(0, builder.EdgeCount);

			Graph graph = builder.Build();

			Assert.Equal(2, graph.VertexCount);
			Assert.Equal(0, graph.ArcCount);
			Assert.Equal(0f, graph.AverageVertexDegree);
			//Assert.Equal(0f, graph.Density);
			Assert.Equal(0, graph.DuplicateArcCount);
			//Assert.Equal(0, graph.MaxVertexDegree);
			Assert.Equal(0, graph.ReflexiveVertexCount);

			Assert.Equal(0, graph.GetNeighbors(0).Length);
			Assert.Equal(0, graph.GetNeighbors(1).Length);
			Assert.False(graph.HasArc(0, 0));
			Assert.False(graph.HasArc(0, 1));
			Assert.False(graph.HasArc(1, 0));
			Assert.False(graph.HasArc(1, 1));

			Assert.Throws<ArgumentOutOfRangeException>(() => graph.GetNeighbors(-1));
			Assert.Throws<ArgumentOutOfRangeException>(() => graph.GetNeighbors(2));

			Assert.Throws<ArgumentOutOfRangeException>(() => graph.HasArc(-1, -1));
			Assert.Throws<ArgumentOutOfRangeException>(() => graph.HasArc(-1, 0));
			Assert.Throws<ArgumentOutOfRangeException>(() => graph.HasArc(-1, 1));
			Assert.Throws<ArgumentOutOfRangeException>(() => graph.HasArc(-1, 2));
			Assert.Throws<ArgumentOutOfRangeException>(() => graph.HasArc(0, -1));
			Assert.Throws<ArgumentOutOfRangeException>(() => graph.HasArc(0, 2));
			Assert.Throws<ArgumentOutOfRangeException>(() => graph.HasArc(1, -1));
			Assert.Throws<ArgumentOutOfRangeException>(() => graph.HasArc(1, 2));
		}

		[Fact]
		public void TwoVertexesOneReflexiveArcGraphTests ()
		{
			GraphBuilder builder = new(2);
			builder.AddArc(1, 1);

			Assert.Equal(2, builder.VertexCount);
			Assert.Equal(1, builder.EdgeCount);

			Graph graph = builder.Build();

			Assert.Equal(2, graph.VertexCount);
			Assert.Equal(1, graph.ArcCount);
			Assert.Equal(1f, graph.AverageVertexDegree);
			//Assert.Equal(0f, graph.Density);
			Assert.Equal(0, graph.DuplicateArcCount);
			//Assert.Equal(1, graph.MaxVertexDegree);
			Assert.Equal(1, graph.ReflexiveVertexCount);

			Assert.Equal(0, graph.GetNeighbors(0).Length);
			Assert.Equal(1, graph.GetNeighbors(1).Length);

			Assert.False(graph.HasArc(0, 0));
			Assert.False(graph.HasArc(0, 1));
			Assert.False(graph.HasArc(1, 0));
			Assert.True(graph.HasArc(1, 1));

			Assert.Throws<ArgumentOutOfRangeException>(() => graph.GetNeighbors(-1));
			Assert.Throws<ArgumentOutOfRangeException>(() => graph.GetNeighbors(2));

			Assert.Throws<ArgumentOutOfRangeException>(() => graph.HasArc(-1, -1));
			Assert.Throws<ArgumentOutOfRangeException>(() => graph.HasArc(-1, 0));
			Assert.Throws<ArgumentOutOfRangeException>(() => graph.HasArc(-1, 1));
			Assert.Throws<ArgumentOutOfRangeException>(() => graph.HasArc(-1, 2));
			Assert.Throws<ArgumentOutOfRangeException>(() => graph.HasArc(0, -1));
			Assert.Throws<ArgumentOutOfRangeException>(() => graph.HasArc(0, 2));
			Assert.Throws<ArgumentOutOfRangeException>(() => graph.HasArc(1, -1));
			Assert.Throws<ArgumentOutOfRangeException>(() => graph.HasArc(1, 2));
		}

		[Fact]
		public void TwoVertexesTwoReflexiveArcsGraphTests ()
		{
			GraphBuilder builder = new(2);
			builder.AddArc(0, 0);
			builder.AddArc(1, 1);

			Assert.Equal(2, builder.VertexCount);
			Assert.Equal(2, builder.EdgeCount);

			Graph graph = builder.Build();

			Assert.Equal(2, graph.VertexCount);
			Assert.Equal(2, graph.ArcCount);
			Assert.Equal(2f, graph.AverageVertexDegree);
			//Assert.Equal(0f, graph.Density);
			Assert.Equal(0, graph.DuplicateArcCount);
			//Assert.Equal(1, graph.MaxVertexDegree);
			Assert.Equal(2, graph.ReflexiveVertexCount);

			Assert.Equal(1, graph.GetNeighbors(0).Length);
			Assert.Equal(1, graph.GetNeighbors(1).Length);

			Assert.True(graph.HasArc(0, 0));
			Assert.False(graph.HasArc(0, 1));
			Assert.False(graph.HasArc(1, 0));
			Assert.True(graph.HasArc(1, 1));

			Assert.Throws<ArgumentOutOfRangeException>(() => graph.GetNeighbors(-1));
			Assert.Throws<ArgumentOutOfRangeException>(() => graph.GetNeighbors(2));

			Assert.Throws<ArgumentOutOfRangeException>(() => graph.HasArc(-1, -1));
			Assert.Throws<ArgumentOutOfRangeException>(() => graph.HasArc(-1, 0));
			Assert.Throws<ArgumentOutOfRangeException>(() => graph.HasArc(-1, 1));
			Assert.Throws<ArgumentOutOfRangeException>(() => graph.HasArc(-1, 2));
			Assert.Throws<ArgumentOutOfRangeException>(() => graph.HasArc(0, -1));
			Assert.Throws<ArgumentOutOfRangeException>(() => graph.HasArc(0, 2));
			Assert.Throws<ArgumentOutOfRangeException>(() => graph.HasArc(1, -1));
			Assert.Throws<ArgumentOutOfRangeException>(() => graph.HasArc(1, 2));
		}

		[Fact]
		public void TwoVertexesThreeReflexiveArcsGraphTests ()
		{
			GraphBuilder builder = new(2);
			builder.AddArc(0, 0);
			builder.AddArc(1, 1);
			builder.AddArc(1, 1);

			Assert.Equal(2, builder.VertexCount);
			Assert.Equal(3, builder.EdgeCount);

			Graph graph = builder.Build();

			Assert.Equal(2, graph.VertexCount);
			Assert.Equal(3, graph.ArcCount);
			Assert.Equal(3f, graph.AverageVertexDegree);
			//Assert.Equal(0f, graph.Density);
			Assert.Equal(1, graph.DuplicateArcCount);
			//Assert.Equal(1, graph.MaxVertexDegree);
			Assert.Equal(3, graph.ReflexiveVertexCount);

			Assert.Equal(1, graph.GetNeighbors(0).Length);
			Assert.Equal(2, graph.GetNeighbors(1).Length);

			Assert.True(graph.HasArc(0, 0));
			Assert.False(graph.HasArc(0, 1));
			Assert.False(graph.HasArc(1, 0));
			Assert.True(graph.HasArc(1, 1));

			Assert.Throws<ArgumentOutOfRangeException>(() => graph.GetNeighbors(-1));
			Assert.Throws<ArgumentOutOfRangeException>(() => graph.GetNeighbors(2));

			Assert.Throws<ArgumentOutOfRangeException>(() => graph.HasArc(-1, -1));
			Assert.Throws<ArgumentOutOfRangeException>(() => graph.HasArc(-1, 0));
			Assert.Throws<ArgumentOutOfRangeException>(() => graph.HasArc(-1, 1));
			Assert.Throws<ArgumentOutOfRangeException>(() => graph.HasArc(-1, 2));
			Assert.Throws<ArgumentOutOfRangeException>(() => graph.HasArc(0, -1));
			Assert.Throws<ArgumentOutOfRangeException>(() => graph.HasArc(0, 2));
			Assert.Throws<ArgumentOutOfRangeException>(() => graph.HasArc(1, -1));
			Assert.Throws<ArgumentOutOfRangeException>(() => graph.HasArc(1, 2));
		}

		[Fact]
		public void TwoVertexesOneArcGraphTests ()
		{
			GraphBuilder builder = new(2);
			builder.AddArc(1, 0);

			Assert.Equal(2, builder.VertexCount);
			Assert.Equal(1, builder.EdgeCount);

			Graph graph = builder.Build();

			Assert.Equal(2, graph.VertexCount);
			Assert.Equal(1, graph.ArcCount);
			Assert.Equal(1f, graph.AverageVertexDegree);
			//Assert.Equal(0f, graph.Density);
			Assert.Equal(0, graph.DuplicateArcCount);
			//Assert.Equal(1, graph.MaxVertexDegree);
			Assert.Equal(0, graph.ReflexiveVertexCount);

			Assert.Equal(0, graph.GetNeighbors(0).Length);
			Assert.Equal(1, graph.GetNeighbors(1).Length);

			Assert.False(graph.HasArc(0, 0));
			Assert.False(graph.HasArc(0, 1));
			Assert.True(graph.HasArc(1, 0));
			Assert.False(graph.HasArc(1, 1));

			Assert.Throws<ArgumentOutOfRangeException>(() => graph.GetNeighbors(-1));
			Assert.Throws<ArgumentOutOfRangeException>(() => graph.GetNeighbors(2));

			Assert.Throws<ArgumentOutOfRangeException>(() => graph.HasArc(-1, -1));
			Assert.Throws<ArgumentOutOfRangeException>(() => graph.HasArc(-1, 0));
			Assert.Throws<ArgumentOutOfRangeException>(() => graph.HasArc(-1, 1));
			Assert.Throws<ArgumentOutOfRangeException>(() => graph.HasArc(-1, 2));
			Assert.Throws<ArgumentOutOfRangeException>(() => graph.HasArc(0, -1));
			Assert.Throws<ArgumentOutOfRangeException>(() => graph.HasArc(0, 2));
			Assert.Throws<ArgumentOutOfRangeException>(() => graph.HasArc(1, -1));
			Assert.Throws<ArgumentOutOfRangeException>(() => graph.HasArc(1, 2));
		}

		[Fact]
		public void TwoVertexesTwoArcsGraphTests ()
		{
			GraphBuilder builder = new(2);
			builder.AddArc(0, 1);
			builder.AddArc(1, 0);

			Assert.Equal(2, builder.VertexCount);
			Assert.Equal(2, builder.EdgeCount);

			Graph graph = builder.Build();

			Assert.Equal(2, graph.VertexCount);
			Assert.Equal(2, graph.ArcCount);
			Assert.Equal(2f, graph.AverageVertexDegree);
			//Assert.Equal(0f, graph.Density);
			Assert.Equal(0, graph.DuplicateArcCount);
			//Assert.Equal(1, graph.MaxVertexDegree);
			Assert.Equal(0, graph.ReflexiveVertexCount);

			Assert.Equal(1, graph.GetNeighbors(0).Length);
			Assert.Equal(1, graph.GetNeighbors(1).Length);

			Assert.False(graph.HasArc(0, 0));
			Assert.True(graph.HasArc(0, 1));
			Assert.True(graph.HasArc(1, 0));
			Assert.False(graph.HasArc(1, 1));

			Assert.Throws<ArgumentOutOfRangeException>(() => graph.GetNeighbors(-1));
			Assert.Throws<ArgumentOutOfRangeException>(() => graph.GetNeighbors(2));

			Assert.Throws<ArgumentOutOfRangeException>(() => graph.HasArc(-1, -1));
			Assert.Throws<ArgumentOutOfRangeException>(() => graph.HasArc(-1, 0));
			Assert.Throws<ArgumentOutOfRangeException>(() => graph.HasArc(-1, 1));
			Assert.Throws<ArgumentOutOfRangeException>(() => graph.HasArc(-1, 2));
			Assert.Throws<ArgumentOutOfRangeException>(() => graph.HasArc(0, -1));
			Assert.Throws<ArgumentOutOfRangeException>(() => graph.HasArc(0, 2));
			Assert.Throws<ArgumentOutOfRangeException>(() => graph.HasArc(1, -1));
			Assert.Throws<ArgumentOutOfRangeException>(() => graph.HasArc(1, 2));
		}

		[Fact]
		public void TwoVertexesThreeArcsGraphTests ()
		{
			GraphBuilder builder = new(2);
			builder.AddArc(0, 1);
			builder.AddArc(1, 0);
			builder.AddArc(0, 1);

			Assert.Equal(2, builder.VertexCount);
			Assert.Equal(3, builder.EdgeCount);

			Graph graph = builder.Build();

			Assert.Equal(2, graph.VertexCount);
			Assert.Equal(3, graph.ArcCount);
			Assert.Equal(3f, graph.AverageVertexDegree);
			//Assert.Equal(0f, graph.Density);
			Assert.Equal(1, graph.DuplicateArcCount);
			//Assert.Equal(1, graph.MaxVertexDegree);
			Assert.Equal(0, graph.ReflexiveVertexCount);

			Assert.Equal(2, graph.GetNeighbors(0).Length);
			Assert.Equal(1, graph.GetNeighbors(1).Length);

			Assert.False(graph.HasArc(0, 0));
			Assert.True(graph.HasArc(0, 1));
			Assert.True(graph.HasArc(1, 0));
			Assert.False(graph.HasArc(1, 1));

			Assert.Throws<ArgumentOutOfRangeException>(() => graph.GetNeighbors(-1));
			Assert.Throws<ArgumentOutOfRangeException>(() => graph.GetNeighbors(2));

			Assert.Throws<ArgumentOutOfRangeException>(() => graph.HasArc(-1, -1));
			Assert.Throws<ArgumentOutOfRangeException>(() => graph.HasArc(-1, 0));
			Assert.Throws<ArgumentOutOfRangeException>(() => graph.HasArc(-1, 1));
			Assert.Throws<ArgumentOutOfRangeException>(() => graph.HasArc(-1, 2));
			Assert.Throws<ArgumentOutOfRangeException>(() => graph.HasArc(0, -1));
			Assert.Throws<ArgumentOutOfRangeException>(() => graph.HasArc(0, 2));
			Assert.Throws<ArgumentOutOfRangeException>(() => graph.HasArc(1, -1));
			Assert.Throws<ArgumentOutOfRangeException>(() => graph.HasArc(1, 2));
		}
	}
}
