using Com.Astral.GodotHub.Data;
using Godot;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

using Debugger = Com.Astral.GodotHub.Debug.Debugger;
using Version = Com.Astral.GodotHub.Data.Version;

namespace Com.Astral.GodotHub.Tabs.Projects
{
	public partial class ProjectItem : Control
	{
		public static event Action<ProjectItem> Closed;

		protected const string APPLICATION_SECTION = "application";
		protected const string NAME_KEY = "config/name";

		[Export] protected Button favoriteToggle;
		[Export] protected RichTextLabel nameLabel;
		[Export] protected Label pathLabel;
		[Export] protected RichTextLabel lastOpenedLabel;
		[Export] protected OptionButton versionButton;
		[Export] protected Button openButton;
		[Export] protected Button removeButton;

		[ExportGroup("Remove")]
		[Export] protected float closeDuration = 0.25f;
		[Export] protected PackedScene confirmationPopup;

		protected GDFile project;
		protected string projectPath = "";
		protected string enginePath = "";

		public void Init(GDFile pProject)
		{
			project = pProject;
			versionButton.GetPopup().TransparentBg = true;
			pathLabel.Text = pProject.Path;
			projectPath = pProject.Path + "/project.godot";

			ConfigFile lProject = new ConfigFile();
			Error lError = lProject.Load(projectPath);

			if (lError == Error.Ok)
			{
				nameLabel.Text = $"[b]{lProject.GetValue(APPLICATION_SECTION, NAME_KEY)}[/b]";
				lastOpenedLabel.Text = GetElapsedTime(
					new FileInfo(projectPath).LastAccessTimeUtc
				);
				favoriteToggle.Toggled += OnFavoriteToggled;

				if (SetVersion(pProject.Version))
				{
					openButton.Pressed += OnOpenPressed;
				}
				else
				{
					Disable(false);
					Debugger.PrintError($"Can't find compatible engine for project {lProject.GetValue(APPLICATION_SECTION, NAME_KEY)}");
				}
			}
			else
			{
				Disable(true);
			}

			removeButton.Pressed += OnRemovePressed;
		}

		protected string GetElapsedTime(DateTime pTime)
		{
			//DateTime lCurrentTime = DateTime.UtcNow;
			return $"[color=#{Colors.ToHexa(Colors.Singleton.Red)}]N/A[/color]";
		}

		protected bool SetVersion(Version pVersion)
		{
			List<Version> lCompatibleVersions = InstallsData.GetCompatibleVersions(pVersion);

			if (lCompatibleVersions.Count == 0)
				return false;

			lCompatibleVersions.Sort();
			Version lVersion;

			for (int i = 0; i < lCompatibleVersions.Count; i++)
			{
				lVersion = lCompatibleVersions[i];
				versionButton.AddItem((string)lVersion, i);

				if (lVersion == pVersion)
				{
					versionButton.Selected = i;
				}
			}

			return true;
		}

		protected void Disable(bool pMissing)
		{
			if (pMissing)
			{
				nameLabel.Text = $"[color=#{Colors.ToHexa(Colors.Singleton.Red)}][b]Missing project[/b][/color]";
				lastOpenedLabel.Text = $"[color=#{Colors.ToHexa(Colors.Singleton.Red)}]N/A[/color]";
			}
			else
			{
				nameLabel.Text = $"[color=#{Colors.ToHexa(Colors.Singleton.Red)}]{nameLabel.Text}[/color]";
			}

			versionButton.Disabled = true;
			openButton.Disabled = true;
		}

		protected void OnFavoriteToggled(bool pToggled)
		{
			ProjectsData.SetFavorite(projectPath, pToggled);
		}

		protected void OnOpenPressed()
		{
			if (!File.Exists(projectPath))
				return;

			try
			{
				Process.Start(new ProcessStartInfo() {
					FileName = InstallsData.GetPath(versionButton.Text),
					WorkingDirectory = project.Path,
					Arguments = "--editor",
				});
			}
			catch (Exception lException)
			{
				Debugger.PrintException(lException);
			}
		}

		protected void OnRemovePressed()
		{
			ConfirmationDialog lDialog = confirmationPopup.Instantiate<ConfirmationDialog>();
			Main.Instance.AddChild(lDialog);
			lDialog.Title = "Remove";
			lDialog.DialogText = "Do you really want to remove this project?";
			lDialog.GetChild<Label>(1, true).HorizontalAlignment = HorizontalAlignment.Center;
			lDialog.PopupCentered();
			lDialog.Confirmed += Remove;
		}

		protected void Remove()
		{
			ProjectsData.RemoveProject(projectPath);
			Close();
		}

		protected void Close()
		{
			CreateTween()
				.SetTrans(Tween.TransitionType.Quad)
				.SetEase(Tween.EaseType.Out)
				.TweenProperty(this, "custom_minimum_size:y", 0f, closeDuration)
				.Finished += QueueFree;
			Closed?.Invoke(this);
		}
	}
}
