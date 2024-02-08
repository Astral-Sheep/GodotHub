namespace Com.Astral.GodotHub.Installer
{
	public static class InstallerConstants
	{
#if DEBUG
		public const string MUTEX_NAME = "GodotHub-AdminMutex-Debug";

		public const string DOWNLOAD_MAP_NAME = "GodotHub-DownloadContentMap-Debug";
		public const string ZIP_MAP_NAME = "GodotHub-ZipPathMap-Debug";
		public const string EXTRACT_MAP_NAME = "GodotHub-ExtractDirMap-Debug";
		public const string JUMP_MAP_NAME = "GodotHub-JumpMap-Debug";
#else
		public const string MUTEX_NAME = "GodotHub-AdminMutex";

		public const string DOWNLOAD_MAP_NAME = "GodotHub-DownloadContentMap";
		public const string ZIP_MAP_NAME = "GodotHub-ZipPathMap";
		public const string EXTRACT_MAP_NAME = "GodotHub-ExtractDirMap";
		public const string JUMP_MAP_NAME = "GodotHub-JumpMap";
#endif //DEBUG

		public const string WRITE_ARGUMENT = "--write-zip";
		public const string EXTRACT_ARGUMENT = "--unzip";
	}
}
