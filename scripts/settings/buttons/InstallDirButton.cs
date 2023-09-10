using Godot;

namespace Com.Astral.GodotHub.Settings.Buttons
{
	public partial class InstallDirButton : DirButton
	{
		[Export] protected DownloadDirButton downloadDirButton;

		protected override void OnDirSelected(string pDir)
		{
			Config.InstallDir = pDir;

			if (Config.UseInstallDirForDownload)
			{
				Config.DownloadDir = pDir;
				downloadDirButton.Text = pDir;
			}

			base.OnDirSelected(pDir);
		}

		protected override void Reset()
		{
			button.Text = Config.InstallDir;
		}
	}
}
