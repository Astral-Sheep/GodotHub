using Com.Astral.GodotHub.Debug;
using Godot;
using Octokit;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Com.Astral.GodotHub.Releases
{
	public partial class ReleaseItem : Control
	{
		protected const string RELEASE_NAME_SUFFIX = "-stable";
		protected const string ASSET_NAME_PREFIX = "Godot_v";
		protected const string FILE_TYPE = ".zip";

		public static event Action<Asset> InstallClicked;

		public int Index { get; protected set; }
		public (int major, int minor, int patch) Version { get; protected set; }

		[Export] protected Godot.Label versionLabel;
		[Export] protected Godot.Label dateLabel;
		[Export] protected CheckBox monoCheck;
		[Export] protected OptionButton osButton;
		[Export] protected OptionButton architectureButton;
		[Export] protected Button installButton;
		//protected List<ReleaseAsset> assets;
		protected List<Asset> assets;
		protected string assetNamePrefix;

		public override void _Ready()
		{
			AddItem(osButton, OS.Windows);
			AddItem(osButton, OS.Linux);
			AddItem(osButton, OS.MacOS);
			AddItem(architectureButton, Architecture.x32);
			AddItem(architectureButton, Architecture.x64);

			osButton.GetPopup().TransparentBg = true;
			architectureButton.GetPopup().TransparentBg = true;

			monoCheck.Toggled += OnMonoChanged;
			osButton.ItemSelected += OnOsChanged;
			architectureButton.ItemSelected += OnArchitectureChanged;
			installButton.Pressed += OnInstallClicked;

			osButton.Selected = (int)Main.Instance.OS;
			architectureButton.Selected = (int)Main.Instance.Architecture;
		}

		public void Init(Release pRelease, int pIndex)
		{
			Index = pIndex;
			assetNamePrefix = $"Godot_v{pRelease.Name}";
			string lVersion = pRelease.Name[0..pRelease.Name.Find(RELEASE_NAME_SUFFIX)];
			Version = (
				int.Parse(lVersion[0].ToString()),
				int.Parse(lVersion[2].ToString()),
				lVersion.Length > 3 ? int.Parse(lVersion[4].ToString()) : 0
			);
			Asset.GetVersion(pRelease);

			//assets = new List<ReleaseAsset>();
			assets = new List<Asset>();
			ReleaseAsset lReleaseAsset;

			for (int i = 0; i < pRelease.Assets.Count; i++)
			{
				lReleaseAsset = pRelease.Assets[i];

				if (lReleaseAsset.Name[0] != ASSET_NAME_PREFIX[0])
					continue;

				if (lReleaseAsset.Name[^4..^0] != FILE_TYPE)
					continue;

				//assets.Add(lReleaseAsset);
				assets.Add(GetAsset(lReleaseAsset));
			}

			//I hate it
			versionLabel.Text = $"Godot {lVersion}";
			dateLabel.Text = $"{pRelease.CreatedAt.Day:D2}/{pRelease.CreatedAt.Month:D2}/{pRelease.CreatedAt.Year:D4}";
			SetInstallButton();
		}

		protected void OnMonoChanged(bool _)
		{
			SetInstallButton();
		}

		protected void OnOsChanged(long pOS)
		{
			architectureButton.Visible = pOS != (long)OS.MacOS;
			SetInstallButton();
		}

		protected void OnArchitectureChanged(long _)
		{
			SetInstallButton();
		}

		protected void OnInstallClicked()
		{
			string lAssetName = GetAssetName(true);

			foreach (Asset asset in assets)
			{
				if (asset.asset.Name == lAssetName)
				{
					SetInstalling();
					InstallClicked?.Invoke(asset);
					return;
				}
			}

			Debugger.PrintError($"No asset named {lAssetName}");
		}

		protected void AddItem(OptionButton pButton, Enum pItem)
		{
			pButton.AddItem(pItem.ToString(), Convert.ToInt32(pItem));
		}

		protected Asset GetAsset(ReleaseAsset pAsset)
		{
			return new Asset(
				pAsset,
				Version,
				Asset.IsMono(pAsset),
				Asset.GetOS(pAsset),
				Asset.GetArchitecture(pAsset)
			);
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
			if (osButton.Selected == (int)OS.MacOS)
			{
				return "_macos.universal";
			}

			string lOS;

			if (osButton.Selected == (int)OS.Linux)
			{
				lOS = $"_linux";
				lOS += monoCheck.ButtonPressed ? "_x86" : ".x86";
				lOS += architectureButton.Selected == (int)Architecture.x32 ? "_32" : "_64";
			}
			else
			{
				lOS = $"_win";
				lOS += architectureButton.Selected == (int)Architecture.x32 ? "32" : "64";

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

		protected void SetInstalling()
		{
			installButton.Disabled = true;
			installButton.Text = "Installing...";
		}
	}
}
