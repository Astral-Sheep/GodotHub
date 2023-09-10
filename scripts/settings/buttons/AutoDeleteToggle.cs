using Godot;

namespace Com.Astral.GodotHub.Settings.Buttons
{
	public partial class AutoDeleteToggle : SettingButton
	{
		[Export] protected DownloadDirButton downloadDirButton;

		public override void Connect()
		{
			button.Toggled += OnToggled;
		}

		public override void Disconnect()
		{
			button.Toggled -= OnToggled;
		}

		protected void OnToggled(bool pToggled)
		{
			Config.AutoDeleteDownload = pToggled;
		}

		protected override void Reset()
		{
			button.ButtonPressed = Config.AutoDeleteDownload;
		}
	}
}
