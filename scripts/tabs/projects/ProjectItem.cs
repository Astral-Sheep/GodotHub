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
		protected const string APPLICATION_SECTION = "application";
		protected const string NAME_KEY = "config/name";

		[Export] protected Button favoriteToggle;
		[Export] protected RichTextLabel nameLabel;
		[Export] protected Label pathLabel;
		[Export] protected RichTextLabel lastOpenedLabel;
		[Export] protected OptionButton versionButton;
		[Export] protected Button openButton;
		[Export] protected Button removeButton;

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

		}
	}
}
