using Com.Astral.GodotHub.Debug;
using Godot;
using System;
using System.IO;

using Environment = System.Environment;
using OS = Com.Astral.GodotHub.Data.OS;
using Version = Com.Astral.GodotHub.Data.Version;

namespace Com.Astral.GodotHub
{
	public static class PathT
	{
		private const string MAC_EXE_PATH = "/Contents/MacOS/Godot";
		private const string DEFAULT_EXE_PATH = "/";

		/// <summary>
		/// The roaming appdata path
		/// </summary>
		public static readonly string appdata = GetEnvironmentPath(Environment.SpecialFolder.ApplicationData) + "/Godot Hub";
		private static readonly string exePath;

		static PathT()
		{
			if (!Directory.Exists(appdata))
			{
				Directory.CreateDirectory(appdata);
			}

#if GODOT_MACOS
			exePath = MAC_EXE_PATH;
#else
			exePath = DEFAULT_EXE_PATH;
#endif
		}

		public static string FormatLinuxFolder(string pPath)
		{
			int lIndex = pPath.RFind("linux_x86");

			if (lIndex >= 0)
			{
				pPath = $"{pPath[..(lIndex + 5)]}.{pPath[(lIndex + 6)..]}";
			}

			return pPath;
		}

		public static string FormatMacOSFolder(string pPath, Version pVersion, bool pMono)
		{
			int lIndex = pPath.RFind($"Godot{(pMono ? "_mono" : "")}.app");

			if (lIndex >= 0)
			{
				string lVersion = $"_v{(string)pVersion}";
				pPath = pPath[..(lIndex + 5)] + lVersion + pPath[(lIndex + 5 + lVersion.Length)..];
			}

			return pPath;
		}

		public static string GetMacOSFolderFromZip(string pZip, bool pMono)
		{
			return pZip[..pZip.RFind("/")] + $"/Godot{(pMono ? "_mono" : "")}.app";
		}

		public static string GetExeFromFolder(string pPath)
		{
#if GODOT_MACOS
			return pPath + exePath;
#elif GODOT_LINUXBSD
			return pPath + pPath[pPath.RFind(exePath)..];
#else
			return pPath + pPath[pPath.RFind(exePath)..] + ".exe";
#endif
		}

		public static string GetExeFromFolder(string pPath, OS pOS)
		{
			if (pOS == OS.MacOS)
			{
				return pPath + MAC_EXE_PATH;
			}

			string lPath = pPath + pPath[pPath.RFind(DEFAULT_EXE_PATH)..];

			if (pOS == OS.Windows)
			{
				lPath += ".exe";
			}

			return lPath;
		}

		public static string GetFolderFromExe(string pPath)
		{
			return pPath[..pPath.RFind(exePath)];
		}

		public static string GetFolderFromExe(string pPath, OS pOS)
		{
			return pPath[..pPath.RFind(pOS == OS.MacOS ? MAC_EXE_PATH : DEFAULT_EXE_PATH)];
		}

		/// <summary>
		/// Rename the <paramref name="pSource"/> folder to <paramref name="pTarget"/>
		/// </summary>
		public static bool RenameFolder(string pSource, string pTarget)
		{
			try
			{
				Directory.Move(pSource, pTarget);
			}
			catch (Exception lException)
			{
				Debugger.PrintException(lException);
				return false;
			}

			return true;
		}

		/// <summary>
		/// Get a <see cref="Environment.SpecialFolder"/> path with every \ replaced by a /
		/// </summary>
		public static string GetEnvironmentPath(Environment.SpecialFolder pFolder)
		{
			return Environment.GetFolderPath(pFolder).Replace("\\", "/");
		}
	}
}
