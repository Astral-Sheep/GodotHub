using Com.Astral.GodotHub.Debug;
using Godot;
using System;

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
			DebugOutput.Instance.PrintMessage("Project tab clicked");
			
			if (projectsTab.Visible)
				return;

			projectsTab.Visible = true;
			versionsTab.Visible = false;
			documentationTab.Visible = false;
		}

		protected void OnVersionsToggled(bool pToggle)
		{
			DebugOutput.Instance.PrintMessage("Version tab clicked");
			
			if (versionsTab.Visible)
				return;

			versionsTab.Visible = true;
			projectsTab.Visible = false;
			documentationTab.Visible = false;
		}

		protected void OnDocumentationToggled(bool pToggle)
		{
			DebugOutput.Instance.PrintMessage("Documentation tab clicked");

			if (documentationTab.Visible)
				return;

			documentationTab.Visible = true;
			projectsTab.Visible = false;
			versionsTab.Visible = false;
		}
	}
}
