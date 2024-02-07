using Com.Astral.GodotHub.Core.Data;
using Com.Astral.GodotHub.Core.Debug;
using Com.Astral.GodotHub.Core.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Error = Com.Astral.GodotHub.Core.Utils.Error;
using Version = Com.Astral.GodotHub.Core.Data.Version;

namespace Com.Astral.GodotHub.Core.Tabs.Projects
{
	public static class ProjectCreator
	{
		// Don't question it, trying to recover the binary data from the project icon is driving me insane
		private const string ICON_DATA = "<svg height=\"128\" width=\"128\" xmlns=\"http://www.w3.org/2000/svg\"><g transform=\"translate(32 32)\"><path d=\"m-16-32c-8.86 0-16 7.13-16 15.99v95.98c0 8.86 7.13 15.99 16 15.99h96c8.86 0 16-7.13 16-15.99v-95.98c0-8.85-7.14-15.99-16-15.99z\" fill=\"#363d52\"/><path d=\"m-16-32c-8.86 0-16 7.13-16 15.99v95.98c0 8.86 7.13 15.99 16 15.99h96c8.86 0 16-7.13 16-15.99v-95.98c0-8.85-7.14-15.99-16-15.99zm0 4h96c6.64 0 12 5.35 12 11.99v95.98c0 6.64-5.35 11.99-12 11.99h-96c-6.64 0-12-5.35-12-11.99v-95.98c0-6.64 5.36-11.99 12-11.99z\" fill-opacity=\".4\"/></g><g stroke-width=\"9.92746\" transform=\"matrix(.10073078 0 0 .10073078 12.425923 2.256365)\"><path d=\"m0 0s-.325 1.994-.515 1.976l-36.182-3.491c-2.879-.278-5.115-2.574-5.317-5.459l-.994-14.247-27.992-1.997-1.904 12.912c-.424 2.872-2.932 5.037-5.835 5.037h-38.188c-2.902 0-5.41-2.165-5.834-5.037l-1.905-12.912-27.992 1.997-.994 14.247c-.202 2.886-2.438 5.182-5.317 5.46l-36.2 3.49c-.187.018-.324-1.978-.511-1.978l-.049-7.83 30.658-4.944 1.004-14.374c.203-2.91 2.551-5.263 5.463-5.472l38.551-2.75c.146-.01.29-.016.434-.016 2.897 0 5.401 2.166 5.825 5.038l1.959 13.286h28.005l1.959-13.286c.423-2.871 2.93-5.037 5.831-5.037.142 0 .284.005.423.015l38.556 2.75c2.911.209 5.26 2.562 5.463 5.472l1.003 14.374 30.645 4.966z\" fill=\"#fff\" transform=\"matrix(4.162611 0 0 -4.162611 919.24059 771.67186)\"/><path d=\"m0 0v-47.514-6.035-5.492c.108-.001.216-.005.323-.015l36.196-3.49c1.896-.183 3.382-1.709 3.514-3.609l1.116-15.978 31.574-2.253 2.175 14.747c.282 1.912 1.922 3.329 3.856 3.329h38.188c1.933 0 3.573-1.417 3.855-3.329l2.175-14.747 31.575 2.253 1.115 15.978c.133 1.9 1.618 3.425 3.514 3.609l36.182 3.49c.107.01.214.014.322.015v4.711l.015.005v54.325c5.09692 6.4164715 9.92323 13.494208 13.621 19.449-5.651 9.62-12.575 18.217-19.976 26.182-6.864-3.455-13.531-7.369-19.828-11.534-3.151 3.132-6.7 5.694-10.186 8.372-3.425 2.751-7.285 4.768-10.946 7.118 1.09 8.117 1.629 16.108 1.846 24.448-9.446 4.754-19.519 7.906-29.708 10.17-4.068-6.837-7.788-14.241-11.028-21.479-3.842.642-7.702.88-11.567.926v.006c-.027 0-.052-.006-.075-.006-.024 0-.049.006-.073.006v-.006c-3.872-.046-7.729-.284-11.572-.926-3.238 7.238-6.956 14.642-11.03 21.479-10.184-2.264-20.258-5.416-29.703-10.17.216-8.34.755-16.331 1.848-24.448-3.668-2.35-7.523-4.367-10.949-7.118-3.481-2.678-7.036-5.24-10.188-8.372-6.297 4.165-12.962 8.079-19.828 11.534-7.401-7.965-14.321-16.562-19.974-26.182 4.4426579-6.973692 9.2079702-13.9828876 13.621-19.449z\" fill=\"#478cbf\" transform=\"matrix(4.162611 0 0 -4.162611 104.69892 525.90697)\"/><path d=\"m0 0-1.121-16.063c-.135-1.936-1.675-3.477-3.611-3.616l-38.555-2.751c-.094-.007-.188-.01-.281-.01-1.916 0-3.569 1.406-3.852 3.33l-2.211 14.994h-31.459l-2.211-14.994c-.297-2.018-2.101-3.469-4.133-3.32l-38.555 2.751c-1.936.139-3.476 1.68-3.611 3.616l-1.121 16.063-32.547 3.138c.015-3.498.06-7.33.06-8.093 0-34.374 43.605-50.896 97.781-51.086h.066.067c54.176.19 97.766 16.712 97.766 51.086 0 .777.047 4.593.063 8.093z\" fill=\"#478cbf\" transform=\"matrix(4.162611 0 0 -4.162611 784.07144 817.24284)\"/><path d=\"m0 0c0-12.052-9.765-21.815-21.813-21.815-12.042 0-21.81 9.763-21.81 21.815 0 12.044 9.768 21.802 21.81 21.802 12.048 0 21.813-9.758 21.813-21.802\" fill=\"#fff\" transform=\"matrix(4.162611 0 0 -4.162611 389.21484 625.67104)\"/><path d=\"m0 0c0-7.994-6.479-14.473-14.479-14.473-7.996 0-14.479 6.479-14.479 14.473s6.483 14.479 14.479 14.479c8 0 14.479-6.485 14.479-14.479\" fill=\"#414042\" transform=\"matrix(4.162611 0 0 -4.162611 367.36686 631.05679)\"/><path d=\"m0 0c-3.878 0-7.021 2.858-7.021 6.381v20.081c0 3.52 3.143 6.381 7.021 6.381s7.028-2.861 7.028-6.381v-20.081c0-3.523-3.15-6.381-7.028-6.381\" fill=\"#fff\" transform=\"matrix(4.162611 0 0 -4.162611 511.99336 724.73954)\"/><path d=\"m0 0c0-12.052 9.765-21.815 21.815-21.815 12.041 0 21.808 9.763 21.808 21.815 0 12.044-9.767 21.802-21.808 21.802-12.05 0-21.815-9.758-21.815-21.802\" fill=\"#fff\" transform=\"matrix(4.162611 0 0 -4.162611 634.78706 625.67104)\"/><path d=\"m0 0c0-7.994 6.477-14.473 14.471-14.473 8.002 0 14.479 6.479 14.479 14.473s-6.477 14.479-14.479 14.479c-7.994 0-14.471-6.485-14.471-14.479\" fill=\"#414042\" transform=\"matrix(4.162611 0 0 -4.162611 656.64056 631.05679)\"/></g></svg>\r\n";

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
					lFStream.Write(Encoding.ASCII.GetBytes(ICON_DATA));
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
