using Com.Astral.GodotHub.Data;
using Com.Astral.GodotHub.Debug;
using Godot;
using System;
using System.Collections.Generic;
using System.IO;

using Error = Com.Astral.GodotHub.Data.Error;
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

		private static readonly Dictionary<RenderMode, (string feature, string method)> RenderModes = new() {
			{ RenderMode.OpenGL2, ("", "GLES2") },
			{ RenderMode.OpenGL3, ("", "GLES3") },
			{ RenderMode.Forward, ("Forward Plus", "forward_plus") },
			{ RenderMode.Mobile, ("Mobile", "mobile") },
			{ RenderMode.Compatibility, ("GL Compatibility", "gl_compatibility") }
		};

		public static void CreateProject(string pName, string pDirectory, Version pVersion, RenderMode pRenderMode, VersionningMode pVersionningMode)
		{
			if (pVersion.major < 4 != (((int)pRenderMode & (int)RenderMode.Config5) == 0))
			{
				Debugger.PrintMessage($"{pVersion.major < 4} == {(((int)pRenderMode & (int)RenderMode.Config5) == 0)}");
				Debugger.PrintMessage($"Invalid version/render mode combination: {pVersion} | {pRenderMode}");
				return;
			}

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

			Error lError;

			if (pVersion.major < 4)
			{
				lError = CreateProjectInternal4(pName, pDirectory, pVersion, pRenderMode, pVersionningMode);
			}
			else
			{
				lError = CreateProjectInternal5(pName, pDirectory, pVersion, pRenderMode, pVersionningMode);
			}

			if (!lError.Ok)
			{
				Debugger.PrintException(lError.exception);
			}
			else
			{
				ProjectsData.AddProject(new GDFile(
					pDirectory,
					false,
					pVersion
				));
			}
		}

		/// <summary>
		/// Create a godot project with <code>config_version=5</code>
		/// </summary>
		/// <param name="pName">Name of the project</param>
		/// <param name="pDirectory">Path to the directory of the project</param>
		/// <param name="pVersion">Version of the project (throw error if under 4.x)</param>
		/// <param name="pRenderMode">Render mode of the project (between Forward+, Mobile and Compatibility)</param>
		private static Error CreateProjectInternal5(string pName, string pDirectory, Version pVersion, RenderMode pRenderMode, VersionningMode pVersionningMode)
		{
			try
			{
				StreamWriter lWriter = new StreamWriter($"{pDirectory}/project.godot");
				bool lIsMono = InstallsData.VersionIsMono(pVersion);
				string lContent = 
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
					$"config/features=PackedStringArray(\"{pVersion}\"{(lIsMono ? ", \"C#\"" : "")}, \"{RenderModes[pRenderMode].feature}\"){EOL}" +
					$"config/icon=\"res://icon.svg\"{EOL}";

				if (lIsMono)
				{
					lContent +=
						$"{EOL}[dotnet]{EOL}{EOL}" +
						$"project/assembly_name=\"{pName}\"{EOL}";
				}

				if (pRenderMode != RenderMode.Forward)
				{
					lContent +=
						$"{EOL}[rendering]{EOL}{EOL}" +
						$"renderer/rendering_method=\"{RenderModes[pRenderMode].method}\"{EOL}";

					if (pRenderMode == RenderMode.Compatibility)
					{
						lContent += $"renderer/rendering_method=\"{RenderModes[pRenderMode].method}\"{EOL}";
					}
				}

				lWriter.Write(lContent);
				lWriter.Close();
			}
			catch (Exception lException)
			{
				return new Error(lException);
			}

			if (pVersionningMode != VersionningMode.None)
			{
				return CreateVersionningFiles(pDirectory, ".godot/");
			}

			return new Error();
		}

		private static Error CreateProjectInternal4(string pName, string pDirectory, Version pVersion, RenderMode pRenderMode, VersionningMode pVersionningMode)
		{
			StreamWriter lWriter;

			try
			{
				lWriter = new StreamWriter($"{pDirectory}/default_env.tres");
				lWriter.Write(
					$"[gd_resource type=\"Environment\" load_steps=2 format=2]{EOL}" +
					$"[sub_resource type=\"ProceduralSky\" id=1]{EOL}" +
					$"[resource]{EOL}" +
					$"background_mode = 2{EOL}" +
					$"background_sky = SubResource( 1 ){EOL}"
				);
				lWriter.Close();
			}
			catch (Exception lException)
			{
				return new Error(lException);
			}

			try
			{
				lWriter = new StreamWriter($"{pDirectory}/project.godot");
				string lContent =
					$"; Engine configuration file.{EOL}" +
					$"; It's best edited using the editor UI and not directly,{EOL}" +
					$"; since the parameters that go here are not all obvious.{EOL}" +
					$";{EOL}" +
					$"; Format:{EOL}" +
					$";   [section] ; section goes between []{EOL}" +
					$";   param=value ; assign values to parameters{EOL}{EOL}" +
					$"config_version=4{EOL}{EOL}" +
					$"[application]{EOL}{EOL}" +
					$"config/name=\"{pName}\"{EOL}" +
					$"config/icon=\"res://icon.png\"{EOL}{EOL}" +
					$"[physics]{EOL}{EOL}" +
					$"common/enable_pause_aware_picking=true{EOL}{EOL}" +
					$"[rendering]{EOL}{EOL}";


				if (pRenderMode != RenderMode.OpenGL3)
				{
					lContent +=
						$"quality/driver/driver_name=\"{RenderModes[pRenderMode].method}\"{EOL}" +
						$"vram_compression/import_etc=true{EOL}" +
						$"vram_compression/import_etc2=false{EOL}";
				}

				lContent += $"environment/default_environment=\"res://default_env.tres\"{EOL}";
				lWriter.Write(lContent);
				lWriter.Close();
			}
			catch (Exception lException)
			{
				return new Error(lException);
			}

			if (pVersionningMode != VersionningMode.None)
			{
				return CreateVersionningFiles(pDirectory, ".import/", ".mono/");
			}

			return new Error();
		}

		private static Error CreateVersionningFiles(string pDirectory, params string[] pElementsToIgnore)
		{
			try
			{
				string lGitignore = $"# Godot files{EOL}";

				for (int i = 0; i < pElementsToIgnore.Length; i++)
				{
					lGitignore += $"{pElementsToIgnore[i]}{EOL}";
				}

				StreamWriter lWriter = new StreamWriter($"{pDirectory}/.gitignore");
				lWriter.Write(lGitignore);
				lWriter.Close();

				lWriter = new StreamWriter($"{pDirectory}/.gitattributes");
				lWriter.Write(
					$"# Normalize EOL for all files that Git considers text files.{EOL}" +
	#if GODOT_WINDOWS
					$"* text=auto eol=crlf{EOL}"
	#elif GODOT_MACOS
					$"* text=auto eol=cr{EOF}"
	#else
					$"* text=auto eol=lf{EOF}"
	#endif
				);
				lWriter.Close();
			}
			catch (Exception lException)
			{
				return new Error(lException);
			}

			return new Error();
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
