using Godot;

namespace Com.Astral.GodotHub.Tabs
{
	public partial class TabManager : Control
	{
		[ExportGroup("Tabs")]
		[Export] protected Tab projectsTab;
		[Export] protected Tab installsTab;
		[Export] protected Tab documentationTab;

		[ExportGroup("Buttons")]
		[Export] protected Button projectsButton;
		[Export] protected Button installsButton;
		[Export] protected Button documentationButton;

		public override void _Ready()
		{
			projectsButton.Toggled += OnProjectsToggled;
			installsButton.Toggled += OnInstallsToggled;
			documentationButton.Toggled += OnDocumentationToggled;

			projectsTab.Toggle(projectsButton.ButtonPressed);
			installsTab.Toggle(installsButton.ButtonPressed);
			documentationTab.Toggle(documentationButton.ButtonPressed);
		}

		protected void OnProjectsToggled(bool pToggle)
		{
			if (projectsTab.Visible || !pToggle)
				return;

			projectsTab.Toggle(true);
			installsTab.Toggle(false);
			documentationTab.Toggle(false);
		}

		protected void OnInstallsToggled(bool pToggle)
		{
			if (installsTab.Visible || !pToggle)
				return;
			
			installsTab.Toggle(true);
			projectsTab.Toggle(false);
			documentationTab.Toggle(false);
		}

		protected void OnDocumentationToggled(bool pToggle)
		{
			if (documentationTab.Visible || !pToggle)
				return;

			documentationTab.Toggle(true);
			projectsTab.Toggle(false);
			installsTab.Toggle(false);
		}
	}
}
