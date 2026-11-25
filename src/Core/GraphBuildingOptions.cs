namespace Graphs.NET.Core
{
	/// <summary>
	/// Configures rules for handling reflexive (self-loop) and duplicate arcs when building a <see cref="Graph"/>.
	/// </summary>
	public class GraphBuildingOptions
	{
		private bool _enableReflexiveArcs = true;
		private bool _enableDuplicateArcs = true;

		/// <summary>
		/// Gets the default fixed options with both reflexive and duplicate arcs enabled.
		/// </summary>
		public static readonly GraphBuildingOptionsFixed Default = new GraphBuildingOptions().Fix();

		/// <summary>
		/// Excludes reflexive arcs (self-loops where source equals target) from the built graph.
		/// </summary>
		/// <returns>The current <see cref="GraphBuildingOptions"/> instance for method chaining.</returns>
		public GraphBuildingOptions DisableReflexiveArcs ()
		{
			_enableReflexiveArcs = false;
			return this;
		}

		/// <summary>
		/// Excludes duplicate parallel arcs between the same pair of vertices from the built graph.
		/// </summary>
		/// <returns>The current <see cref="GraphBuildingOptions"/> instance for method chaining.</returns>
		public GraphBuildingOptions DisableDuplicateArcs ()
		{
			_enableDuplicateArcs = false;
			return this;
		}

		/// <summary>
		/// Freezes the current configuration into an immutable <see cref="GraphBuildingOptionsFixed"/> value.
		/// </summary>
		/// <returns>An immutable <see cref="GraphBuildingOptionsFixed"/> snapshot.</returns>
		public GraphBuildingOptionsFixed Fix ()
		{
			GraphBuildingOptionsFixed options = new(_enableReflexiveArcs, _enableDuplicateArcs);
			return options;
		}
	}

	/// <summary>
	/// Represents an immutable snapshot of graph building options passed to <see cref="GraphBuilder.Build(GraphBuildingOptionsFixed)"/>.
	/// </summary>
	public readonly struct GraphBuildingOptionsFixed
	{
		internal readonly bool enableReflexiveArcs;
		internal readonly bool enableDuplicateArcs;

		internal GraphBuildingOptionsFixed (bool enableReflexive, bool enableDuplicate)
		{
			enableReflexiveArcs = enableReflexive;
			enableDuplicateArcs = enableDuplicate;
		}
	}
}