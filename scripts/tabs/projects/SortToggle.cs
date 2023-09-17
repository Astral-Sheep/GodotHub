using Godot;
using System;

namespace Com.Astral.GodotHub.Tabs.Projects
{
	public partial class SortToggle : Button
	{
		public event Action<bool> CustomToggled;

		[Export] protected TextureRect arrowRect;

		protected bool toggled = false;

		public override void _Ready()
		{
			arrowRect.Visible = false;
			Toggled += OnToggled;
		}

		public void Disable()
		{
			if (!ButtonPressed)
				return;

			SetPressedNoSignal(false);
			arrowRect.Visible = false;
			toggled = false;
		}

		public void OnToggled(bool pToggled)
		{
			SetPressedNoSignal(true);
			arrowRect.Visible = true;
			toggled = !toggled;
			arrowRect.Rotation = toggled ? 0f : Mathf.Pi;
			CustomToggled?.Invoke(toggled);
		}
	}
}
