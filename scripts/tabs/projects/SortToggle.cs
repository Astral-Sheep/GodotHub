using Godot;
using System;

namespace Com.Astral.GodotHub.Tabs.Projects
{
	public partial class SortToggle : Button
	{
		public event Action<bool> CustomToggled;
		protected bool toggled = false;

		public override void _Ready()
		{
			Toggled += OnToggled;
		}

		public void Disable()
		{
			if (!ButtonPressed)
				return;

			SetPressedNoSignal(false);
			toggled = false;
		}

		protected void OnToggled(bool pToggled)
		{
			SetPressedNoSignal(true);
			toggled = !toggled;
			CustomToggled?.Invoke(toggled);
		}
	}
}
