using Com.Astral.GodotHub.Debug;
using Godot;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;

namespace Com.Astral.GodotHub.Data
{
	public static class InstallsData
	{
		private const string PATH = "path";
		private const string FAVORITE = "favorite";

		public static event Action<GDFile> VersionAdded;

		private static readonly string exePath;
		private static readonly Regex folderExpr;
		private static readonly string filePath = PathT.appdata + "/installs.cfg";
		private static ConfigFile file;

		static InstallsData()
		{
#if GODOT_WINDOWS
			folderExpr = new Regex(@"Godot_v[0-9]+(?:[.][0-9]+){1,2}-stable(_mono)??_win[0-9]{2}");
			exePath = "/Godot_v{0}-stable{1}_win{2}.exe";
#elif GODOT_LINUXBSD
			folderExpr = new Regex(@"Godot_v[0-9]+(?:[.][0-9]+){1,2}-stable(_mono)??_linux[._]x86_[0-9]{2}");
			exePath = "/Godot_{0}-stable{1}_linux.x86_{2}";
#elif GODOT_MACOS
			folderExpr = new Regex(@"(?:Godot)(_mono)??(?:[.]app)$");
			exePath = "/Contents/MacOS/Godot";
#endif

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
		/// Add engine version to the installs.cfg file
		/// </summary>
		public static void AddVersion(string pPath, bool pIsExe)
		{
			if (!folderExpr.IsMatch(pPath))
				return;

			if (!pIsExe)
			{
#if GODOT_MACOS
				pPath += exePath;
#else
				pPath += string.Format(exePath, (Version)pPath, pPath.Contains("_mono") ? "_mono" : "", pPath[^2..]);
#endif
			}

			if (!File.Exists(pPath))
			{
				Debugger.PrintError($"Invalid file passed as executable: {pPath}. Version not added");
				return;
			}

			string lVersion = (string)(Version)pPath;

			if (file.HasSection(lVersion))
			{
				string lCurrent = (string)file.GetValue(lVersion, PATH);

				if (lCurrent.Contains("_mono"))
				{
					if (!pPath.Contains("_mono"))
					{
						Debugger.PrintWarning($"Less advanced version passed in method {nameof(AddVersion)}, keeping the current one");
						return;
					}
#if GODOT_WINDOWS
					//Get architecture in _win{xx}.exe
					else if (int.Parse(pPath[^6..^4]) <= int.Parse(lCurrent[^6..^4]))
					{
						Debugger.PrintWarning($"Less advanced version passed in method {nameof(AddVersion)}, keeping the current one");
						return;
					}
#elif GODOT_LINUXBSD
					//Get architecture in linux.x86_{xx}
					else if (int.Parse(pPath[^2..]) <= int.Parse(lCurrent[^2..]))
					{
						Debugger.PrintWarning($"Less advanced version passed in method {nameof(AddVersion)}, keeping the current one");
						return;
					}
#endif
				}
			}

			file.SetValue(lVersion, PATH, pPath);
			file.SetValue(lVersion, FAVORITE, false);
			Save();
			VersionAdded?.Invoke(new GDFile(pPath, false, (Version)lVersion));
		}

		public static void RemoveVersion(Version pVersion)
		{
			if (!file.HasSection((string)pVersion))
				return;

			file.EraseSection((string)pVersion);
			Save();
		}

		/// <summary>
		/// Return engine path from <see cref="Version"/>
		/// </summary>
		public static string GetPath(string pVersion)
		{
			if (!file.HasSection(pVersion))
				return "";

			return (string)file.GetValue(pVersion, PATH);
		}

		public static void SetFavorite(Version pVersion, bool pIsFavorite)
		{
			if (!file.HasSection((string)pVersion))
				return;

			file.SetValue((string)pVersion, FAVORITE, pIsFavorite);
			Save();
		}

		/// <summary>
		/// Return engine versions that share the same major and minor indices
		/// </summary>
		public static List<Version> GetCompatibleVersions(Version pVersion)
		{
			List<Version> lCompatibleVersions = new List<Version>();
			string[] lVersions = file.GetSections() ?? Array.Empty<string>();
			Version lVersion;

			for (int i = 0; i < lVersions.Length; i++)
			{
				lVersion = (Version)lVersions[i];

				if (lVersion.IsCompatible(pVersion))
				{
					lCompatibleVersions.Add(lVersion);
				}
			}

			return lCompatibleVersions;
		}

		/// <summary>
		/// Return all installed engine versions
		/// </summary>
		public static List<GDFile> GetAllVersions()
		{
			List<GDFile> lVersions = new List<GDFile>();

			foreach (string version in file.GetSections())
			{
				lVersions.Add(new GDFile(
					(string)file.GetValue(version, PATH),
					(bool)file.GetValue(version, FAVORITE),
					(Version)version
				));
			}

			return lVersions;
		}

		/// <summary>
		/// Retrive all engine versions from the default directory
		/// </summary>
		public static void Reset()
		{
			IEnumerator<string> lDirectories = Directory.EnumerateDirectories(Config.InstallDir).GetEnumerator();

			while (lDirectories.MoveNext())
			{
				if (folderExpr.IsMatch(lDirectories.Current))
				{
					AddVersion(lDirectories.Current.Replace("\\", "/"), false);
				}
			}
		}

		/// <summary>
		/// Save the installs.cfg file
		/// </summary>
		public static void Save()
		{
			file.Save(filePath);
		}
	}
}
