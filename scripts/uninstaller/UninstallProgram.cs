namespace Com.Astral.GodotHub.Uninstaller
{
	internal class UninstallProgram
	{
		public static void Main(string[] pArgs)
		{
			if (pArgs == null || pArgs.Length == 0)
				return;

			try
			{
				for (int i = 0; i < pArgs.Length; i++)
				{
					DeletePath(pArgs[i]);
				}
			}
			catch (Exception lException)
			{
				Console.WriteLine(lException);
				Console.ReadKey();
			}
		}

		private static void DeletePath(string pPath)
		{
			if (File.Exists(pPath))
			{
				File.Delete(pPath);
			}
			else if (Directory.Exists(pPath))
			{
				Directory.Delete(pPath, true);
			}
		}
	}
}
