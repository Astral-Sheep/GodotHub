using Com.Astral.GodotHub.Debug;
using Godot;
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
		}

		private static string GetVersionFromFolder(string pPath)
		{
			//Godot 4.0 and above
			if (Directory.Exists($"{pPath}/.godot"))
			{
				ConfigFile lFile = new ConfigFile();
				lFile.Load($"{pPath}/.godot/editor/project_metadata.cfg");
				string lExecutable = (string)lFile.GetValue("editor_metadata", "executable_path");
				return (string)(Version)lExecutable;
				//To do: recognize MacOS
			}
			else
			{
				//To do: Godot 3.5 and lower
			}

			return "";
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
					file.SetValue(lDirectory, VERSION, GetVersionFromFolder(lDirectories.Current));
					file.SetValue(lDirectory, FAVORITE, false);
				}
			}
		}
	}
}
