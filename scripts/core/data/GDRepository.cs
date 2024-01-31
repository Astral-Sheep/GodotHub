using Com.Astral.GodotHub.Core.Debug;
using Com.Astral.GodotHub.Core.Utils;
using Newtonsoft.Json;
using Octokit;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace Com.Astral.GodotHub.Core.Data
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

		/// <summary>
		/// Initialize the <see cref="GitHubClient"/> and load the repositories<br/>
		/// Can be called only once (won't do anything otherwise)<br/>
		/// Exceptions are handled by returning an <see cref="Error"/>
		/// </summary>
		/// <exception cref="ApiException"></exception>
		/// <exception cref="DirectoryNotFoundException"></exception>
		/// <exception cref="FileNotFoundException"></exception>
		/// <exception cref="IOException"></exception>
		/// <exception cref="PathTooLongException"></exception>
		/// <exception cref="System.Security.SecurityException"></exception>
		/// <exception cref="UnauthorizedAccessException"></exception>
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
					Error lError = await UpdateReleases();

					if (!lError.Ok)
					{
						return lError;
					}
				}

				Loaded?.Invoke();
				Debugger.LogValidation("Releases loaded successfully");
				return new Error();
			}
			catch (Exception lException)
			{
				return new Error(lException);
			}
		}

		/// <summary>
		/// Check if there are new releases and retrieve them<br/>
		/// Exceptions are handled by returning an <see cref="Error"/>
		/// </summary>
		/// <exception cref="ApiException"></exception>
		/// <exception cref="DirectoryNotFoundException"></exception>
		/// <exception cref="FileNotFoundException"></exception>
		/// <exception cref="IOException"></exception>
		/// <exception cref="PathTooLongException"></exception>
		/// <exception cref="System.Security.SecurityException"></exception>
		/// <exception cref="UnauthorizedAccessException"></exception>
		public async static Task<Error> UpdateReleases()
		{
			if (Releases == null)
			{
				try
				{
					Releases = ReadOnlyListToList(await client.Repository.Release.GetAll(USER, REPO));

					for (int i = Releases.Count - 1; i >= 0; i--)
					{
						if ((Version)Releases[i].TagName < Version.minimumSupportedVersion)
						{
							Releases.RemoveAt(i);
						}
					}

					SaveReleases();
					Debugger.LogMessage($"{Releases.Count} new releases found");
				}
				catch (Exception lException)
				{
					Debugger.LogException(lException);
				}

				return new Error();
			}

			try
			{
				if (Releases[0] == await client.Repository.Release.GetLatest(USER, REPO))
					return new Error();
			}
			catch (Exception lException)
			{
				Debugger.LogException(lException);
				return new Error(lException);
			}

			try
			{
				List<Release> lReleases = ReadOnlyListToList(await client.Repository.Release.GetAll(USER, REPO));
				int lLastIndex = 0;
				string lLastName = Releases[0].TagName;

				for (lLastIndex = 0; lLastIndex < lReleases.Count; lLastIndex++)
				{
					if (lReleases[lLastIndex].TagName == lLastName)
						break;
				}

				Releases.InsertRange(0, lReleases.GetRange(0, lLastIndex));				
				SaveReleases();
				Updated?.Invoke(Releases.GetRange(0, lLastIndex));
				Debugger.LogMessage($"{lLastIndex} new releases found");
			}
			catch (Exception lException)
			{
				Debugger.LogException(lException);
				return new Error(lException);
			}

			return new Error();
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
					new JsonSerializerSettings() {
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
