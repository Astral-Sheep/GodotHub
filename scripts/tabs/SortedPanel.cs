using Godot;
using System.Collections.Generic;

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

		public abstract void Sort(SortType pType, bool pReversed);
	}
}
