using System;
using System.IO;

namespace Com.Astral.GodotHub
{
	public static class PathT
	{
		/// <summary>
		/// The roaming appdata path
		/// </summary>
		public static readonly string appdata = GetEnvironmentPath(Environment.SpecialFolder.ApplicationData) + "/Godot Hub";

		static PathT()
		{
			if (!Directory.Exists(appdata))
			{
				Directory.CreateDirectory(appdata);
			}
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
