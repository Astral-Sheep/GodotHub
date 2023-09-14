using Com.Astral.GodotHub.Debug;
using Godot;
using System;
using System.Runtime.InteropServices;
using Environment = System.Environment;

namespace Com.Astral.GodotHub.Data
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

		public static readonly OS os;
		public static readonly Architecture architecture;

		private static readonly string filePath = PathT.appdata + "/config.cfg";
		private static ConfigFile file;

		static Config()
		{
			os = Environment.OSVersion.Platform switch {
				PlatformID.Win32NT => OS.Windows,
				PlatformID.Unix => OS.Linux,
				_ => OS.MacOS,
			};

			architecture = (int)RuntimeInformation.OSArchitecture % 2 == 1 ?
				Architecture.x64 :
				Architecture.x32;

			file = new ConfigFile();
			Error lError = file.Load(filePath);

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
				default:
					break;
			}
		}

		public static void Save()
		{
			file.Save(filePath);
			Debugger.PrintValidation("Config saved");
		}

		public static void ResetAll()
		{
			AutoCloseDownload = true;
			AutoCreateShortcut = true;
			AutoDeleteDownload = true;
			AutoRefreshRepos = true;

#if DEBUG
			Debug = true;
			DownloadDir = PathT.GetEnvironmentPath(Environment.SpecialFolder.UserProfile) + "/Downloads";
			InstallDir = PathT.GetEnvironmentPath(Environment.SpecialFolder.UserProfile) + "/Downloads";
#else
			Debug = false;
			DownloadDir = PathT.GetEnvironmentPath(Environment.SpecialFolder.ProgramFiles);
			InstallDir = PathT.GetEnvironmentPath(Environment.SpecialFolder.ProgramFiles);
#endif

			ProjectDir = PathT.GetEnvironmentPath(Environment.SpecialFolder.MyDocuments);
			UseInstallDirForDownload = true;
			Reset?.Invoke();
		}

		public static Variant GetValue(Settings pSetting)
		{
			return file.GetValue(nameof(Settings), pSetting.ToString());
		}

		public static void SetValue(Settings pSetting, Variant pValue)
		{
			file.SetValue(nameof(Settings), pSetting.ToString(), pValue);
		}
	}
}
