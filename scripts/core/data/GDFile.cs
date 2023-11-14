namespace Com.Astral.GodotHub.Core.Data
{
	/// <summary>
	/// Struct corresponding to an engine executable or a Godot project
	/// </summary>
	public struct GDFile
	{
		/// <summary>
		/// Absolute path to the file
		/// </summary>
		public string Path { get; private set; }
		/// <summary>
		/// Whether or not this file is marked as favorite by the user
		/// </summary>
		public bool IsFavorite { get; private set; }
		/// <summary>
		/// <see cref="Version"/> of the engine or the project
		/// </summary>
		public Version Version { get; private set; }

		public GDFile(string pPath, bool pIsFavorite, Version pVersion)
		{
			Path = pPath;
			IsFavorite = pIsFavorite;
			Version = pVersion;
		}
	}
}
