using Com.Astral.GodotHub.Debug;
using Godot;
using Godot.Collections;
using System.Collections.Generic;
using System.IO;

namespace Com.Astral.GodotHub.Data
{
	public static class ProjectsData
	{
		private const string VERSION = "version";
		private const string FAVORITE = "favorite";

		private static readonly string filePath = PathT.appdata + "/project.cfg";
		private static ConfigFile file;

		static ProjectsData()
		{
			file = new ConfigFile();
			Error lError = file.Load(filePath);

			switch (lError)
			{
				case Error.DoesNotExist:
				case Error.Failed:
				case Error.FileNotFound:
					Reset();
					Save();
					break;
				case Error.FileNoPermission:
				case Error.Unauthorized:
					Debugger.PrintError($"Can't load nor create config file: {lError}");
					break;
				default:
					break;
			}
		}

		/// <summary>
		/// Add project to the project.cfg file
		/// </summary>
		public static void AddProject(GDFile pProject)
		{
			file.SetValue(pProject.Path, VERSION, (string)pProject.Version);
			file.SetValue(pProject.Path, FAVORITE, pProject.IsFavorite);
			Save();
		}

		public static bool HasProject(string pProject)
		{
			return file.HasSection(pProject);
		}

		/// <summary>
		/// Remove project from the project.cfg file if it exists
		/// </summary>
		/// <param name="pPath"></param>
		public static void RemoveProject(string pPath)
		{
			if (file.HasSection(pPath))
			{
				file.EraseSection(pPath);
				Save();
			}
		}

		/// <summary>
		/// Whether or not the given project is marked as favorite by the user
		/// </summary>
		/// <param name="pProject"></param>
		/// <returns></returns>
		public static bool IsFavorite(string pProject)
		{
			return (bool)file.GetValue(pProject, FAVORITE);
		}

		/// <summary>
		/// Return the <see cref="Version"/> of the given project 
		/// </summary>
		public static Version GetVersion(string pProject)
		{
			return (Version)(string)file.GetValue(pProject, VERSION);
		}

		/// <summary>
		/// Mark given project as favorite (or not depending on the value of <paramref name="pFavorite"/>)
		/// </summary>
		public static void SetFavorite(string pProject, bool pFavorite)
		{
			file.SetValue(pProject, FAVORITE, pFavorite);
			Save();
		}

		public static void SetVersion(string pProject, string pVersion)
		{
			file.SetValue(pProject, VERSION, pVersion);
			Save();
		}

		public static Version GetVersionFromFolder(string pPath)
		{
			ConfigFile lConfig = new ConfigFile();

			if (lConfig.Load(pPath + "/project.godot") == Error.Ok)
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
				Debugger.PrintError("Invalid project passed, can't find version from folder");
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
			if (pConfigVersion < 4)
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
		/// Save the projects.cfg file
		/// </summary>
		public static void Save()
		{
			file.Save(filePath);
		}

		private static void Reset()
		{
			IEnumerator<string> lDirectories = Directory.EnumerateDirectories(Config.ProjectDir).GetEnumerator();
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
