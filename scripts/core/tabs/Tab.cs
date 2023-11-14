using Godot;

namespace Com.Astral.GodotHub.Tabs
{
	public abstract partial class Tab : Control
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

		protected abstract void Connect();
		protected abstract void Disconnect();
	}
}
