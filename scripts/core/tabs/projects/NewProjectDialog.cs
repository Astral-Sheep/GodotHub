using Com.Astral.GodotHub.Core.Data;
using Com.Astral.GodotHub.Core.Debug;
using Com.Astral.GodotHub.Core.Utils;
using Godot;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using Colors = Com.Astral.GodotHub.Core.Utils.Colors;
using Version = Com.Astral.GodotHub.Core.Data.Version;

namespace Com.Astral.GodotHub.Core.Tabs.Projects
{
	public partial class NewProjectDialog : ConfirmationDialog
	{
		/// <summary>
		/// Event called when the ok button is pressed
		/// <code>
		/// string:	Project name<br/>
		/// string: Project directory<br/>
		/// Version: Project engine version<br/>
		/// RenderMode: Project render mode (can change depending on the version)<br/>
		/// VersioningMode: Project version control type<br/>
		/// </code>
		/// </summary>
		public new event Action<string, string, Version, RenderMode, VersioningMode> Confirmed;

		[ExportGroup("Project name")]
		[Export] protected LineEdit projectName;
		[Export] protected LineEdit projectDirectory;
		[Export] protected Button folderCreateButton;
		[Export] protected Button browseButton;
		[ExportSubgroup("Error handling")]
		[Export] protected RichTextLabel nameErrorLabel;
		[Export] protected RichTextLabel pathErrorLabel;

		[ExportGroup("Project version")]
		[Export] protected OptionButton versionOption;

		[ExportGroup("Renderer options")]
		[Export] protected OptionButton renderOption;

		[ExportGroup("Versioning options")]
		[Export] protected OptionButton versioningOption;

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

			versioningOption.AddItem(VersioningMode.None.ToString(), (int)VersioningMode.None);
			versioningOption.AddItem(VersioningMode.Git.ToString(), (int)VersioningMode.Git);
			versioningOption.Selected = 0;

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
				(VersioningMode)versioningOption.Selected
			);
		}

		[SuppressMessage("ReSharper", "ConvertIfStatementToConditionalTernaryExpression")]
		protected void OnNameTextChanged(string pName)
		{
			if (string.IsNullOrEmpty(pName))
			{
				nameErrorLabel.Text = BBCodeT.GetColoredText("Invalid name: empty name", Colors.Singleton.Red);
			}
			else
			{
				nameErrorLabel.Text = "";
			}
		}

		protected void OnDirectoryTextChanged(string pPath)
		{
			if (!Directory.Exists(pPath))
			{
				pathErrorLabel.Text = BBCodeT.GetColoredText("Given folder does not exist", Colors.Singleton.Red);
			}
			else if (Directory.GetFiles(pPath).Length > 0)
			{
				pathErrorLabel.Text = BBCodeT.GetColoredText("Given folder is not empty", Colors.Singleton.Yellow);
			}
			else
			{
				pathErrorLabel.Text = "";
			}
		}

		protected void OnFolderCreatePressed()
		{
			string lPath = $"{projectDirectory.Text}/{projectName.Text}";

			if (Directory.Exists(lPath))
			{
				ExceptionHandler.Singleton.LogMessage(
					$"{projectDirectory.Text} directory already exists",
					"Directory already exists",
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
