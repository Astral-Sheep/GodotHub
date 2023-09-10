using Godot;

namespace Com.Astral.GodotHub.Settings.Buttons
{
	public abstract partial class SettingButton : Control
	{
		[Export] protected Button button;

		public abstract void Connect();
		public abstract void Disconnect();
	}
}
