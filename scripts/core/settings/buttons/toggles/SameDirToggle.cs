using Com.Astral.GodotHub.Core.Data;
using Com.Astral.GodotHub.Core.Settings.Buttons.Directory;
using Godot;

namespace Com.Astral.GodotHub.Core.Settings.Buttons.Toggles
{
	public partial class SameDirToggle : SettingToggle
	{
		[Export] protected DownloadDirButton downloadDirButton;

		protected override void OnToggled(bool pToggled)
		{
			AppConfig.UseInstallDirForDownload = pToggled;
			downloadDirButton.Enabled = !pToggled;
		}

		protected override void Reset()
		{
			button.ButtonPressed = AppConfig.UseInstallDirForDownload;
		}
	}
}
