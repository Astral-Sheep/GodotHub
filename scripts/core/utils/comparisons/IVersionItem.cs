using Com.Astral.GodotHub.Core.Data;

namespace Com.Astral.GodotHub.Core.Utils.Comparisons
{
	public interface IVersionItem
	{
		/// <summary>
		/// <see cref="Data.Version"/> of the item
		/// </summary>
		Version Version { get; }
	}
}
