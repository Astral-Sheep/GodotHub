using Godot;

namespace Com.Astral.GodotHub.Tabs
{
	public partial class Tab : Control
	{
		public virtual void Toggle(bool pToggled)
		{
			if (Visible == pToggled)
				return;

			if (pToggled)
			{
				Visible = true;
				Connect();
			}
			else
			{
				Disconnect();
				Visible = false;
			}
		}

		protected virtual void Connect() { }
		protected virtual void Disconnect() { }
	}
}
