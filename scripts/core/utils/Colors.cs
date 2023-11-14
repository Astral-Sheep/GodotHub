using Godot;

namespace Com.Astral.GodotHub.Core.Utils
{
	public partial class Colors : Node
	{
		public static Colors Singleton { get; private set; }

		public Color White => _white;
		public Color Blue => _blue;
		public Color Green => _green;
		public Color Yellow => _yellow;
		public Color Red => _red;

		[Export] protected Color _white = new Color(0xB8B8B8FF);
		[Export] protected Color _blue = new Color(0x379DA6FF);
		[Export] protected Color _green = new Color(0x769656FF);
		[Export] protected Color _yellow = new Color(0xE7B44FFF);
		[Export] protected Color _red = new Color(0xD4504FFF);

		public override void _EnterTree()
		{
			Singleton = this;
		}

		public override void _ExitTree()
		{
			Singleton = null;
		}

		/// <summary>
		/// Return a string of the color converted to hexadecimal
		/// </summary>
		public static string ToHexa(Color pColor)
		{
			return $"{pColor.R8:x2}{pColor.G8:x2}{pColor.B8:x2}";
		}
	}
}
