using Com.Astral.GodotHub.Debug;
using Com.Astral.GodotHub.Releases;
using Godot;
using System;
using System.Runtime.InteropServices;
using Environment = System.Environment;

namespace Com.Astral.GodotHub
{
	public partial class Main : Node
	{
		public static Main Instance { get; private set; }

		public event Action Initialized;

		public OS OS { get; protected set; }
		public Architecture Architecture { get; protected set; }

		private Main() : base()
		{
			if (Instance != null)
			{
				GD.PushWarning($"{nameof(Main)} instance already exists, destroying the last added");
				Free();
				return;
			}

			Instance = this;

			switch (Environment.OSVersion.Platform)
			{
				case PlatformID.Win32NT:
					OS = OS.Windows;
					break;
				case PlatformID.Unix:
					OS = OS.Linux;
					break;
				case PlatformID.Other:
					OS = OS.MacOS;
					break;
			}

			Architecture = (int)RuntimeInformation.OSArchitecture % 2 == 1 ?
				Architecture.x64 :
				Architecture.x32;
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
