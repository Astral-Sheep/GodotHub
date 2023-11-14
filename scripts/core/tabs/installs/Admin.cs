#if GODOT_WINDOWS

using Com.Astral.GodotHub.AdminInstall;
using Com.Astral.GodotHub.Debug;
using System;
using System.Diagnostics;
using System.IO;
using System.IO.MemoryMappedFiles;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using Mutex = System.Threading.Mutex;
using Debugger = Com.Astral.GodotHub.Debug.Debugger;
using Godot;
using System.ComponentModel;

namespace Com.Astral.GodotHub.Tabs.Installs
{
	/// <summary>
	/// Static class used to run file writing process with admin permission<br/>
	/// Call it only in a <c>#if GODOT_WINDOWS</c> block since it is available only on this platform<br/>
	/// </summary>
	public static class Admin
	{
		private const string MUTEX_NAME = @"GodotHub-AdminMutex";

		private static readonly string adminProcessPath = @$"{AppDomain.CurrentDomain.BaseDirectory}\AdminInstall.exe";
		private static readonly Mutex mutex = new Mutex(true, MUTEX_NAME);
		private static Process adminProcess;
		private static bool extracted = false;

		static Admin()
		{
			MemoryMappedFile.CreateNew(AdminInstaller.ZIP_MAP_NAME, 4096);
			MemoryMappedFile.CreateNew(AdminInstaller.EXTRACT_MAP_NAME, 4096);
			MemoryMappedFile.CreateNew(AdminInstaller.DELETE_MAP_NAME, 1);
		}

		public static bool Download(HttpContent pContent, string pZipPath)
		{
			if (adminProcess != null)
			{
				Debugger.LogWarning("Another admin download has already started, cancelling last request");
				return false;
			}

			try
			{
				// HttpContent
				Stream lContent = pContent.ReadAsStream();
				byte[] lContentBytes = new byte[lContent.Length];
				lContent.Read(lContentBytes, 0, (int)lContent.Length);
				lContent.Close();

				WriteBytesInMap(AdminInstaller.DOWNLOAD_MAP_NAME, lContentBytes);
				WriteStringInMap(AdminInstaller.ZIP_MAP_NAME, pZipPath);

				adminProcess = StartAdminProcess("--write_zip");
				Thread.Sleep(500);
			}
			catch (Win32Exception)
			{
				Debugger.LogWarning("Failed to get admin rights");
				return false;
			}
			catch (Exception lException)
			{
				ExceptionHandler.Singleton.LogException(lException);
				return false;
			}

			mutex.ReleaseMutex();
			mutex.WaitOne();
			return !adminProcess.HasExited;
		}

		public static bool ExtractZip(string pZipPath, string pExtractDirectory)
		{
			if (adminProcess == null)
			{
				try
				{
					WriteStringInMap(AdminInstaller.ZIP_MAP_NAME, pZipPath);
					adminProcess = StartAdminProcess("--extract");
					Thread.Sleep(500);
				}
				catch (Win32Exception)
				{
					Debugger.LogWarning("Failed to get admin rights");
					return false;
				}
				catch (Exception lException)
				{
					ExceptionHandler.Singleton.LogException(lException);
					return false;
				}
			}

			WriteStringInMap(AdminInstaller.EXTRACT_MAP_NAME, pExtractDirectory);
			WriteBytesInMap(AdminInstaller.DELETE_MAP_NAME, new byte[] { 0 });

			mutex.ReleaseMutex();
			mutex.WaitOne();
			extracted = !adminProcess.HasExited;
			return extracted;
		}

		public static bool DeleteZip()
		{
			if (adminProcess == null)
				return false;

			if (!extracted)
			{
				WriteBytesInMap(AdminInstaller.DELETE_MAP_NAME, new byte[] { 1 });
			}

			mutex.ReleaseMutex();
			mutex.WaitOne();
			return !adminProcess.HasExited || (adminProcess.HasExited && adminProcess.ExitCode == 0);
		}

		public static void EndAdminProcess()
		{
			if (adminProcess == null)
				return;

			if (!adminProcess.HasExited)
			{
				adminProcess.Kill();
			}

			adminProcess = null;
		}

		private static void WriteStringInMap(string pMapName, string pString)
		{
			byte[] lBytes = Encoding.UTF8.GetBytes(pString);
			MemoryMappedFile lMMFile = MemoryMappedFile.CreateOrOpen(pMapName, lBytes.Length);

			using (MemoryMappedViewStream lMMVStream = lMMFile.CreateViewStream())
			{
				lMMVStream.Write(lBytes, 0, lBytes.Length);
				lMMVStream.Close();
			}
		}

		private static void WriteBytesInMap(string pMapName, byte[] pBytes)
		{
			MemoryMappedFile lMMFile = MemoryMappedFile.CreateOrOpen(pMapName, pBytes.Length);

			using (MemoryMappedViewStream lMMVStream = lMMFile.CreateViewStream())
			{
				lMMVStream.Write(pBytes, 0, pBytes.Length);
				lMMVStream.Close();
			}
		}

		private static Process StartAdminProcess(string pArgument)
		{
			extracted = false;
			return Process.Start(new ProcessStartInfo() {
				FileName = adminProcessPath,
				Arguments = pArgument,
				Verb = "runas",
				UseShellExecute = true,
			});
		}
	}
}

#endif //GODOT_WINDOWS
