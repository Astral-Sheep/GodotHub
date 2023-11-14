namespace Com.Astral.GodotHub.Core.Settings.Buttons.Toggles
{
	public abstract partial class SettingToggle : SettingButton
	{
		public override void Connect()
		{
			button.Toggled += OnToggled;
		}

		public override void Disconnect()
		{
			button.Toggled -= OnToggled;
		}

		protected abstract void OnToggled(bool pToggled);
	}
}
