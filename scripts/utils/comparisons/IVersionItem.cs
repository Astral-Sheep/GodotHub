using Com.Astral.GodotHub.Data;

namespace Com.Astral.GodotHub.Utils.Comparisons
{
	public interface IVersionItem
	{
		/// <summary>
		/// <see cref="Data.Version"/> of the item
		/// </summary>
		Version Version { get; }
	}
}
