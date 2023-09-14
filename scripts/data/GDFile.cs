namespace Com.Astral.GodotHub.Data
{
	/// <summary>
	/// Struct corresponding to an engine executable or a Godot project
	/// </summary>
	public struct GDFile
	{
		/// <summary>
		/// The absolute path to the file
		/// </summary>
		public string Path { get; private set; }
		/// <summary>
		/// Whether or not this file is marked as favorite by the user
		/// </summary>
		public bool IsFavorite { get; private set; }
		/// <summary>
		/// The version of the engine or the project (currently 0.0.0 for versions under 4.0)
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
