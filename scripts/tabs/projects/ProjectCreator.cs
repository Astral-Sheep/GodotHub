using Com.Astral.GodotHub.Data;
using Com.Astral.GodotHub.Debug;
using Godot;
using Godot.Collections;
using System;
using System.IO;

using FileAccess = Godot.FileAccess;
using Version = Com.Astral.GodotHub.Data.Version;

namespace Com.Astral.GodotHub.Tabs.Projects
{
	public static class ProjectCreator
	{
#if GODOT_WINDOWS
		private const string EOL = "\r\n";
#elif GODOT_MACOS
		private const string EOL = "\r";
#else
		private const string EOL = "\n";
#endif

		private static readonly Dictionary<RenderMode, string> RenderModes = new Dictionary<RenderMode, string>() {
			{ RenderMode.Forward, "forward_plus" },
			{ RenderMode.Mobile, "mobile" },
			{ RenderMode.Compatibility, "gl_compatibility" }
		};

		public static void CreateProject(string pName, string pDirectory, Version pVersion, RenderMode pRenderMode, VersionningMode pVersionningMode)
		{
			if (!Directory.Exists(pDirectory))
			{
				try
				{
					Directory.CreateDirectory(pDirectory);
				}
				catch (Exception lException)
				{
					Debugger.PrintException(lException);
					return;
				}
			}

			InitGodotFile(pName, pVersion, pDirectory, pRenderMode);

			if (pVersionningMode != VersionningMode.None)
			{
				InitVersionningFiles(pDirectory, pVersion);
			}

			ProjectsData.AddProject(new GDFile(pDirectory, false, pVersion));
		}

		private static void InitGodotFile(string pName, Version pVersion, string pFolderPath, RenderMode pRenderMode)
		{
			StreamWriter lWriter = new StreamWriter($"{pFolderPath}/project.godot");

			if (pVersion.major >= 4)
			{
				bool lIsMono = InstallsData.VersionIsMono(pVersion);

				lWriter.Write(
					$"; Engine configuration file.{EOL}" +
					$"; It's best edited using the editor UI and not directly,{EOL}" +
					$"; since the parameters that go here are not all obvious.{EOL}" +
					$";{EOL}" +
					$"; Format:{EOL}" +
					$";   [section] ; section goes between []{EOL}" +
					$";   param=value ; assign values to parameters{EOL}{EOL}" +
					$"config_version=5{EOL}{EOL}" +
					$"[application]{EOL}{EOL}" +
					$"config/name=\"{pName}\"{EOL}" +
					$"config/features=PackedStringArray(\"{pVersion}\"{(lIsMono ? ", \"C#\"" : "")}, \"{pRenderMode}\"){EOL}"/* +*/
					//$"config/icon=\"res://icon.svg\"{EOF}"
				);

				if (InstallsData.VersionIsMono(pVersion))
				{
					lWriter.Write(
						$"{EOL}[dotnet]{EOL}{EOL}" +
						$"project/assembly_name=\"{pName}\"{EOL}"
					);
				}

				if (pRenderMode != RenderMode.Forward)
				{
					lWriter.Write(
						$"{EOL}[rendering]{EOL}{EOL}" +
						$"rendering/rendering_method=\"{RenderModes[pRenderMode]}\"{EOL}"
					);
				}
			}
			else
			{

			}

			lWriter.Close();
		}

		private static void InitVersionningFiles(string pDir, Version pVersion)
		{
			if (pVersion.major >= 4)
			{
				StreamWriter lWriter = new StreamWriter($"{pDir}/.gitignore");
				lWriter.Write(
					$"# Godot files{EOL}" +
					$".godot/{EOL}"
				);
				lWriter.Close();

				lWriter = new StreamWriter($"{pDir}/.gitattributes");
				lWriter.Write(
					$"# Normalize EOL for all files that Git considers text files.{EOL}" +
#if GODOT_WINDOWS
					$"* text=auto eol=crlf{EOL}"
#elif GODOT_LINUXBSD
					$"* text=auto eol=lf{EOF}"
#else
					$"* text=auto eol=cr{EOF}"
#endif
				);
				lWriter.Close();
			}
			else
			{
				//To do
			}
		}

		public static void AddIcon(string pProjectPath)
		{
			CompressedTexture2D lIcon = ResourceLoader.Load<CompressedTexture2D>("res://icon.svg");
			//lIcon.TakeOverPath(pProjectPath + "/icon.png");
			Stream lFile = File.OpenWrite($"{pProjectPath}/icon.png");
			lFile.Write(lIcon.GetImage().GetData());
			lFile.Close();
			//ResourceSaver.Save(lIcon, $"{pProjectPath}/icon.svg", ResourceSaver.SaverFlags.None);
			//FileAccess lFile = FileAccess.Open($"{pProjectPath}/icon.svg", FileAccess.ModeFlags.Write);
			//File.Copy(ProjectSettings.GlobalizePath("res://icon.svg"), $"{pProjectPath}/icon.svg");
		}
	}
}
