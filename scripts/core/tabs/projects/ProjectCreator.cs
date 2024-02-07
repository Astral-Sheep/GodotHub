using Com.Astral.GodotHub.Core.Data;
using Com.Astral.GodotHub.Core.Debug;
using Com.Astral.GodotHub.Core.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using Error = Com.Astral.GodotHub.Core.Utils.Error;
using FileAccess = Godot.FileAccess;
using Version = Com.Astral.GodotHub.Core.Data.Version;

namespace Com.Astral.GodotHub.Core.Tabs.Projects
{
	public static class ProjectCreator
	{
		private const string ICON_PATH = "assets/default_icon.svg";
		
		private static readonly Dictionary<RenderMode, (string feature, string method)> RenderModes = new() {
			{ RenderMode.OpenGL2, ("", "GLES2") },
			{ RenderMode.OpenGL3, ("", "GLES3") },
			{ RenderMode.Forward, ("Forward Plus", "forward_plus") },
			{ RenderMode.Mobile, ("Mobile", "mobile") },
			{ RenderMode.Compatibility, ("GL Compatibility", "gl_compatibility") }
		};

		/// <summary>
		/// Create a new project with the specified parameters
		/// </summary>
		/// <param name="pName">Name of the project</param>
		/// <param name="pDirectory">Path to the directory of the project</param>
		/// <param name="pVersion"><see cref="Version"/> of the project</param>
		/// <param name="pRenderMode"><see cref="RenderMode"/> of the project</param>
		/// <param name="pVersioningMode"><see cref="VersioningMode"/> of the project</param>
		public static void CreateProject(string pName, string pDirectory, Version pVersion, RenderMode pRenderMode, VersioningMode pVersioningMode)
		{
			if (pVersion.major < 4 != (((int)pRenderMode & (int)RenderMode.Config5) == 0))
			{
				Debugger.LogError($"Invalid version/render mode combination: {pVersion} | {pRenderMode}");
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
					ExceptionHandler.Singleton.LogException(lException);
					//Debugger.LogException(lException);
					return;
				}
			}

			Error lError;

			if (pVersion.major < 4)
			{
				lError = CreateProjectInternal4(pName, pDirectory, pVersion, pRenderMode, pVersioningMode);
			}
			else
			{
				lError = CreateProjectInternal5(pName, pDirectory, pVersion, pRenderMode, pVersioningMode);
			}

			if (!lError.Ok)
			{
				//To do: create error popup
				ExceptionHandler.Singleton.LogException(lError.Exception);
				//Debugger.LogException(lError.Exception);
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

		private static Error CreateProjectInternal5(string pName, string pDirectory, Version pVersion, RenderMode pRenderMode, VersioningMode pVersioningMode)
		{
			try
			{
				using (StreamWriter lWriter = new StreamWriter($"{pDirectory}/project.godot"))
				{
					bool lIsMono = InstallsData.VersionIsMono(pVersion);
					string lContent = 
						$"; Engine configuration file.{PathT.EOL}" +
						$"; It's best edited using the editor UI and not directly,{PathT.EOL}" +
						$"; since the parameters that go here are not all obvious.{PathT.EOL}" +
						$";{PathT.EOL}" +
						$"; Format:{PathT.EOL}" +
						$";   [section] ; section goes between []{PathT.EOL}" +
						$";   param=value ; assign values to parameters{PathT.EOL}{PathT.EOL}" +
						$"config_version=5{PathT.EOL}{PathT.EOL}" +
						$"[application]{PathT.EOL}{PathT.EOL}" +
						$"config/name=\"{pName}\"{PathT.EOL}" +
						$"config/features=PackedStringArray(\"{pVersion}\"{(lIsMono ? ", \"C#\"" : "")}, \"{RenderModes[pRenderMode].feature}\"){PathT.EOL}" +
						$"config/icon=\"res://icon.svg\"{PathT.EOL}";

					if (lIsMono)
					{
						lContent +=
							$"{PathT.EOL}[dotnet]{PathT.EOL}{PathT.EOL}" +
							$"project/assembly_name=\"{pName}\"{PathT.EOL}";
					}

					if (pRenderMode != RenderMode.Forward)
					{
						lContent +=
							$"{PathT.EOL}[rendering]{PathT.EOL}{PathT.EOL}" +
							$"renderer/rendering_method=\"{RenderModes[pRenderMode].method}\"{PathT.EOL}";

						if (pRenderMode == RenderMode.Compatibility)
						{
							lContent += $"renderer/rendering_method=\"{RenderModes[pRenderMode].method}\"{PathT.EOL}";
						}
					}

					lWriter.Write(lContent);
					lWriter.Close();
				}
			}
			catch (Exception lException)
			{
				return new Error(lException);
			}

			Error lError = CopyIcon(pDirectory);

			if (!lError.Ok)
			{
				return lError;
			}
			
			if (pVersioningMode != VersioningMode.None)
			{
				return CreateVersionningFiles(pDirectory, ".godot/");
			}

			return new Error();
		}

		private static Error CreateProjectInternal4(string pName, string pDirectory, Version pVersion, RenderMode pRenderMode, VersioningMode pVersioningMode)
		{
			try
			{
				using (StreamWriter lWriter = new StreamWriter($"{pDirectory}/default_env.tres"))
				{
					lWriter.Write(
						$"[gd_resource type=\"Environment\" load_steps=2 format=2]{PathT.EOL}" +
						$"[sub_resource type=\"ProceduralSky\" id=1]{PathT.EOL}" +
						$"[resource]{PathT.EOL}" +
						$"background_mode = 2{PathT.EOL}" +
						$"background_sky = SubResource( 1 ){PathT.EOL}"
					);
					lWriter.Close();
				}
			}
			catch (Exception lException)
			{
				return new Error(lException);
			}

			try
			{
				using (StreamWriter lWriter = new StreamWriter($"{pDirectory}/project.godot"))
				{
					string lContent =
						$"; Engine configuration file.{PathT.EOL}" +
						$"; It's best edited using the editor UI and not directly,{PathT.EOL}" +
						$"; since the parameters that go here are not all obvious.{PathT.EOL}" +
						$";{PathT.EOL}" +
						$"; Format:{PathT.EOL}" +
						$";   [section] ; section goes between []{PathT.EOL}" +
						$";   param=value ; assign values to parameters{PathT.EOL}{PathT.EOL}" +
						$"config_version=4{PathT.EOL}{PathT.EOL}" +
						$"[application]{PathT.EOL}{PathT.EOL}" +
						$"config/name=\"{pName}\"{PathT.EOL}" +
						$"config/icon=\"res://icon.png\"{PathT.EOL}{PathT.EOL}" +
						$"[physics]{PathT.EOL}{PathT.EOL}" +
						$"common/enable_pause_aware_picking=true{PathT.EOL}{PathT.EOL}" +
						$"[rendering]{PathT.EOL}{PathT.EOL}";

					if (pRenderMode != RenderMode.OpenGL3)
					{
						lContent +=
							$"quality/driver/driver_name=\"{RenderModes[pRenderMode].method}\"{PathT.EOL}" +
							$"vram_compression/import_etc=true{PathT.EOL}" +
							$"vram_compression/import_etc2=false{PathT.EOL}";
					}

					lContent += $"environment/default_environment=\"res://default_env.tres\"{PathT.EOL}";
					lWriter.Write(lContent);
					lWriter.Close();
				}
			}
			catch (Exception lException)
			{
				return new Error(lException);
			}

			Error lError = CopyIcon(pDirectory);

			if (!lError.Ok)
			{
				return lError;
			}

			if (pVersioningMode != VersioningMode.None)
			{
				return CreateVersionningFiles(pDirectory, ".import/", ".mono/");
			}

			return new Error();
		}

		private static Error CreateVersionningFiles(string pDirectory, params string[] pElementsToIgnore)
		{
			try
			{
				using (StreamWriter lWriter = new StreamWriter($"{pDirectory}/.gitignore"))
				{
					string lGitignore = $"# Godot files{PathT.EOL}";

					for (int i = 0; i < pElementsToIgnore.Length; i++)
					{
						lGitignore += $"{pElementsToIgnore[i]}{PathT.EOL}";
					}

					lWriter.Write(lGitignore);
					lWriter.Close();
				}

				using (StreamWriter lWriter = new StreamWriter($"{pDirectory}/.gitattributes"))
				{
					lWriter.Write(
						$"# Normalize EOL for all files that Git considers text files.{PathT.EOL}" +
#if GODOT_WINDOWS
						$"* text=auto eol=crlf{PathT.EOL}"
#elif GODOT_MACOS
						$"* text=auto eol=cr{PathT.EOL}"
#else
						$"* text=auto eol=lf{PathT.EOL}"
#endif
					);
					lWriter.Close();
				}
			}
			catch (Exception lException)
			{
				return new Error(lException);
			}

			return new Error();
		}

		/// <summary>
		/// Create a copy of the default project icon at <paramref name="pProjectPath"/>
		/// </summary>
		private static Error CopyIcon(string pProjectPath)
		{
			try
			{
				using (FileStream lFStream = new FileStream($"{pProjectPath}/icon.svg", FileMode.Create))
				{
					lFStream.Write(FileAccess.GetFileAsBytes($"res://{ICON_PATH}"));
					lFStream.Close();
				}
			}
			catch (Exception lException)
			{
				return new Error(lException);
			}

			return new Error();
		}
	}
}
