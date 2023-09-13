using Godot;
using System;
using System.Diagnostics;
using System.IO;
using Debugger = Com.Astral.GodotHub.Debug.Debugger;

namespace Com.Astral.GodotHub.Tabs.Projects
{
	public partial class ProjectItem : Control
	{
		[Export] protected Button favoriteToggle;
		[Export] protected RichTextLabel nameLabel;
		[Export] protected Label pathLabel;
		[Export] protected Label lastOpenedLabel;
		[Export] protected OptionButton versionButton;
		[Export] protected Button openButton;
		[Export] protected Button removeButton;

		protected string projectPath = "";
		protected string projectDirectory = "";
		protected string enginePath = "";

		public void Init(string pPath)
		{
			pathLabel.Text = pPath;
			projectDirectory = pPath;
			projectPath = pPath + "/project.godot";
			versionButton.GetPopup().TransparentBg = true;

			SetVersion();

			ConfigFile lProject = new ConfigFile();
			Error lError = lProject.Load(projectPath);

			if (lError == Error.Ok)
			{
				versionButton.Disabled = false;
				openButton.Disabled = false;

				nameLabel.Text = $"[b]{lProject.GetValue("application", "config/name")}[/b]";
				lastOpenedLabel.Text = GetElapsedTime(
					new FileInfo(projectPath).LastAccessTimeUtc
				);

				openButton.Pressed += OnOpenPressed;

				lError = lProject.Load(projectDirectory + "/.godot/editor/project_metadata.cfg");

				if (lError != Error.Ok)
				{
					Disable(false);
					Debugger.PrintError($"Can't find version of project {lProject.GetValue("application", "config/name")}");
				}
			}
			else
			{
				nameLabel.Text = $"[b][color=#{Colors.ToHexa(Colors.Singleton.Red)}]Missing project[/color][/b]";
				Disable(true);
			}
		}

		protected void Disable(bool pMissing)
		{
			if (pMissing)
			{
				nameLabel.Text = $"[color=#{Colors.ToHexa(Colors.Singleton.Red)}][b]Missing project[/b][/color]";
			}
			else
			{
				nameLabel.Text = $"[color=#{Colors.ToHexa(Colors.Singleton.Red)}]{nameLabel.Text}[/color]";
			}

			lastOpenedLabel.Text = "";
			versionButton.Disabled = true;
			openButton.Disabled = true;
		}

		protected Error SetVersion()
		{
			ConfigFile lProjectData = new ConfigFile();

			if (File.Exists(projectDirectory + "/.godot/editor/project_metadata.cfg"))
			{
				//4.0 and above
				Error lError = lProjectData.Load(projectDirectory + "/.godot/editor/project_metadata.cfg");

				if (lError != Error.Ok)
					return lError;

				string lEngine = (string)lProjectData.GetValue("editor_metadata", "executable_path");
				string lVersion = lEngine[(lEngine.RFind("Godot_v") + 7)..lEngine.RFind("-stable")];
				Debugger.PrintMessage($"Version: {lVersion}");
			}
			else
			{

			}

			return Error.Ok;
		}

		protected void OnOpenPressed()
		{
			if (!File.Exists(projectPath))
				return;

			Process.Start(new ProcessStartInfo() {
				FileName = "C:/Users/thoma/Downloads/Godot_v4.0.4-stable_mono_win64/Godot_v4.0.4-stable_mono_win64.exe",
				WorkingDirectory = projectDirectory,
				Arguments = "--editor"
			});
		}

		protected void OnRemovePressed()
		{

		}

		protected string GetElapsedTime(DateTime pTime)
		{
			DateTime lCurrentTime = DateTime.UtcNow;
			return "";
		}
	}
}
