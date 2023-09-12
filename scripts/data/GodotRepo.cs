﻿using Com.Astral.GodotHub.Debug;
using Com.Astral.GodotHub.Settings;
using Newtonsoft.Json;
using Octokit;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace Com.Astral.GodotHub.Data
{
	public static class GodotRepo
	{
		private const string USER = "godotengine";
		private const string REPO = "godot";
		private const string PRODUCT_NAME = "GodotHub";

		public static event Action RepoRetrieved;

		private static readonly string releasesPath = Config.dataPath + "/releases.json";

		private static GitHubClient client;
		private static List<Release> releases;

		public async static Task<bool> Init()
		{
			client = new GitHubClient(new ProductHeaderValue(PRODUCT_NAME));

			try
			{
				LoadReleases();
				int lCount = releases == null ? 0 : releases.Count;
				await UpdateRepository();
				Debugger.PrintMessage($"{releases.Count - lCount} new releases found");
			}
			catch (Exception lException)
			{
				Debugger.PrintError($"{lException.GetType()}: {lException.Message}");
				return false;
			}

			RepoRetrieved?.Invoke();
			return true;
		}

		public async static Task UpdateRepository()
		{
			if (releases == null)
			{
				releases = ReadOnlyListToList(await client.Repository.Release.GetAll(USER, REPO));
				return;
			}

			if (releases[0] == await client.Repository.Release.GetLatest(USER, REPO))
				return;

			releases = ReadOnlyListToList(await client.Repository.Release.GetAll(USER, REPO));
			SaveReleases();
		}

		public static List<Release> GetReleases()
		{
			return releases;
		}

		private static void SaveReleases()
		{
			try
			{
				StreamWriter lWriter = new StreamWriter(releasesPath, false);
				lWriter.Write(JsonConvert.SerializeObject(releases));
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
				releases = JsonConvert.DeserializeObject<List<Release>>(
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
