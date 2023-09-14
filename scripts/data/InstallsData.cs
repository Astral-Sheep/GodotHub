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

		private static readonly string exePath;
		private static readonly Regex folderExpr;
		private static readonly string filePath = PathT.appdata + "/installs.cfg";
		private static ConfigFile file;

		static InstallsData()
		{
			if (Config.os == OS.MacOS)
			{
				folderExpr = new Regex(@"(?:Godot)(_mono)??(?:[.]app)$");
				exePath = "/Contents/MacOS/Godot";
			}
			else
			{
				string lPattern = @"Godot_v[0-9]+(?:[.][0-9]+){1,2}-stable(_mono)??";
				exePath = "Godot_v{0}-stable{1}";

				if (Config.os == OS.Windows)
				{
					lPattern += "_win";
					exePath += "_win";
				}
				else
				{
					lPattern += "_linux[._]x86_";
					exePath += "_linux{2}x86_";
				}

				if (Config.architecture == Architecture.x64)
				{
					lPattern += "64";
					exePath += "64";
				}
				else
				{
					lPattern += "32";
					exePath += "32";

				}

				if (Config.os == OS.Windows)
				{
					exePath += ".exe";
				}

				folderExpr = new Regex(lPattern);
			}

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
			if (!pIsExe)
			{
				if (Config.os == OS.MacOS)
				{
					pPath += "/Contents/MacOS/Godot";
				}
				else
				{
					pPath += $"/{pPath[(pPath.RFind("/") + 1)..]}";

					if (Config.os == OS.Windows)
					{
						pPath += ".exe";
					}
				}
			}

			if (!File.Exists(pPath))
			{
				Debugger.PrintError("Invalid file passed as executable");
				return;
			}

			string lVersion = (string)(Version)pPath;
			file.SetValue(lVersion, PATH, pPath);
			file.SetValue(lVersion, FAVORITE, false);
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
