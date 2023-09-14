using Com.Astral.GodotHub.Debug;
using Newtonsoft.Json;
using Octokit;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace Com.Astral.GodotHub.Data
{
	public static class GDRepository
	{
		private const string USER = "godotengine";
		private const string REPO = "godot";
		private const string PRODUCT_NAME = "GodotHub";

		/// <summary>
		/// Event called when the initial loading is completed
		/// </summary>
		public static event Action Loaded;
		/// <summary>
		/// Event called when the list of releases is updated. Only new releases are sent
		/// </summary>
		public static event Action<List<Release>> Updated;

		private static readonly string releasesPath = PathT.appdata + "/releases.json";

		public static List<Release> Releases => _releases;
		private static List<Release> _releases;
		private static GitHubClient client;

		public async static Task<bool> Init()
		{
			if (client != null)
				return true;

			client = new GitHubClient(new ProductHeaderValue(PRODUCT_NAME));

			try
			{
				LoadReleases();

				if (Config.AutoUpdateRepository || _releases == null)
				{
					await UpdateReleases();
				}

				Loaded?.Invoke();
				Debugger.PrintValidation("Releases loaded successfully");
				return true;
			}
			catch (Exception lException)
			{
				Debugger.PrintException(lException);
				return false;
			}
		}

		/// <summary>
		/// Check if there are new releases and retrieve them
		/// </summary>
		public async static Task UpdateReleases()
		{
			if (_releases == null)
			{
				try
				{
					_releases = ReadOnlyListToList(await client.Repository.Release.GetAll(USER, REPO));
					SaveReleases();
					Debugger.PrintMessage($"{_releases.Count} new releases found");
				}
				catch (Exception lException)
				{
					Debugger.PrintException(lException);
				}

				return;
			}

			try
			{
				if (_releases[0] == await client.Repository.Release.GetLatest(USER, REPO))
					return;
			}
			catch (Exception lException)
			{
				Debugger.PrintException(lException);
				return;
			}

			try
			{
				int lCount = _releases.Count;
				_releases = ReadOnlyListToList(await client.Repository.Release.GetAll(USER, REPO));
				SaveReleases();

				lCount = _releases.Count - lCount;
				Updated?.Invoke(_releases.GetRange(0, lCount));
				Debugger.PrintMessage($"{lCount} new releases found");
			}
			catch (Exception lException)
			{
				Debugger.PrintException(lException);
			}
		}

		private static void SaveReleases()
		{
			try
			{
				StreamWriter lWriter = new StreamWriter(releasesPath, false);
				lWriter.Write(JsonConvert.SerializeObject(_releases));
				lWriter.Close();
			}
			catch (Exception lException)
			{
				Debugger.PrintException(lException);
			}
		}

		private static void LoadReleases()
		{
			if (!File.Exists(releasesPath))
				return;

			try
			{
				StreamReader lReader = new StreamReader(releasesPath);
				_releases = JsonConvert.DeserializeObject<List<Release>>(
					lReader.ReadToEnd(),
					new JsonSerializerSettings() {
						ContractResolver = new FullParameterContractResolver()
					}
				);
				lReader.Close();
			}
			catch (Exception lException)
			{
				Debugger.PrintException(lException);
			}
		}

		private static List<Release> ReadOnlyListToList(IReadOnlyList<Release> pList)
		{
			List<Release> lList = new List<Release>();

			for (int i = 0; i < pList.Count; i++)
			{
				lList.Add(pList[i]);
			}

			return lList;
		}
	}
}
