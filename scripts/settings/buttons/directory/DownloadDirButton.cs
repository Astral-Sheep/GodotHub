using Com.Astral.GodotHub.Data;

namespace Com.Astral.GodotHub.Settings.Buttons.Directory
{
	public partial class DownloadDirButton : DirButton
	{
		public bool Enabled
		{
			get => _enabled;
			set
			{
				if (Enabled == value)
					return;

				Visible = value;
				_enabled = value;
			}
		}
		protected bool _enabled = true;

		protected override void OnDirSelected(string pDir)
		{
			Config.DownloadDir = pDir;
			base.OnDirSelected(pDir);
		}

		protected override void Reset()
		{
			button.Text = " " + Config.DownloadDir;
			Enabled = !Config.UseInstallDirForDownload;
		}
	}
}
