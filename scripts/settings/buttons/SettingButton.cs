using Godot;

namespace Com.Astral.GodotHub.Settings.Buttons
{
	public abstract partial class SettingButton : Control, ISettingButton
	{
		[Export] protected Button button;

		public override void _Ready()
		{
			Reset();
			Config.Reset += Reset;
		}

		protected override void Dispose(bool pDisposing)
		{
			if (pDisposing)
			{
				Config.Reset -= Reset;
			}
		}

		public abstract void Connect();
		public abstract void Disconnect();
		protected abstract void Reset();
	}
}
