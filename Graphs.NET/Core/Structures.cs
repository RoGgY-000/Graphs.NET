namespace Graphs.NET.Core
{
	public struct Nothing : IComparable<Nothing>
	{
		public readonly int CompareTo (Nothing other)
		{
			return 0;
		}
	}

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
