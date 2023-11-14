using Com.Astral.GodotHub.Core.Debug;
using Com.Astral.GodotHub.Core.Utils;
using Godot;
using System;
using System.Runtime.InteropServices;
using Environment = System.Environment;

using GError = Godot.Error;

namespace Com.Astral.GodotHub.Core.Data
{
	/// <summary>
	/// Static class used to get and set application parameters
	/// </summary>
	public static class AppConfig
	{
		private enum Settings
		{
			AutoCloseDownload,
			AutoCreateShortcut,
			AutoDeleteZip,
			AutoUpdateRepository,
			Debug,
			DownloadDir,
			InstallDir,
			ProjectDir,
			UseInstallDirForZip,
		}

		/// <summary>
		/// Event called when all settings are set to default values
		/// </summary>
		public static event Action Reset;

		#region PROPERTIES

		/// <summary>
		/// Whether or not to close the download popup when completed or cancelled
		/// </summary>
		public static bool AutoCloseDownload
		{
			get => (bool)GetValue(Settings.AutoCloseDownload);
			set
			{
				SetValue(Settings.AutoCloseDownload, value);
			}
		}

		/// <summary>
		/// Whether or not to create a shortcut on desktop after installing a <see cref="Version"/> of Godot
		/// </summary>
		public static bool AutoCreateShortcut
		{
			get => (bool)GetValue(Settings.AutoCreateShortcut);
			set
			{
				SetValue(Settings.AutoCreateShortcut, value);
			}
		}

		/// <summary>
		/// Whether or not to delete the .zip file once it's extracted
		/// </summary>
		public static bool AutoDeleteZip
		{
			get => (bool)GetValue(Settings.AutoDeleteZip);
			set
			{
				SetValue(Settings.AutoDeleteZip, value);
			}
		}

		/// <summary>
		/// Whether or not to retrieve new releases when launching the application
		/// </summary>
		public static bool AutoUpdateRepository
		{
			get => (bool)GetValue(Settings.AutoUpdateRepository);
			set
			{
				SetValue(Settings.AutoUpdateRepository, value);
			}
		}

		/// <summary>
		/// Whether or not the <see cref="Debugger"/> is visible
		/// </summary>
		public static bool Debug
		{
			get => (bool)GetValue(Settings.Debug);
			set
			{
				SetValue(Settings.Debug, value);
			}
		}

		/// <summary>
		/// Directory in which the .zip file is downloaded
		/// </summary>
		public static string DownloadDir
		{
			get => (string)GetValue(Settings.DownloadDir);
			set
			{
				SetValue(Settings.DownloadDir, value);
			}
		}

		/// <summary>
		/// Directory in which the .zip is extracted
		/// </summary>
		public static string InstallDir
		{
			get => (string)GetValue(Settings.InstallDir);
			set
			{
				SetValue(Settings.InstallDir, value);
			}
		}

		/// <summary>
		/// Default directory in which a project is created
		/// </summary>
		public static string ProjectDir
		{
			get => (string)GetValue(Settings.ProjectDir);
			set
			{
				SetValue(Settings.ProjectDir, value);
			}
		}

		/// <summary>
		/// Whether or not to use the same directory to download and extract the .zip file
		/// </summary>
		public static bool UseInstallDirForDownload
		{
			get => (bool)GetValue(Settings.UseInstallDirForZip);
			set
			{
				SetValue(Settings.UseInstallDirForZip, value);
			}
		}

		#endregion //PROPERTIES

		/// <summary>
		/// Current platform operating system
		/// </summary>
		public static readonly OS os;
		/// <summary>
		/// Current platform architecture
		/// </summary>
		public static readonly Architecture architecture;

		private static readonly string filePath = PathT.appdata + "/config.cfg";
		private static ConfigFile file;

		static AppConfig()
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
			GError lError = file.Load(filePath);

			switch (lError)
			{
				case GError.DoesNotExist:
				case GError.Failed:
				case GError.FileNotFound:
					ResetAll();
					Save();
					break;
				case GError.FileNoPermission:
				case GError.Unauthorized:
					ExceptionHandler.Singleton.LogMessage(
						$"Can't load nor create application config file: {lError}",
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
		/// Save all settings
		/// </summary>
		public static void Save()
		{
			file.Save(filePath);
		}

		/// <summary>
		/// Set all settings to default values
		/// </summary>
		public static void ResetAll()
		{
			AutoCloseDownload = true;
			AutoCreateShortcut = true;
			AutoDeleteZip = true;
			AutoUpdateRepository = false;

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

		private static Variant GetValue(Settings pSetting)
		{
			return file.GetValue(nameof(Settings), pSetting.ToString());
		}

		private static void SetValue(Settings pSetting, Variant pValue)
		{
			file.SetValue(nameof(Settings), pSetting.ToString(), pValue);
		}
	}
}
