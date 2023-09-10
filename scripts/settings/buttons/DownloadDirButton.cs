namespace Com.Astral.GodotHub.Settings.Buttons
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

		public override void _Ready()
		{
			button.Text = Config.InstallDir;
			Enabled = !Config.UseInstallDirForDownload;
		}

		public override void Connect()
		{
			if (!_enabled)
				return;

			base.Connect();
		}

		public override void Disconnect()
		{
			if (!_enabled)
				return;

			base.Disconnect();
		}

		protected override void OnDirSelected(string pDir)
		{
			Config.DownloadDir = pDir;
			base.OnDirSelected(pDir);
		}
	}
}
