using Com.Astral.GodotHub.Debug;
using Godot;
using System.IO;
using Environment = System.Environment;

namespace Com.Astral.GodotHub.Settings
{
	public static class Config
	{
		private const string SETTINGS = "Settings";
		private const string DEBUG = "Debug";
		private const string PROJECT_DIR = "ProjectDir";
		private const string USE_INSTALL_DIR_FOR_DOWNLOAD = "UseInstallDirForDownload";
		private const string AUTO_DELETE_DOWNLOAD = "AutoDeleteDownload";
		private const string INSTALL_DIR = "InstallDir";
		private const string DOWNLOAD_DIR = "DownloadDir";

		#region PARAMETERS

		public static bool Debug
		{
			get => (bool)GetValue(DEBUG);
			set
			{
				SetValue(DEBUG, value);
			}
		}

		public static string ProjectDir
		{
			get => (string)GetValue(PROJECT_DIR);
			set
			{
				SetValue(PROJECT_DIR, value);
			}
		}

		public static bool UseInstallDirForDownload
		{
			get => (bool)GetValue(USE_INSTALL_DIR_FOR_DOWNLOAD);
			set
			{
				SetValue(USE_INSTALL_DIR_FOR_DOWNLOAD, value);
			}
		}

		public static bool AutoDeleteDownload
		{
			get => (bool)GetValue(AUTO_DELETE_DOWNLOAD);
			set
			{
				SetValue(AUTO_DELETE_DOWNLOAD, value);
			}
		}

		public static string InstallDir
		{
			get => (string)GetValue(INSTALL_DIR);
			set
			{
				SetValue(INSTALL_DIR, value);
			}
		}

		public static string DownloadDir
		{
			get => (string)GetValue(DOWNLOAD_DIR);
			set
			{
				SetValue(DOWNLOAD_DIR, value);
			}
		}

		#endregion //PARAMETERS

		private static readonly string configPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + @"\Godot Hub";
		private static ConfigFile config;

		static Config()
		{
			if (!Directory.Exists(configPath))
			{
				Directory.CreateDirectory(configPath);
			}

			configPath += @"\.config";
			config = new ConfigFile();
			Error lError = config.Load(configPath);

			switch (lError)
			{
				case Error.DoesNotExist:
				case Error.Failed:
				case Error.FileNotFound:
					ResetAll();
					Save();
					break;
				case Error.FileNoPermission:
				case Error.Unauthorized:
					Debugger.PrintError($"Can't load nor create config file: {lError}");
					break;
				case Error.Ok:
					break;
			}
		}

		public static void Save()
		{
			config.Save(configPath);
		}

		public static void ResetAll()
		{
			Debug = true;
			ProjectDir = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
			UseInstallDirForDownload = true;
			AutoDeleteDownload = true;
#if DEBUG
			InstallDir = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile) + @"\Downloads";
			DownloadDir = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile) + @"\Downloads";
#else
			InstallPath = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles)";
			DownloadPath = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles)";
#endif
		}

		public static Variant GetValue(string pName)
		{
			return config.GetValue(SETTINGS, pName);
		}

		public static void SetValue(string pName, Variant pValue)
		{
			config.SetValue(SETTINGS, pName, pValue);
		}
	}
}
