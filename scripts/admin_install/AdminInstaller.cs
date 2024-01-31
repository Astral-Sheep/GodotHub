using System.IO.Compression;
using System.IO.MemoryMappedFiles;
using System.Text;

namespace Com.Astral.GodotHub.AdminInstall
{
	internal static class AdminInstaller
	{
		internal static void WriteZip()
		{
			byte[] lContent;

			using (MemoryMappedFile lMMFile = MemoryMappedFile.OpenExisting(AdminInstallConstants.DOWNLOAD_MAP_NAME))
			{
				using (MemoryMappedViewStream lMMVStream = lMMFile.CreateViewStream())
				{
					lContent = new byte[lMMVStream.Length];
					lMMVStream.Read(lContent, 0, lContent.Length);
					lMMVStream.Close();
				}
			}

			using (FileStream lFStream = new FileStream(GetZipPath(), FileMode.Create))
			{
				lFStream.Write(lContent, 0, lContent.Length);
				lFStream.Close();
			}
		}

		internal static void Extract()
		{
			ZipFile.ExtractToDirectory(
				GetZipPath(),
				GetExtractPath()
			);
		}

		internal static void DeleteZip()
		{
			File.Delete(GetZipPath());
		}

		internal static void CancelInstall(bool pDeleteZip, bool pDeleteExecutable)
		{
			if (pDeleteZip)
			{
				string lZip = GetZipPath();

				if (File.Exists(lZip))
				{
					File.Delete(lZip);
				}
			}

			if (pDeleteExecutable)
			{
				string lExecutable = GetExtractPath();

				if (File.Exists(lExecutable))
				{
					File.Delete(lExecutable);
				}
				else if (Directory.Exists(lExecutable))
				{
					Directory.Delete(lExecutable);
				}
			}
		}

		internal static byte Jump()
		{
			using (MemoryMappedFile lMMFile = MemoryMappedFile.OpenExisting(AdminInstallConstants.JUMP_MAP_NAME))
			{
				using (MemoryMappedViewStream lMMVStream = lMMFile.CreateViewStream())
				{
					byte[] lBytes = new byte[1];
					lMMVStream.Read(lBytes, 0, 1);
					return lBytes[0];
				}
			}
		}

		private static string GetZipPath()
		{
			return GetStringFromMap(AdminInstallConstants.ZIP_MAP_NAME);
		}

		private static string GetExtractPath()
		{
			return GetStringFromMap(AdminInstallConstants.EXTRACT_MAP_NAME);
		}

		private static string GetStringFromMap(string pMapName)
		{
			using (MemoryMappedFile lMMFile = MemoryMappedFile.OpenExisting(pMapName))
			{
				using (MemoryMappedViewStream lMMVStream = lMMFile.CreateViewStream())
				{
					byte[] lStringBytes = new byte[lMMVStream.Length];
					lMMVStream.Read(lStringBytes, 0, lStringBytes.Length);
					lMMVStream.Close();
					string lString = Encoding.UTF8.GetString(lStringBytes);
					
					// Since the stream has a static size, the string can be shorter than the allocated space.
					// The remaining bytes are set to 0, and we don't want to recover that part,
					// thus the lString[..lString.IndexOf('\0')]
					return lString[..lString.IndexOf('\0')];
				}
			}
		}
	}
}
