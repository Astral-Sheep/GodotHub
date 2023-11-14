using Com.Astral.GodotHub.Core.Data;
using Godot;

namespace Com.Astral.GodotHub.Core.Settings.Buttons
{
	public abstract partial class SettingButton : Control, ISettingButton
	{
		[Export] protected Button button;

		public override void _Ready()
		{
			Reset();
			AppConfig.Reset += Reset;
		}

		protected override void Dispose(bool pDisposing)
		{
			if (!pDisposing)
				return;

			AppConfig.Reset -= Reset;
		}

		public abstract void Connect();
		public abstract void Disconnect();
		protected abstract void Reset();
	}
}
