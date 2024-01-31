#if GODOT_WINDOWS

using Com.Astral.GodotHub.AdminInstall;
using Com.Astral.GodotHub.Core.Debug;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.IO.MemoryMappedFiles;
using System.Net.Http;
using System.Text;
using System.Threading;

using Debugger = Com.Astral.GodotHub.Core.Debug.Debugger;
using Mutex = System.Threading.Mutex;

namespace Com.Astral.GodotHub.Core.Tabs.Installs
{
	/// <summary>
	/// Static class used to run processes with admin permission<br/>
	/// Call it only in a <c>#if GODOT_WINDOWS</c> block since it is available only on this platform<br/>
	/// </summary>
	public static class Admin
	{
		private static readonly string adminInstallProcessPath = @$"{AppDomain.CurrentDomain.BaseDirectory}\AdminInstall.exe";
		private static readonly string adminDeleteProcessPath = @$"{AppDomain.CurrentDomain.BaseDirectory}\AdminDelete.exe";
		private static Mutex mutex;
		private static Process adminProcess;
		private static bool extracted = false;

		static Admin()
		{
			MemoryMappedFile.CreateNew(AdminInstallConstants.ZIP_MAP_NAME, 4096);
			MemoryMappedFile.CreateNew(AdminInstallConstants.EXTRACT_MAP_NAME, 4096);
			MemoryMappedFile.CreateNew(AdminInstallConstants.JUMP_MAP_NAME, 1);
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

				WriteBytesInMap(AdminInstallConstants.DOWNLOAD_MAP_NAME, lContentBytes);
				WriteStringInMap(AdminInstallConstants.ZIP_MAP_NAME, pZipPath);

				adminProcess = StartAdminInstallProcess(AdminInstallConstants.WRITE_ARGUMENT);
				Thread.Sleep(1000);
			}
			catch (Win32Exception)
			{
				LogAdminRightsDenial($"{nameof(Admin)}.{nameof(Download)}", pZipPath);
				return false;
			}
			catch (Exception lException)
			{
#if DEBUG
				ExceptionHandler.Singleton.LogMessage(
					$"Error in method {nameof(Admin)}.{nameof(Download)}\n\n{lException.Message}",
					lException.GetType().ToString(),
					ExceptionHandler.ExceptionGravity.Error
				);
#else
				string lZip = pZipPath.LastIndexOf('/') > 0 ? pZipPath[pZipPath.LastIndexOf('/')..] : pZipPath;
				ExceptionHandler.Singleton.LogMessage(
					$"Unable to download file {lZip}\n\n{lException.GetType()}: {lException.Message}",
					null,
					ExceptionHandler.ExceptionGravity.Error
				);
#endif //DEBUG
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
					WriteStringInMap(AdminInstallConstants.ZIP_MAP_NAME, pZipPath);
					adminProcess = StartAdminInstallProcess(AdminInstallConstants.EXTRACT_ARGUMENT);
					Thread.Sleep(1000);
				}
				catch (Win32Exception)
				{
					LogAdminRightsDenial($"{nameof(Admin)}.{nameof(ExtractZip)}", pZipPath);
					return false;
				}
				catch (Exception lException)
				{
#if DEBUG
					ExceptionHandler.Singleton.LogMessage(
						$"Error in method {nameof(Admin)}.{nameof(ExtractZip)}\n\n{lException.Message}",
						lException.GetType().ToString(),
						ExceptionHandler.ExceptionGravity.Error
					);
#else
					string lZip = pZipPath.LastIndexOf('/') > 0 ? pZipPath[pZipPath.LastIndexOf('/')..] : pZipPath;
					ExceptionHandler.Singleton.LogMessage(
						$"Unable to unzip file {lZip}\n\n{lException.GetType().ToString()}: {lException.Message}",
						null,
						ExceptionHandler.ExceptionGravity.Error
					);
#endif //DEBUG
					return false;
				}
			}

			WriteStringInMap(AdminInstallConstants.EXTRACT_MAP_NAME, pExtractDirectory);
			SetJumpMap(JumpInstruction.None);

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
				SetJumpMap(JumpInstruction.Delete);
			}

			mutex.ReleaseMutex();
			mutex.WaitOne();
			return !adminProcess.HasExited || adminProcess.ExitCode == 0;
		}

		public static void CancelUnzip(string pZipPath, string pExecutablePath)
		{
			if (adminProcess == null)
				return;

			WriteStringInMap(AdminInstallConstants.ZIP_MAP_NAME, pZipPath);
			WriteStringInMap(AdminInstallConstants.EXTRACT_MAP_NAME, pExecutablePath);
			SetJumpMap(JumpInstruction.Cancel);
			mutex.ReleaseMutex();
			mutex.WaitOne();
		}

		public static bool DeletePaths(params string[] pPaths)
		{
			if (pPaths.Length == 0)
				return true;

			string lArguments = "";

			for (int i = 0; i < pPaths.Length; i++)
			{
				lArguments += $"\"{pPaths[i]}\" ";
			}

			try
			{
				StartAdminProcess(adminDeleteProcessPath, lArguments).WaitForExit();
			}
#if GODOT_WINDOWS
			catch (Win32Exception lException)
			{
#if DEBUG
				ExceptionHandler.Singleton.LogMessage(
					$"Unable to get admin rights in method {nameof(Admin)}.{nameof(DeletePaths)}",
					"Admin rights denied"
				);				
#else
				string lPaths = "";

				for (int i = 0; i < pPaths.Length; i++)
				{
					lPaths += pPaths + "\n";
				}
				
				ExceptionHandler.Singleton.LogMessage(
					$"Unable to get admin rights to delete paths\n{lPaths}",
					"Admin rights denied"
				);
#endif //DEBUG
				return false;
			}
#endif //GODOT_WINDOWS
			catch (Exception lException)
			{
#if DEBUG
				ExceptionHandler.Singleton.LogMessage(
					$"Error in method {nameof(Admin)}.{nameof(DeletePaths)}\n\n{lException.Message}",
					lException.GetType().ToString(),
					ExceptionHandler.ExceptionGravity.Error
				);				
#else
				string lPaths = "";

				for (int i = 0; i < pPaths.Length; i++)
				{
					lPaths += pPaths + "\n";
				}
				
				ExceptionHandler.Singleton.LogMessage(
					$"Unable to delete paths\n{lPaths}",
					null,
					ExceptionHandler.ExceptionGravity.Error
				);
#endif //DEBUG
				return false;
			}

			return true;
		}

		private static Process StartAdminInstallProcess(string pArgument)
		{
			extracted = false;
			mutex = new Mutex(true, AdminInstallConstants.MUTEX_NAME);
			return StartAdminProcess(adminInstallProcessPath, pArgument);
		}

		private static Process StartAdminProcess(string pFileName, string pArguments)
		{
			return Process.Start(new ProcessStartInfo() {
				FileName = pFileName,
				Arguments = pArguments,
				Verb = "runas",
				UseShellExecute = true,
			});
		}

		public static void EndAdminInstallProcess()
		{
			if (adminProcess == null)
				return;

			if (!adminProcess.HasExited)
			{
				adminProcess.Kill();
			}

			mutex.Dispose();
			mutex = null;
			adminProcess = null;
		}

		private static void SetJumpMap(JumpInstruction pInstruction)
		{
			WriteBytesInMap(AdminInstallConstants.JUMP_MAP_NAME, new byte[] { (byte)pInstruction });
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

		private static void LogAdminRightsDenial(string pMethodName = null, string pZip = null)
		{
#if DEBUG
			ExceptionHandler.Singleton.LogMessage(
				$"Unable to get admin rights in method {pMethodName}",
				"Admin rights denied"
			);
#else
			string lZip = pZip?.LastIndexOf('/') > 0 ? pZip[pZip.LastIndexOf('/')..] : pZip;
			ExceptionHandler.Singleton.LogMessage(
				$"Unable to get admin rights to download {lZip}",
				"Admin rights denied"
			);
#endif //DEBUG
		}
	}
}

#endif //GODOT_WINDOWS
