namespace Graphs.NET
{
	public struct Empty : IComparable<Empty>
	{
		public readonly int CompareTo (Empty other) => 0;
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
