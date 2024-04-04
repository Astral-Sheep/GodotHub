namespace Com.Astral.GodotHub.Updater
{
	internal static class UpdateProgram
	{
		private static readonly string Appdata = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData).Replace('\\', '/') + "/Godot Hub";

		private static readonly string NewProjectsPath = Appdata + "/projects.cfg";
		private static readonly string NewEnginesPath = Appdata + "/engines.cfg";

		private static readonly string[] OldProjectsPaths = new string[] {
			Appdata + "/project.cfg",
		};
		private static readonly string[] OldEnginesPaths = new string[] {
			Appdata + "/installs.cfg",
		};

		public static void Main(string[] pArgs)
		{
			bool lAutoDeleteSource = pArgs.Length > 0 && (pArgs[0].ToLower() == "-d" || pArgs[0].ToLower() == "--delete");
			
			if (UpdatePath(OldProjectsPaths, NewProjectsPath, lAutoDeleteSource))
			{
				Console.WriteLine("Projects data updated.");
			}
			else
			{
				Console.WriteLine("Projects data can't be updated.");
			}

			if (UpdatePath(OldEnginesPaths, NewEnginesPath, lAutoDeleteSource))
			{
				Console.WriteLine("Engines data updated.");
			}
			else
			{
				Console.WriteLine("Engines data can't be updated.");
			}

			Console.WriteLine("\nPress any key to close the window");
			Console.ReadKey();
		}

		private static bool UpdatePath(string[] pOldPaths, string pNewPath, bool pDeleteSource = false)
		{
			for (int i = 0; i < pOldPaths.Length; i++)
			{
				if (CopyFile(pOldPaths[i], pNewPath, pDeleteSource))
					return true;
			}

			return false;
		}

		private static bool CopyFile(string pSource, string pDestination, bool pDeleteSource)
		{
			if (!File.Exists(pSource))
				return false;

			try
			{
				using (
					FileStream lSourceFStream = new FileStream(pSource, FileMode.Open),
					lDestFStream = new FileStream(pDestination, FileMode.OpenOrCreate)
				)
				{
					lSourceFStream.CopyTo(lDestFStream);
					lDestFStream.Close();
					lSourceFStream.Close();
				}
			}
			catch (Exception lException)
			{
				Console.WriteLine(lException);
				return false;
			}

			if (pDeleteSource)
			{
				File.Delete(pSource);
			}
			
			return true;
		}
	}
}
