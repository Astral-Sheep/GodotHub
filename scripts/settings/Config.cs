using Com.Astral.GodotHub.Debug;
using Godot;
using System;
using System.IO;
using Environment = System.Environment;

namespace Com.Astral.GodotHub.Settings
{
	public static class Config
	{
		public enum Settings
		{
			AutoCloseDownload,
			AutoCreateShortcut,
			AutoDeleteZip,
			AutoRefreshRepos,
			Debug,
			DownloadDir,
			InstallDir,
			ProjectDir,
			UseInstallDirForZip,
		}

		public static event Action Reset;

		#region PROPERTIES

		public static bool AutoCloseDownload
		{
			get => (bool)GetValue(Settings.AutoCloseDownload);
			set
			{
				SetValue(Settings.AutoCloseDownload, value);
			}
		}

		public static bool AutoCreateShortcut
		{
			get => (bool)GetValue(Settings.AutoCreateShortcut);
			set
			{
				SetValue(Settings.AutoCreateShortcut, value);
			}
		}

		public static bool AutoDeleteDownload
		{
			get => (bool)GetValue(Settings.AutoDeleteZip);
			set
			{
				SetValue(Settings.AutoDeleteZip, value);
			}
		}

		public static bool AutoRefreshRepos
		{
			get => (bool)GetValue(Settings.AutoRefreshRepos);
			set
			{
				SetValue(Settings.AutoRefreshRepos, value);
			}
		}

		public static bool Debug
		{
			get => (bool)GetValue(Settings.Debug);
			set
			{
				SetValue(Settings.Debug, value);
			}
		}

		public static string DownloadDir
		{
			get => (string)GetValue(Settings.DownloadDir);
			set
			{
				SetValue(Settings.DownloadDir, value);
			}
		}

		public static string InstallDir
		{
			get => (string)GetValue(Settings.InstallDir);
			set
			{
				SetValue(Settings.InstallDir, value);
			}
		}

		public static string ProjectDir
		{
			get => (string)GetValue(Settings.ProjectDir);
			set
			{
				SetValue(Settings.ProjectDir, value);
			}
		}

		public static bool UseInstallDirForDownload
		{
			get => (bool)GetValue(Settings.UseInstallDirForZip);
			set
			{
				SetValue(Settings.UseInstallDirForZip, value);
			}
		}

		#endregion //PROPERTIES

		public static readonly string dataPath = GetEnvironmentPath(Environment.SpecialFolder.ApplicationData) + "/Godot Hub";
		private static readonly string configPath;
		private static ConfigFile config;

		static Config()
		{
			if (!Directory.Exists(dataPath))
			{
				Directory.CreateDirectory(dataPath);
			}

			configPath = dataPath + "/config.cfg";
			config = new ConfigFile();
			Error lError = config.Load(configPath);

			switch (lError)
			{
				case Error.DoesNotExist:
				case Error.Failed:
				case Error.FileNotFound:
					ResetAll();
					Save();
					Debugger.PrintMessage("Config file created successfully");
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
			Debugger.PrintMessage("Config saved");
		}

		public static void ResetAll()
		{
			AutoCloseDownload = true;
			AutoCreateShortcut = true;
			AutoDeleteDownload = true;
			AutoRefreshRepos = true;

#if DEBUG
			Debug = true;
			DownloadDir = GetEnvironmentPath(Environment.SpecialFolder.UserProfile) + "/Downloads";
			InstallDir = GetEnvironmentPath(Environment.SpecialFolder.UserProfile) + "/Downloads";
#else
			Debug = false;
			DownloadDir = GetEnvironmentPath(Environment.SpecialFolder.ProgramFiles);
			InstallDir = GetEnvironmentPath(Environment.SpecialFolder.ProgramFiles);
#endif

			ProjectDir = GetEnvironmentPath(Environment.SpecialFolder.MyDocuments);
			UseInstallDirForDownload = true;
			Reset?.Invoke();
		}

		public static Variant GetValue(Settings pSetting)
		{
			return config.GetValue(nameof(Settings), pSetting.ToString());
		}

		public static void SetValue(Settings pSetting, Variant pValue)
		{
			config.SetValue(nameof(Settings), pSetting.ToString(), pValue);
		}

		private static string GetEnvironmentPath(Environment.SpecialFolder pFolder)
		{
			return Environment.GetFolderPath(pFolder).Replace('\\', '/');
		}
	}
}
