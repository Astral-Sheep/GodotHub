using Com.Astral.GodotHub.Data;
using Godot;
using System;
using System.Diagnostics;
using System.IO;

using Debugger = Com.Astral.GodotHub.Debug.Debugger;
using OS = Com.Astral.GodotHub.Data.OS;
using Version = Com.Astral.GodotHub.Data.Version;

namespace Com.Astral.GodotHub.Tabs.Installs
{
	public partial class InstallItem : Control
	{
		public Version Version { get; protected set; }
		public int Index { get; protected set; }

		[Export] protected RichTextLabel nameLabel;
		[Export] protected Label pathLabel;
		[Export] protected CheckBox isMonoBox;
		[Export] protected Button openButton;
		[Export] protected Button uninstallButton;

		public void Init(Project pInstall, int pIndex)
		{
			Version = pInstall.Version;
			Index = pIndex;
			pathLabel.Text = pInstall.Path;

			if (!File.Exists(pInstall.Path))
			{
				nameLabel.Text = $"[color=#{Colors.ToHexa(Colors.Singleton.Red)}][b]Missing version[/b][/color]";
				isMonoBox.ButtonPressed = false;
				openButton.Disabled = true;

				//Connect to remove from file
				uninstallButton.Disabled = true;
				return;
			}


			if (Config.os == OS.MacOS)
			{
				//To do
				//Fuck you
			}
			else
			{
				string lExecutable = pInstall.Path[(pInstall.Path.RFind("/") + 1)..];
				nameLabel.Text = $"[b]Godot {pInstall.Version}[/b]";
				isMonoBox.ButtonPressed = lExecutable.Contains("mono");
			}

			openButton.Pressed += OnOpenPressed;
		}

		protected void OnOpenPressed()
		{
			try
			{
				Process.Start(new ProcessStartInfo() {
					FileName = pathLabel.Text,
					WorkingDirectory = pathLabel.Text[..pathLabel.Text.RFind("/")],
					Arguments = "--project-manager"
				});
			}
			catch (Exception lException)
			{
				Debugger.PrintException(lException);
			}
		}
	}
}
