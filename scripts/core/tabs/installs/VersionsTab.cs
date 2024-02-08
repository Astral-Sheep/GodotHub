using Godot;

namespace Com.Astral.GodotHub.Core.Tabs.Versions
{
	public partial class VersionsTab : Tab
	{
		[ExportGroup("Panels")]
		[Export] protected EnginesPanel enginesPanel;
		[Export] protected ReleasePanel releasesPanel;
		[Export] protected Button enginesButton;
		[Export] protected Button releasesButton;

		public override void _Ready()
		{
			enginesButton.ButtonPressed = true;
			releasesButton.ButtonPressed = false;
			enginesPanel.Visible = true;
			releasesPanel.Visible = false;
		}

		protected override void Connect()
		{
			enginesButton.Toggled += OnInstallsToggled;
			releasesButton.Toggled += OnReleasesToggled;
		}

		protected override void Disconnect()
		{
			releasesButton.Toggled -= OnReleasesToggled;
			enginesButton.Toggled -= OnInstallsToggled;
		}

		protected void OnInstallsToggled(bool pToggled)
		{
			if (!pToggled || enginesPanel.Visible)
				return;

			enginesPanel.Visible = true;
			releasesPanel.Visible = false;
		}

		protected void OnReleasesToggled(bool pToggled)
		{
			if (!pToggled || releasesPanel.Visible)
				return;

			releasesPanel.Visible = true;
			enginesPanel.Visible = false;
		}
	}
}
