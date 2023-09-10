using Com.Astral.GodotHub.Debug;
using Octokit;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Com.Astral.GodotHub.Releases
{
	public static class GodotRepo
	{
		private const string USER = "godotengine";
		private const string REPO = "godot";
		private const string PRODUCT_NAME = "GodotHub";

		public static event Action RepoRetrieved;

		private static GitHubClient client;
		private static IReadOnlyList<Release> releases;

		public async static Task<bool> Init()
		{
			client = new GitHubClient(new ProductHeaderValue(PRODUCT_NAME));
			
			try
			{
				await UpdateRepository();
				Debugger.PrintMessage($"{releases.Count} releases found");
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
			releases = await client.Repository.Release.GetAll(USER, REPO);
		}

		public static Release GetRelease(string pName)
		{
			foreach (Release release in releases)
			{
				if (release.Name == pName)
					return release;
			}

			return null;
		}

		public static Release GetLatestRelease()
		{
			return releases[0];
		}

		public static IReadOnlyList<Release> GetReleases()
		{
			return releases;
		}
	}
}
