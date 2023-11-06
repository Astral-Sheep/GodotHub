using Com.Astral.GodotHub.Debug;
using System;
using System.Diagnostics;

namespace Com.Astral.GodotHub.Tabs.Installs
{
	public static class AdminInstaller
	{
		public static bool InstallAsAdmin(string pArguments)
		{
			Process lProcess = null;

			try
			{
				lProcess = Process.Start(new ProcessStartInfo() {
#if DEBUG
					FileName = @"C:\Users\thoma\Documents\Projects\Godot Hub\AdminInstall\bin\Debug\net6.0\AdminInstall.exe",
#else
					FileName = $"{AppDomain.CurrentDomain.BaseDirection}\AdminInstall\AdminInstall.exe",
#endif //DEBUG
					Arguments = pArguments,
					UseShellExecute = true,
				});
				lProcess.WaitForExit();
			}
			catch (Exception lException)
			{
				ExceptionHandler.Singleton.LogException(lException);
				return false;
			}

			return lProcess.ExitCode == 0;
		}
	}
}
