namespace Graphs.NET.Core
{
	public class GraphBuildingOptions
	{
		private bool _enableReflexiveEdges = true;
		private bool _enableDuplicateEdges = true;

		public static GraphBuildingOptionsFixed Default { get; } = new GraphBuildingOptions().Fix();

		public GraphBuildingOptions EnableReflexiveEdges ()
		{
			_enableReflexiveEdges = true;
			return this;
		}
		public GraphBuildingOptions DisableReflexiveEdges ()
		{
			_enableReflexiveEdges = false;
			return this;
		}
		public GraphBuildingOptions EnableDuplicateEdges ()
		{
			_enableDuplicateEdges = true;
			return this;
		}
		public GraphBuildingOptions DisableDuplicateEdges ()
		{
			_enableDuplicateEdges = false;
			return this;
		}

		public GraphBuildingOptionsFixed Fix ()
		{
			GraphBuildingOptionsFixed options = new(_enableReflexiveEdges, _enableDuplicateEdges);
			return options;
		}
	}
}