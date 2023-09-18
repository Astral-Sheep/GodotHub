using Godot;

namespace Com.Astral.GodotHub.Tabs.Installs
{
	public partial class InstallsTab : Tab
	{
		[ExportGroup("Panels")]
		[Export] protected InstallsPanel installsPanel;
		[Export] protected ReleasePanel releasesPanel;
		[Export] protected Button installsButton;
		[Export] protected Button releasesButton;

		public override void _Ready()
		{
			installsButton.ButtonPressed = true;
			releasesButton.ButtonPressed = false;
			installsPanel.Visible = true;
			releasesPanel.Visible = false;
		}

		protected override void Connect()
		{
			installsButton.Toggled += OnInstallsToggled;
			releasesButton.Toggled += OnReleasesToggled;
		}

		protected override void Disconnect()
		{
			releasesButton.Toggled -= OnReleasesToggled;
			installsButton.Toggled -= OnInstallsToggled;
		}

		protected void OnInstallsToggled(bool pToggled)
		{
			if (!pToggled || installsPanel.Visible)
				return;

			installsPanel.Visible = true;
			releasesPanel.Visible = false;
		}

		protected void OnReleasesToggled(bool pToggled)
		{
			if (!pToggled || releasesPanel.Visible)
				return;

			releasesPanel.Visible = true;
			installsPanel.Visible = false;
		}
	}
}
