using Com.Astral.GodotHub.Core.Debug;
using Com.Astral.GodotHub.Core.Utils;
using Godot;
using Godot.Collections;
using System;
using System.Collections.Generic;
using System.IO;

using GError = Godot.Error;

namespace Com.Astral.GodotHub.Core.Data
{
	public static class ProjectsData
	{
		private const string VERSION = "version";
		private const string FAVORITE = "favorite";

		/// <summary>
		/// Event called when a project is added in the projects config file
		/// </summary>
		public static event Action<GDFile> Added;

		private static readonly string filePath = PathT.appdata + "/projects.cfg";
		private static ConfigFile file;

		static ProjectsData()
		{
			file = new ConfigFile();
			GError lError = file.Load(filePath);

			switch (lError)
			{
				case GError.DoesNotExist:
				case GError.Failed:
				case GError.FileNotFound:
					Reset();
					Save();
					break;
				case GError.FileNoPermission:
				case GError.Unauthorized:
					ExceptionHandler.Singleton.LogMessage(
						$"Can't load nor create projects config file: {lError}",
						lError.ToString(),
						ExceptionHandler.ExceptionGravity.Error
					);
					Debugger.LogError($"Can't load nor create config file: {lError}");
					break;
				default:
					break;
			}
		}

		/// <summary>
		/// Add the project to the project config file
		/// </summary>
		public static void AddProject(GDFile pProject)
		{
			file.SetValue(pProject.Path, VERSION, (string)pProject.Version);
			file.SetValue(pProject.Path, FAVORITE, pProject.IsFavorite);
			Save();
			Added?.Invoke(pProject);
		}

		/// <summary>
		/// Whether or not a project is in the project config file
		/// </summary>
		public static bool HasProject(string pDirectory)
		{
			return file.HasSection(pDirectory);
		}

		/// <summary>
		/// Remove the project from the project config file
		/// </summary>
		public static void RemoveProject(string pPath)
		{
			if (file.HasSection(pPath))
			{
				file.EraseSection(pPath);
				Save();
			}
		}

		/// <summary>
		/// Set project favorite status
		/// </summary>
		public static void SetFavorite(string pProject, bool pFavorite)
		{
			file.SetValue(pProject, FAVORITE, pFavorite);
			Save();
		}

		/// <summary>
		/// Set project current <see cref="Version"/>
		/// </summary>
		public static void SetVersion(string pProject, string pVersion)
		{
			file.SetValue(pProject, VERSION, pVersion);
			Save();
		}

		/// <summary>
		/// Return project <see cref="Version"/> from a path<br/>
		/// If the path doesn't refer to a project, it returns the default <see cref="Version"/>
		/// </summary>
		public static Version GetVersionFromFolder(string pPath)
		{
			ConfigFile lConfig = new ConfigFile();
			GError lError = lConfig.Load(pPath + "/project.godot");

			if (lError == GError.Ok)
			{
				int lConfigVersion = (int)lConfig.GetValue("", "config_version");

				if (lConfigVersion >= 5)
				{
					return GetGodot4OrHigherVersion(lConfig);
				}
				else
				{
					return GetGodot3OrLowerVersion(lConfigVersion);
				}
			}
			else
			{
				ExceptionHandler.Singleton.LogMessage(
					$"Invalid project passed, can't find version from folder: {lError}",
					lError.ToString(),
					ExceptionHandler.ExceptionGravity.Error
				);
				Debugger.LogError($"Invalid project passed, can't find version from folder: {lError}");
				return new Version();
			}
		}

		private static Version GetGodot4OrHigherVersion(ConfigFile pConfig)
		{
			Array<string> lFeatures = (Array<string>)pConfig.GetValue("application", "config/features");
			return (Version)lFeatures[0];
		}

		private static Version GetGodot3OrLowerVersion(int pConfigVersion)
		{
			if (pConfigVersion != 4)
				return new Version();

			return new Version(3, 5, 2);
		}

		/// <summary>
		/// Return all saved projects
		/// </summary>
		public static List<GDFile> GetProjects()
		{
			List<GDFile> lProjects = new List<GDFile>();

			foreach (string project in file.GetSections())
			{
				lProjects.Add(new GDFile(
					project,
					(bool)file.GetValue(project, FAVORITE),
					(Version)(string)file.GetValue(project, VERSION)
				));
			}

			return lProjects;
		}

		/// <summary>
		/// Save projects.cfg
		/// </summary>
		public static void Save()
		{
			file.Save(filePath);
		}

		private static void Reset()
		{
			IEnumerator<string> lDirectories = Directory.EnumerateDirectories(AppConfig.ProjectDir).GetEnumerator();
			string lDirectory;

			while (lDirectories.MoveNext())
			{
				if (File.Exists($"{lDirectories.Current}/project.godot"))
				{
					lDirectory = lDirectories.Current.Replace("\\", "/");
					file.SetValue(lDirectory, VERSION, (string)GetVersionFromFolder(lDirectories.Current));
					file.SetValue(lDirectory, FAVORITE, false);
				}
			}
		}
	}
}
