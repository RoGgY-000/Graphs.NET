namespace Graphs.NET.Core
{
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
