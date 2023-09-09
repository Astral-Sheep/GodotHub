using Com.Astral.GodotHub.Debug;
using Godot;

namespace Com.Astral.GodotHub
{
	public partial class TabManager : Node
	{
		[ExportGroup("Tabs")]
		[Export] protected Tab projectsTab;
		[Export] protected Tab versionsTab;
		[Export] protected Tab documentationTab;

		[ExportGroup("Buttons")]
		[Export] protected Button projectsButton;
		[Export] protected Button versionsButton;
		[Export] protected Button documentationButton;

		public override void _Ready()
		{
			projectsButton.Toggled += OnProjectsToggled;
			versionsButton.Toggled += OnVersionsToggled;
			documentationButton.Toggled += OnDocumentationToggled;
			projectsTab.Visible = projectsButton.ButtonPressed;
			versionsTab.Visible = versionsButton.ButtonPressed;
			documentationTab.Visible = documentationButton.ButtonPressed;
		}

		protected override void Dispose(bool pDisposing)
		{
			if (!pDisposing)
				return;

			if (documentationButton != null)
			{
				documentationButton.Toggled -= OnDocumentationToggled;
			}

			if (versionsButton != null)
			{
				versionsButton.Toggled -= OnVersionsToggled;
			}

			if (projectsButton != null)
			{
				projectsButton.Toggled -= OnProjectsToggled;
			}
		}

		protected void OnProjectsToggled(bool pToggle)
		{
			if (projectsTab.Visible || !pToggle)
				return;

			Debugger.PrintMessage("Project tab clicked");

			projectsTab.Visible = true;
			versionsTab.Visible = false;
			documentationTab.Visible = false;
		}

		protected void OnVersionsToggled(bool pToggle)
		{
			if (versionsTab.Visible || !pToggle)
				return;

			Debugger.PrintMessage("Version tab clicked");
			
			versionsTab.Visible = true;
			projectsTab.Visible = false;
			documentationTab.Visible = false;
		}

		protected void OnDocumentationToggled(bool pToggle)
		{
			if (documentationTab.Visible || !pToggle)
				return;

			Debugger.PrintMessage("Documentation tab clicked");

			documentationTab.Visible = true;
			projectsTab.Visible = false;
			versionsTab.Visible = false;
		}
	}
}
