using Com.Astral.GodotHub.Data;
using Com.Astral.GodotHub.Tabs.Comparisons;
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
	public partial class ProjectItem : Control, IFavoriteItem, INamedItem, ITimedItem, IVersionItem, IValidItem
	{
		protected const string APPLICATION_SECTION = "application";
		protected const string NAME_KEY = "config/name";

		public event Action<ProjectItem> Closed;

		public bool IsFavorite { get; protected set; }
		public string ItemName { get; protected set; }
		public double TimeSinceLastOpening { get; protected set; }
		public Version Version => project.Version;
		public bool IsValid { get; protected set; } = true;

		[ExportGroup("Data")]
		[Export] protected RichTextLabel nameLabel;
		[Export] protected Label pathLabel;
		[Export] protected RichTextLabel lastOpenedLabel;

		[ExportGroup("Buttons")]
		[Export] protected Button favoriteToggle;
		[Export] protected OptionButton versionButton;
		[Export] protected Button openButton;
		[Export] protected Button removeButton;

		[ExportGroup("Removal")]
		[Export] protected float closeDuration = 0.25f;
		[Export] protected PackedScene confirmationPopupScene;

		protected GDFile project;
		protected string projectPath = "";

		public override void _Ready()
		{
			versionButton.GetPopup().TransparentBg = true;
		}

		protected override void Dispose(bool pDisposing)
		{
			if (pDisposing)
			{
				InstallsData.VersionAdded -= OnVersionAdded;
			}
		}

		public void Init(GDFile pProject)
		{
			project = pProject;
			projectPath = pProject.Path + "/project.godot";
			pathLabel.Text = pProject.Path;

			ConfigFile lProject = new ConfigFile();
			Error lError = lProject.Load(projectPath);

			if (lError == Error.Ok)
			{
				ItemName = (string)lProject.GetValue(APPLICATION_SECTION, NAME_KEY);
				nameLabel.Text = $"[b]{ItemName}[/b]";

				DateTime lTime = new DirectoryInfo(project.Path)
						.GetFiles()
						.OrderByDescending(f => f.LastWriteTimeUtc)
						.First().LastWriteTimeUtc;
				lastOpenedLabel.Text = TimeFormater.Format(lTime);
				TimeSinceLastOpening = (DateTime.UtcNow - lTime).TotalSeconds;

				IsFavorite = project.IsFavorite;
				favoriteToggle.ButtonPressed = project.IsFavorite;
				favoriteToggle.Toggled += OnFavoriteToggled;

				if (!SetVersion(pProject.Version))
				{
					Disable(false);
					Debugger.PrintError($"Can't find compatible engine for project {ItemName}");
				}

				openButton.Pressed += OnOpenPressed;
				InstallsData.VersionAdded += OnVersionAdded;
				InstallsData.VersionRemoved += OnVersionRemoved;
			}
			else
			{
				Disable(true);
			}

			removeButton.Pressed += OnRemovePressed;
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

		protected void Remove()
		{
			ProjectsData.RemoveProject(project.Path);
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

		protected void OnFavoriteToggled(bool pToggled)
		{
			ProjectsData.SetFavorite(project.Path, pToggled);
			IsFavorite = pToggled;
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

		protected void OnVersionAdded(GDFile pInstall)
		{
			if (!pInstall.Version.IsCompatible(project.Version))
				return;

			if (versionButton.ItemCount == 0)
			{
				versionButton.AddItem((string)pInstall.Version, 0);
				versionButton.Disabled = false;
				nameLabel.Text = $"[b]{ItemName}[/b]";
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
			versionButton.Clear();

			for (int i = 0; i < lVersions.Count; i++)
			{
				versionButton.AddItem((string)lVersions[i], i);
			}

			versionButton.Selected = lVersions.IndexOf(project.Version);
		}

		protected void OnVersionRemoved(Version pVersion)
		{
			bool lHasVersion = false;
			int lVersionIndex = -1;

			for (int i = 0; i < versionButton.ItemCount; i++)
			{
				if (versionButton.GetItemText(i) == (string)pVersion)
				{
					lHasVersion = true;
					lVersionIndex = i;
					break;
				}
			}

			if (!lHasVersion)
				return;

			for (int i = lVersionIndex; i < versionButton.ItemCount - 1; i++)
			{
				versionButton.SetItemText(i, versionButton.GetItemText(i + 1));
			}

			versionButton.RemoveItem(versionButton.ItemCount - 1);
		}
	}
}
