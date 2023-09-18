using Godot;

namespace Com.Astral.GodotHub.Tabs
{
	public abstract partial class SortedPanel : Control
	{
		public enum SortType
		{
			Version = 0,
			Date = 1,
		}

		[Export] protected Control itemContainer;
	}
}
