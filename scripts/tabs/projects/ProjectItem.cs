using Com.Astral.GodotHub.Data;
using Godot;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
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
		[Export] protected PackedScene confirmationPopupScene;

		protected GDFile project;
		protected string projectPath = "";
		protected string projectName = "";
		protected string enginePath = "";

		protected override void Dispose(bool pDisposing)
		{
			if (pDisposing)
			{
				InstallsData.VersionAdded -= UpdateVersion;
			}
		}

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
				projectName = (string)lProject.GetValue(APPLICATION_SECTION, NAME_KEY);
				nameLabel.Text = $"[b]{projectName}[/b]";
				lastOpenedLabel.Text = GetElapsedTime(
					new DirectoryInfo(project.Path)
						.GetFiles()
						.OrderByDescending(f => f.LastWriteTimeUtc)
						.First().LastWriteTimeUtc
				);
				favoriteToggle.Toggled += OnFavoriteToggled;

				if (!SetVersion(pProject.Version))
				{
					Disable(false);
					Debugger.PrintError($"Can't find compatible engine for project {lProject.GetValue(APPLICATION_SECTION, NAME_KEY)}");
				}

				openButton.Pressed += OnOpenPressed;
				InstallsData.VersionAdded += UpdateVersion;
			}
			else
			{
				Disable(true);
			}

			removeButton.Pressed += OnRemovePressed;
		}

		protected string GetElapsedTime(DateTime pTime)
		{
			DateTime lCurrentTime = DateTime.UtcNow;
			TimeSpan lDifferenceSpan = lCurrentTime - pTime;

			//To refactor: i don't think it needs an explanation of why
			if (pTime.Year < lCurrentTime.Year)
			{
				float lDifference = lCurrentTime.Year - pTime.Year;

				if (lDifference == 1 && lCurrentTime.Month < pTime.Month)
				{
					return $"{pTime.Month - lCurrentTime.Month} months ago";
				}
				else
				{
					return $"{lDifference} year{(lDifference > 1 ? "s" : "")} ago";
				}
			}
			else if (pTime.Month < lCurrentTime.Month)
			{
				float lDifference = lCurrentTime.Month - pTime.Month;

				if (lDifference == 1 && lCurrentTime.Day < pTime.Day)
				{
					return $"{lDifferenceSpan.Days} days ago";
				}
				else
				{
					return $"{lDifference} month{(lDifference > 1 ? "s" : "")} ago";
				}
			}
			else if (pTime.Day < lCurrentTime.Day)
			{
				if (lDifferenceSpan.Hours < 24)
				{
					return $"{lDifferenceSpan.Hours} hours ago";
				}
				else
				{
					return $"{lDifferenceSpan.Days} day{(lDifferenceSpan.Days > 1 ? "s" : "")} ago";
				}
			}
			else if (pTime.Hour < lCurrentTime.Hour)
			{
				if (lDifferenceSpan.Minutes < 60)
				{
					return $"{lDifferenceSpan.Minutes} minutes ago";
				}
				else
				{
					return $"{lDifferenceSpan.Hours} hour{(lDifferenceSpan.Hours > 1 ? "s" : "")} ago";
				}
			}
			else if (pTime.Minute < lCurrentTime.Minute)
			{
				if (lDifferenceSpan.Seconds < 60)
				{
					return $"{lDifferenceSpan.Seconds} seconds ago";
				}
				else
				{
					return $"{lDifferenceSpan.Minutes} minute{(lDifferenceSpan.Minutes > 1 ? "s" : "")} ago";
				}
			}
			else if (pTime.Second < lCurrentTime.Second)
			{
				return $"{lDifferenceSpan.Seconds} second{(lDifferenceSpan.Seconds > 1 ? "s" : "")} ago";
			}
			else
			{
				return "Now";
			}
		}

		protected bool SetVersion(Version pVersion)
		{
			List<Version> lCompatibleVersions = InstallsData.GetCompatibleVersions(pVersion);

			if (lCompatibleVersions.Count == 0)
				return false;

			lCompatibleVersions.Sort();
			lCompatibleVersions.Reverse();
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

		protected void UpdateVersion(GDFile pInstall)
		{
			if (!pInstall.Version.IsCompatible(project.Version))
				return;

			if (versionButton.ItemCount == 0)
			{
				versionButton.AddItem((string)pInstall.Version, 0);
				versionButton.Disabled = false;
				nameLabel.Text = $"[b]{projectName}[/b]";
				return;
			}

			List<Version> lVersions = new List<Version>() { pInstall.Version };
			lVersions.Reverse();

			for (int i = 0; i < versionButton.ItemCount; i++)
			{
				lVersions.Add((Version)versionButton.GetItemText(i));
			}

			lVersions.Sort();

			for (int i = 0; i < lVersions.Count; i++)
			{
				versionButton.AddItem((string)lVersions[i], i);
			}

			versionButton.Selected = lVersions.IndexOf(project.Version);
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
			ConfirmationDialog lDialog = confirmationPopupScene.Instantiate<ConfirmationDialog>();
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
