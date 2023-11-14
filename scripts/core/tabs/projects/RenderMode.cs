namespace Com.Astral.GodotHub.Tabs.Projects
{
	public enum RenderMode : int
	{
		OpenGL2 = 0b000,
		OpenGL3 = 0b001,
		Forward = 0b100,
		Mobile = 0b101,
		Compatibility = 0b110,
		/// <summary>
		/// This is not a render mode but a way to separate older modes from newer ones
		/// </summary>
		Config5 = 0b100,
	}
}
