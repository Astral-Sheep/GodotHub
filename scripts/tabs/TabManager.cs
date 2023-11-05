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
#if DEBUG
			documentationButton.Toggled += OnDocumentationToggled;
#endif //DEBUG

			projectsTab.Toggle(projectsButton.ButtonPressed);
			installsTab.Toggle(installsButton.ButtonPressed);
#if DEBUG
			documentationTab.Toggle(documentationButton.ButtonPressed);
#else
			documentationButton.Visible = false;
			documentationTab.Visible = false;
#endif //DEBUG
		}

		protected void OnProjectsToggled(bool pToggle)
		{
			if (projectsTab.Visible || !pToggle)
				return;

			projectsTab.Toggle(true);
			installsTab.Toggle(false);
#if DEBUG
			documentationTab.Toggle(false);
#endif //DEBUG
		}

		protected void OnInstallsToggled(bool pToggle)
		{
			if (installsTab.Visible || !pToggle)
				return;
			
			installsTab.Toggle(true);
			projectsTab.Toggle(false);
#if DEBUG
			documentationTab.Toggle(false);
#endif //DEBUG
		}

#if DEBUG
		protected void OnDocumentationToggled(bool pToggle)
		{
			if (documentationTab.Visible || !pToggle)
				return;

			documentationTab.Toggle(true);
			projectsTab.Toggle(false);
			installsTab.Toggle(false);
		}
#endif //DEBUG
	}
}
