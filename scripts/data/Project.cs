namespace Com.Astral.GodotHub.Data
{
	public struct Project
	{
		public string Path { get; private set; }
		public bool IsFavorite { get; private set; }
		public Version Version { get; private set; }

		public Project(string pPath, bool pIsFavorite, Version pVersion)
		{
			Path = pPath;
			IsFavorite = pIsFavorite;
			Version = pVersion;
		}
	}
}
