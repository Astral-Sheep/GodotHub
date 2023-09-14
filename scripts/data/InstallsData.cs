using Com.Astral.GodotHub.Debug;
using Godot;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

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
				exePath = "/Content/Mac/Godot";
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
					exePath += "64{3}";
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

		public static void AddVersion(string pPath, bool pIsExe)
		{
			if (!pIsExe)
			{
				if (Config.os == OS.MacOS)
				{
					pPath += "/Content/Mac/Godot";
				}
				else
				{
					pPath += $"/{pPath[(pPath.RFind("/") + 1)..]}";
					pPath += ".exe";
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

		public static string GetPath(string pVersion)
		{
			if (!file.HasSection(pVersion))
				return "";

			return (string)file.GetValue(pVersion, PATH);
		}

		public static string GetPath(Version pVersion)
		{
			return (string)file.GetValue((string)pVersion, PATH);
		}

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

			//To do: retrieve all engines with the same version x.x
			return lCompatibleVersions;
		}

		public static List<Project> GetAllVersions()
		{
			List<Project> lVersions = new List<Project>();

			foreach (string version in file.GetSections())
			{
				lVersions.Add(new Project(
					(string)file.GetValue(version, PATH),
					(bool)file.GetValue(version, FAVORITE),
					(Version)version
				));
			}

			return lVersions;
		}

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

		public static void Save()
		{
			file.Save(filePath);
		}
	}
}
