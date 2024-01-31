using Com.Astral.GodotHub.Core.Data;
using Com.Astral.GodotHub.Core.Debug;
using Godot;

using Error = Com.Astral.GodotHub.Core.Utils.Error;

namespace Com.Astral.GodotHub.Core.Tabs
{
	public partial class UpdateRepoButton : Button
	{
		public override void _Ready()
		{
			Pressed += OnPressed;
		}

		protected async void OnPressed()
		{
			Error lError = await GDRepository.UpdateReleases();

			if (!lError.Ok)
			{
				Debugger.LogException(lError.Exception);
			}
		}
	}
}
