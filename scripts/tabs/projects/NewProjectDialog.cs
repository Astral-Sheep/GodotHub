using Com.Astral.GodotHub.Data;
using Com.Astral.GodotHub.Debug;
using Com.Astral.GodotHub.Utils;
using Godot;
using System;
using System.Collections.Generic;
using System.IO;
using Version = Com.Astral.GodotHub.Data.Version;

namespace Com.Astral.GodotHub.Tabs.Projects
{
	public partial class NewProjectDialog : ConfirmationDialog
	{
		/// <summary>
		/// Event called when the ok button is pressed
		/// </summary>
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

		protected int currentMajor;

		public override void _Ready()
		{
			base.Confirmed += OnConfirmed;
			PopupCentered();
			GetOkButton().FocusMode = Control.FocusModeEnum.None;
			GetCancelButton().FocusMode = Control.FocusModeEnum.None;
			projectDirectory.Text = AppConfig.ProjectDir;

			List<GDFile> lAvailableVersions = InstallsData.GetAllVersions();

			if (lAvailableVersions.Count > 0)
			{
				lAvailableVersions.Sort((lhs, rhs) => rhs.Version.CompareTo(lhs.Version));

				for (int i = 0; i < lAvailableVersions.Count; i++)
				{
					versionOption.AddItem((string)lAvailableVersions[i].Version, i);
				}

				versionOption.Selected = 0;
				currentMajor = lAvailableVersions[0].Version.major;
				SetRenderModes();
			}
			else
			{
				versionOption.Disabled = true;
				renderOption.Disabled = true;
				GetOkButton().Disabled = true;
			}

			versionningOption.AddItem(VersionningMode.None.ToString(), (int)VersionningMode.None);
			versionningOption.AddItem(VersionningMode.Git.ToString(), (int)VersionningMode.Git);
			versionningOption.Selected = 0;

			projectName.TextChanged += OnNameTextChanged;
			projectDirectory.TextChanged += OnDirectoryTextChanged;
			folderCreateButton.Pressed += OnFolderCreatePressed;
			browseButton.Pressed += OnBrowsePressed;
			versionOption.ItemSelected += OnVersionSelected;
		}

		#region EVENT_HANDLING

		protected void OnConfirmed()
		{
			Confirmed?.Invoke(
				projectName.Text,
				projectDirectory.Text,
				(Version)versionOption.Text,
				GetRenderMode(),
				(VersionningMode)versionningOption.Selected
			);
		}

		protected void OnNameTextChanged(string pName)
		{
			//To do: add RichTextLabel to log this on dialog popup
			if (string.IsNullOrEmpty(pName))
			{
				Debugger.LogError("Invalid name: empty name");
			}
		}

		protected void OnDirectoryTextChanged(string pPath)
		{
			//To do: add RichTextLabel to log this on dialog popup
			if (!Directory.Exists(pPath))
			{
				Debugger.LogError("Invalid path");
			}
			else if (Directory.GetFiles(pPath).Length > 0)
			{
				Debugger.LogWarning("Non empty directory");
			}
		}

		protected void OnFolderCreatePressed()
		{
			string lPath = $"{projectDirectory.Text}/{projectName.Text}";

			if (Directory.Exists(lPath))
			{
				ExceptionHandler.Singleton.LogMessage(
					$"{projectDirectory.Text} directory already exists",
					$"Directory already exists",
					ExceptionHandler.ExceptionGravity.Error
				);
				Debugger.LogError($"{projectDirectory.Text} directory already exists");
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

		protected void OnVersionSelected(long _)
		{
			int lMajor = int.Parse(new ReadOnlySpan<char>(new char[] { versionOption.Text[0] }));

			if (lMajor == currentMajor)
				return;

			currentMajor = lMajor;
			SetRenderModes();
		}

		#endregion //EVENT_HANDLING

		protected void SetRenderModes()
		{
			if (currentMajor < 4)
			{
				renderOption.Clear();
				renderOption.AddItem(RenderMode.OpenGL3.ToString(), 0);
				renderOption.AddItem(RenderMode.OpenGL2.ToString(), 1);
			}
			else
			{
				renderOption.Clear();
				renderOption.AddItem(RenderMode.Forward.ToString(), 0);
				renderOption.AddItem(RenderMode.Mobile.ToString(), 1);
				renderOption.AddItem(RenderMode.Compatibility.ToString(), 1);
			}

			renderOption.Selected = 0;
		}

		protected RenderMode GetRenderMode()
		{
			if (currentMajor < 4)
			{
				return (RenderMode)renderOption.Selected;
			}
			else
			{
				return (RenderMode)(renderOption.Selected + 0b100);
			}
		}
	}
}
