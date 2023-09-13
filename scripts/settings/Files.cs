using Com.Astral.GodotHub.Debug;
using Godot;
using Godot.Collections;

namespace Com.Astral.GodotHub.Settings
{
	public static class Files
	{
		private const string PROJECTS = "Projects";
		private const string INSTALLS = "Installs";
		private const string PATHS = "Paths";
		private const string FAVORITES = "Favorites";

		#region PROPERTIES

		public static Array<string> Projects
		{
			get => (Array<string>)files.GetValue(PROJECTS, PATHS);
			set
			{
				files.SetValue(PROJECTS, PATHS, value);
			}
		}

		public static Array<bool> Favorites
		{
			get => (Array<bool>)files.GetValue(PROJECTS, PATHS);
			set
			{
				files.SetValue(PROJECTS, PATHS, value);
			}
		}

		public static Array<string> Installs
		{
			get => (Array<string>)files.GetValue(INSTALLS, PATHS);
			set
			{
				files.SetValue(INSTALLS, PATHS, value);
			}
		}

		#endregion //PROPERTIES

		private static readonly string path = Config.dataPath + "/files.ini";
		private static ConfigFile files;

		static Files()
		{
			files = new ConfigFile();
			Error lError = files.Load(path);

			switch (lError)
			{
				case Error.DoesNotExist:
				case Error.Failed:
				case Error.FileNotFound:
					ResetAll();
					Save();
					Debugger.PrintMessage("Files save created successfully");
					break;
				case Error.FileNoPermission:
				case Error.Unauthorized:
					Debugger.PrintError($"Can't load nor create files save: {lError}");
					break;
				case Error.Ok:
					break;
			}
		}

		private static void Save()
		{
			files.Save(path);
		}

		private static void ResetAll()
		{
			ResetProject();
			ResetInstalls();
		}

		private static void ResetProject()
		{
			files.SetValue(PROJECTS, PATHS, new Array<string>());
			files.SetValue(PROJECTS, FAVORITES, new Array<bool>());
		}

		private static void ResetInstalls()
		{
			files.SetValue(INSTALLS, PATHS, new Array<string>());
		}
	}
}
