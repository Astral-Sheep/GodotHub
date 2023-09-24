using Com.Astral.GodotHub.Debug;
using Com.Astral.GodotHub.Utils;
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

		public static List<Release> Releases { get; private set; }
		private static GitHubClient client;

		public async static Task<Error> Init()
		{
			if (client != null)
			{
				return new Error();
			}

			client = new GitHubClient(new ProductHeaderValue(PRODUCT_NAME));

			try
			{
				LoadReleases();

				if (AppConfig.AutoUpdateRepository || Releases == null)
				{
					await UpdateReleases();
				}

				Loaded?.Invoke();
				Debugger.PrintValidation("Releases loaded successfully");
				return new Error();
			}
			catch (Exception lException)
			{
				return new Error(lException);
			}
		}

		/// <summary>
		/// Check if there are new releases and retrieve them
		/// </summary>
		public async static Task UpdateReleases()
		{
			if (Releases == null)
			{
				try
				{
					Releases = ReadOnlyListToList(await client.Repository.Release.GetAll(USER, REPO));

					for (int i = Releases.Count - 1; i >= 0; i++)
					{
						if ((Version)Releases[i].Name < Version.minimumSupportedVersion)
						{
							Releases.RemoveAt(i);
						}
					}

					SaveReleases();
					Debugger.PrintMessage($"{Releases.Count} new releases found");
				}
				catch (Exception lException)
				{
					//To do: create error popup
					Debugger.PrintException(lException);
				}

				return;
			}

			try
			{
				if (Releases[0] == await client.Repository.Release.GetLatest(USER, REPO))
					return;
			}
			catch (Exception lException)
			{
				//To do: create error popup
				Debugger.PrintException(lException);
				return;
			}

			try
			{
				List<Release> lReleases = ReadOnlyListToList(await client.Repository.Release.GetAll(USER, REPO));
				int lLastIndex = 0;
				string lLastName = Releases[0].Name;

				for (lLastIndex = 0; lLastIndex < lReleases.Count; lLastIndex++)
				{
					if (lReleases[lLastIndex].Name == lLastName)
						break;
				}

				Releases.InsertRange(0, lReleases.GetRange(0, lLastIndex));				
				SaveReleases();
				Updated?.Invoke(Releases.GetRange(0, lLastIndex));
				Debugger.PrintMessage($"{lLastIndex} new releases found");
			}
			catch (Exception lException)
			{
				//To do: create error popup
				Debugger.PrintException(lException);
			}
		}

		private static void SaveReleases()
		{
			using (StreamWriter lWriter = new StreamWriter(releasesPath, false))
			{
				lWriter.Write(JsonConvert.SerializeObject(Releases));
				lWriter.Close();
			}
		}

		private static void LoadReleases()
		{
			if (!File.Exists(releasesPath))
				return;

			using (StreamReader lReader = new StreamReader(releasesPath))
			{
				Releases = JsonConvert.DeserializeObject<List<Release>>(
					lReader.ReadToEnd(),
					new JsonSerializerSettings()
					{
						ContractResolver = new FullParameterContractResolver()
					}
				);
				lReader.Close();
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
