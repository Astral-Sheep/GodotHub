using Com.Astral.GodotHub.Data;
using Com.Astral.GodotHub.Debug;
using Godot;
using System;
using System.Text.RegularExpressions;

namespace Com.Astral.GodotHub
{
	public partial class Main : Node
	{
		public static Main Instance { get; private set; }

		public event Action Initialized;

		private Main() : base()
		{
			if (Instance != null)
			{
				GD.PushWarning($"{nameof(Main)} instance already exists, destroying the last added");
				Free();
				return;
			}

			Instance = this;
		}

		public override void _Ready()
		{
			DisplayServer.WindowSetMinSize(new Vector2I(1100, 600));
			Init();
		}

		protected override void Dispose(bool pDisposing)
		{
			if (pDisposing && Instance == this)
			{
				Instance = null;
			}
		}

		protected async void Init()
		{
			await GodotRepo.Init();
			Initialized?.Invoke();
		}
	}
}
