using System.IO.Compression;
using System.IO.MemoryMappedFiles;
using System.Text;

namespace Com.Astral.GodotHub.AdminInstall
{
	public static class AdminInstaller
	{
		public const string DOWNLOAD_MAP_NAME = @"GodotHub-DownloadContentMap";
		public const string ZIP_MAP_NAME = @"GodotHub-ZipPathMap";
		public const string EXTRACT_MAP_NAME = @"GodotHub-ExtractDirMap";
		public const string DELETE_MAP_NAME = @"GodotHub-DeleteMap";

		internal static void WriteZip()
		{
			byte[] lContent;

			using (MemoryMappedFile lMMFile = MemoryMappedFile.OpenExisting(DOWNLOAD_MAP_NAME))
			{
				Console.WriteLine("Map opened");
				using (MemoryMappedViewStream lMMVStream = lMMFile.CreateViewStream())
				{
					Console.WriteLine("View stream opened");
					lContent = new byte[lMMVStream.Length];
					lMMVStream.Read(lContent, 0, lContent.Length);
					lMMVStream.Close();
					Console.WriteLine("Content read");
				}
			}

			using (FileStream lFStream = new FileStream(GetZipPath(), FileMode.Create))
			{
				Console.WriteLine("File stream opened");
				lFStream.Write(lContent, 0, lContent.Length);
				lFStream.Close();
				Console.WriteLine("Content written");
			}

			Console.WriteLine("Zip written");
		}

		internal static void Extract()
		{
			ZipFile.ExtractToDirectory(
				GetZipPath(),
				GetExtractPath()
			);

			Console.WriteLine("Zip extracted");
		}

		internal static void DeleteZip()
		{
			File.Delete(GetZipPath());
			Console.WriteLine("Zip deleted");
		}

		internal static bool JumpToDelete()
		{
			using (MemoryMappedFile lMMFile = MemoryMappedFile.OpenExisting(DELETE_MAP_NAME))
			{
				using (MemoryMappedViewStream lMMVStream = lMMFile.CreateViewStream())
				{
					byte[] lBytes = new byte[1];
					lMMVStream.Read(lBytes, 0, 1);
					return lBytes[0] != 0;
				}
			}
		}

		private static string GetZipPath()
		{
			return GetStringFromMap(ZIP_MAP_NAME);
		}

		private static string GetExtractPath()
		{
			return GetStringFromMap(EXTRACT_MAP_NAME);
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
					return lString[..lString.IndexOf('\0')];
				}
			}
		}
	}
}
