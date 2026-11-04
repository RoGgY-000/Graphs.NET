namespace Graphs.NET
{
	public struct Nothing : IComparable<Nothing>
	{
		public readonly int CompareTo (Nothing other) => 0;
	}

	public readonly struct GraphBuildingOptionsFixed
	{
		internal readonly bool alwaysReflexiveArcs;
		internal readonly bool enableReflexiveArcs;
		internal readonly bool enableDuplicateArcs;

		internal GraphBuildingOptionsFixed (bool alwaysReflexive, bool enableReflexive, bool enableDuplicate)
		{
			alwaysReflexiveArcs = alwaysReflexive;
			enableReflexiveArcs = enableReflexive;
			enableDuplicateArcs = enableDuplicate;
		}
	}
}
