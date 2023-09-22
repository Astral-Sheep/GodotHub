using Com.Astral.GodotHub.Data;
using Com.Astral.GodotHub.Debug;
using Godot;
using System;
using System.Collections.Generic;
using System.IO;
using Version = Com.Astral.GodotHub.Data.Version;

namespace Com.Astral.GodotHub.Tabs.Projects
{
	public partial class NewProjectDialog : ConfirmationDialog
	{
		public new event Action<string, string, Version, RenderMode, VersionningMode> Confirmed;

		[ExportGroup("Project name")]
		[Export] protected LineEdit projectName;
		[Export] protected LineEdit projectDirectory;
		[Export] protected Button folderCreateButton;
		[Export] protected Button browseButton;

		[ExportGroup("Project version")]
		[Export] protected OptionButton versionOption;

		[ExportGroup("Renderer options")]
		[Export] protected OptionButton renderOption;

		[ExportGroup("Versionning options")]
		[Export] protected OptionButton versionningOption;

		public override void _Ready()
		{
			base.Confirmed += OnConfirmed;
			PopupCentered();
			GetOkButton().FocusMode = Control.FocusModeEnum.None;
			GetCancelButton().FocusMode = Control.FocusModeEnum.None;
			projectDirectory.Text = Config.ProjectDir;

			List<GDFile> lAvailableVersions = InstallsData.GetAllVersions();
			lAvailableVersions.Sort((lhs, rhs) => rhs.Version.CompareTo(lhs.Version));

			for (int i = 0; i < lAvailableVersions.Count; i++)
			{
				versionOption.AddItem((string)lAvailableVersions[i].Version, i);
			}

			versionOption.Selected = 0;

			renderOption.AddItem(RenderMode.Forward.ToString(), (int)RenderMode.Forward);
			renderOption.AddItem(RenderMode.Mobile.ToString(), (int)RenderMode.Mobile);
			renderOption.AddItem(RenderMode.Compatibility.ToString(), (int)RenderMode.Compatibility);
			renderOption.Selected = 0;

			versionningOption.AddItem(VersionningMode.None.ToString(), (int)VersionningMode.None);
			versionningOption.AddItem(VersionningMode.Git.ToString(), (int)VersionningMode.Git);
			versionningOption.Selected = 0;

			projectName.TextChanged += OnNameTextChanged;
			projectDirectory.TextChanged += OnDirectoryTextChanged;
			folderCreateButton.Pressed += OnFolderCreatePressed;
			browseButton.Pressed += OnBrowsePressed;
		}

		protected void OnConfirmed()
		{
			Confirmed?.Invoke(
				projectName.Text,
				projectDirectory.Text,
				(Version)versionOption.Text,
				(RenderMode)renderOption.Selected,
				(VersionningMode)versionningOption.Selected
			);
		}

		protected void OnNameTextChanged(string pName)
		{
			if (string.IsNullOrEmpty(pName))
			{
				Debugger.PrintError("Invalid name: empty name");
			}
		}

		protected void OnDirectoryTextChanged(string pPath)
		{
			if (!Directory.Exists(pPath))
			{
				Debugger.PrintError("Invalid path");
			}
			else if (Directory.GetFiles(pPath).Length > 0)
			{
				Debugger.PrintWarning("Non empty directory");
			}
		}

		protected void OnFolderCreatePressed()
		{
			string lPath = $"{projectDirectory.Text}/{projectName.Text}";

			if (Directory.Exists(lPath))
			{
				Debugger.PrintError($"Directory already exists at path {lPath}");
				return;
			}

			Directory.CreateDirectory(lPath);
			projectDirectory.Text = lPath;
		}

		protected void OnBrowsePressed()
		{
			FileDialog lDialog = Main.Instance.InstantiateFileDialog(this);
			lDialog.FileMode = FileDialog.FileModeEnum.OpenDir;
			lDialog.CurrentDir = Directory.Exists(projectDirectory.Text) ?
				projectDirectory.Text :
				PathT.GetEnvironmentPath(System.Environment.SpecialFolder.UserProfile);
			lDialog.DirSelected += OnDirSelected;
		}

		protected void OnDirSelected(string pDir)
		{
			projectDirectory.Text = pDir;
		}
	}
}
