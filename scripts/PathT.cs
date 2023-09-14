using System;
using System.IO;

namespace Com.Astral.GodotHub
{
	public static class PathT
	{
		public static readonly string appdata = GetEnvironmentPath(Environment.SpecialFolder.ApplicationData) + "/Godot Hub";

		static PathT()
		{
			if (!Directory.Exists(appdata))
			{
				Directory.CreateDirectory(appdata);
			}
		}

		public static string GetEnvironmentPath(Environment.SpecialFolder pFolder)
		{
			return Environment.GetFolderPath(pFolder).Replace("\\", "/");
		}
	}
}
