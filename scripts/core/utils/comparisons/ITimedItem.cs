namespace Com.Astral.GodotHub.Core.Utils.Comparisons
{
	public interface ITimedItem
	{
		/// <summary>
		/// Time since the last time this item's files were opened
		/// </summary>
		double TimeSinceLastOpening { get; }
	}
}
