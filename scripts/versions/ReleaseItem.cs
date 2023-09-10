using Godot;
using Octokit;
using System;
using System.Collections.Generic;

namespace Com.Astral.GodotHub.Releases
{
	public partial class ReleaseItem : Control
	{
		protected const string RELEASE_NAME_SUFFIX = "-stable";
		protected const string ASSET_NAME_PREFIX = "Godot_v";
		protected const string FILE_TYPE = ".zip";

		public int Index { get; protected set; }
		public (int major, int minor, int patch) Version { get; protected set; }

		[Export] protected Godot.Label versionLabel;
		[Export] protected CheckBox monoCheck;
		[Export] protected OptionButton osButton;
		[Export] protected OptionButton bitButton;
		[Export] protected Button installButton;
		protected List<ReleaseAsset> assets;
		protected string assetNamePrefix;

		public override void _Ready()
		{
			AddItem(osButton, VersionInstaller.OS.Windows);
			AddItem(osButton, VersionInstaller.OS.Linux);
			AddItem(osButton, VersionInstaller.OS.MacOS);
			AddItem(bitButton, VersionInstaller.Bit.x32);
			AddItem(bitButton, VersionInstaller.Bit.x64);

			osButton.GetPopup().TransparentBg = true;
			bitButton.GetPopup().TransparentBg = true;

			monoCheck.Toggled += OnMonoChanged;
			osButton.ItemSelected += OnOsChanged;
			bitButton.ItemSelected += OnBitChanged;
			installButton.Pressed += OnInstallClicked;
		}

		public void Init(Release pRelease, int pIndex)
		{
			Index = pIndex;
			assets = new List<ReleaseAsset>();
			ReleaseAsset lAsset;

			for (int i = 0; i < pRelease.Assets.Count; i++)
			{
				lAsset = pRelease.Assets[i];

				if (lAsset.Name[0] != ASSET_NAME_PREFIX[0])
					continue;

				if (lAsset.Name[^4..^0] != FILE_TYPE)
					continue;

				assets.Add(lAsset);
			}

			//I hate it
			string lVersion = pRelease.Name[0..pRelease.Name.Find(RELEASE_NAME_SUFFIX)];
			Version = (
				int.Parse(lVersion[0].ToString()),
				int.Parse(lVersion[2].ToString()),
				lVersion.Length > 3 ? int.Parse(lVersion[4].ToString()) : 0
			);
			versionLabel.Text = $" Godot {lVersion}";
			assetNamePrefix = $"Godot_v{pRelease.Name}";
		}

		protected void OnMonoChanged(bool _)
		{
			SetInstallButton();
		}

		protected void OnOsChanged(long pOS)
		{
			bitButton.Visible = pOS != (long)VersionInstaller.OS.MacOS;
			SetInstallButton();
		}

		protected void OnBitChanged(long _)
		{
			SetInstallButton();
		}

		protected async void OnInstallClicked()
		{
			string lAssetName = GetAssetName(true);

			foreach (ReleaseAsset asset in assets)
			{
				if (asset.Name == lAssetName)
				{
					await VersionInstaller.InstallAsset(asset, monoCheck.ButtonPressed);
				}
			}
		}

		protected void AddItem(OptionButton pButton, Enum pItem)
		{
			pButton.AddItem(pItem.ToString(), Convert.ToInt32(pItem));
		}

		protected string GetAssetName(bool pIsFile)
		{
			string lName = assetNamePrefix;

			if (monoCheck.ButtonPressed)
			{
				lName += "_mono";
			}

			lName += GetOS();

			if (pIsFile)
			{
				lName += FILE_TYPE;
			}

			return lName;
		}

		protected string GetOS()
		{
			if (osButton.Selected == (int)VersionInstaller.OS.MacOS)
			{
				return "macos.universal";
			}

			string lOS;

			if (osButton.Selected == (int)VersionInstaller.OS.Linux)
			{
				lOS = $"_linux";
				lOS += monoCheck.ButtonPressed ? "_86" : ".86";
				lOS += bitButton.Selected == (int)VersionInstaller.Bit.x32 ? "_32" : "_64";
			}
			else
			{
				lOS = $"_win";
				lOS += bitButton.Selected == (int)VersionInstaller.Bit.x32 ? "32" : "64";

				if (!monoCheck.ButtonPressed)
				{
					lOS += ".exe";
				}
			}

			return lOS;
		}

		protected void SetInstallButton()
		{
			installButton.Disabled = VersionInstaller.IsInstalled(GetAssetName(false));
			installButton.Text = installButton.Disabled ? "Installed" : "Install";
		}
	}
}
