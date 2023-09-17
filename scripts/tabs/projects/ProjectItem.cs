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

		public string ProjectName { get; protected set; }
		public double TimeSinceLastOpening { get; protected set; }
		public Version Version => project.Version;
		public bool IsFavorite => project.IsFavorite;
		public bool IsValid { get; protected set; } = true;

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
				ProjectName = (string)lProject.GetValue(APPLICATION_SECTION, NAME_KEY);
				nameLabel.Text = $"[b]{ProjectName}[/b]";

				DateTime lTime = new DirectoryInfo(project.Path)
						.GetFiles()
						.OrderByDescending(f => f.LastWriteTimeUtc)
						.First().LastWriteTimeUtc;
				lastOpenedLabel.Text = GetElapsedTime(lTime);
				TimeSinceLastOpening = (DateTime.UtcNow - lTime).TotalSeconds;
				favoriteToggle.Toggled += OnFavoriteToggled;
				favoriteToggle.ButtonPressed = project.IsFavorite;

				if (!SetVersion(pProject.Version))
				{
					Disable(false);
					Debugger.PrintError($"Can't find compatible engine for project {ProjectName}");
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
				int lDifference = lCurrentTime.Year - pTime.Year;

				if (lDifference == 1 && lCurrentTime.Month < pTime.Month)
				{
					return FormatTime(pTime.Month - lCurrentTime.Month, "month");
				}
				else
				{
					return FormatTime(lDifference, "year");
				}
			}
			else if (pTime.Month < lCurrentTime.Month)
			{
				int lDifference = lCurrentTime.Month - pTime.Month;

				if (lDifference == 1 && lCurrentTime.Day < pTime.Day)
				{
					return FormatTime(lDifferenceSpan.Days, "day");
				}
				else
				{
					return FormatTime(lDifference, "month");
				}
			}
			else if (pTime.Day < lCurrentTime.Day)
			{
				if (lDifferenceSpan.TotalHours < 24)
				{
					return FormatTime(lDifferenceSpan.Hours, "hour");
				}
				else
				{
					return FormatTime(lDifferenceSpan.Days, "day");
				}
			}
			else if (pTime.Hour < lCurrentTime.Hour)
			{
				if (lDifferenceSpan.TotalMinutes < 60)
				{
					return FormatTime(lDifferenceSpan.Minutes, "minute");
				}
				else
				{
					return FormatTime(lDifferenceSpan.Hours, "hour");
				}
			}
			else if (pTime.Minute < lCurrentTime.Minute)
			{
				if (lDifferenceSpan.TotalSeconds < 60)
				{
					return FormatTime(lDifferenceSpan.Seconds, "second");
				}
				else
				{
					return FormatTime(lDifferenceSpan.Minutes, "minute");
				}
			}
			else if (pTime.Second < lCurrentTime.Second)
			{
				return FormatTime(lDifferenceSpan.Seconds, "second");
			}
			else
			{
				return "Now";
			}
		}

		protected string FormatTime(int pTime, string pSuffix)
		{
			return $"{pTime} {pSuffix}{(pTime > 1 ? "s" : "")} ago";
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
				nameLabel.Text = $"[b]{ProjectName}[/b]";
				IsValid = true;
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

			IsValid = false;
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
