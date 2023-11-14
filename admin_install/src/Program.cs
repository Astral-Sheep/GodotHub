namespace Com.Astral.GodotHub.AdminInstall
{
	internal class Program
	{
		private const string MUTEX_NAME = @"GodotHub-AdminMutex";

		private static readonly Mutex mutex;

		static Program()
		{
			mutex = Mutex.OpenExisting(MUTEX_NAME);
		}

		//arg[0] = --mode (--write_zip | --unzip)
		public static void Main(string[] pArgs)
		{
			AppDomain.CurrentDomain.ProcessExit += new EventHandler(OnProcessExit);

			if (pArgs.Length == 0)
			{
#if DEBUG
				Console.WriteLine("No argument given");
				Console.ReadKey();
#endif //DEBUG
				throw new ArgumentException("No argument given");
			}

			switch (pArgs[0])
			{
				case "--write_zip":
					goto WRITE;
				case "--extract":
					goto EXTRACT;
				default:
#if DEBUG
					Console.WriteLine($"Invalid mode given: {pArgs[0]}");
					Console.ReadKey();
#endif //DEBUG
					throw new ArgumentException($"Invalid mode given: {pArgs[0]}");
			}

			#region WRITE

		WRITE:
			Console.WriteLine("Waiting for mutex to write");
			//mutex.WaitOne();
			WaitForMutex();
			Console.WriteLine("Mutex recovered");
			try
			{
				AdminInstaller.WriteZip();
			}
			catch (Exception lException)
			{
				//mutex.ReleaseMutex();
				ReleaseMutex();
				Console.WriteLine("Failed to write zip");
				Console.WriteLine(lException.ToString());
				Console.ReadKey();
				throw;
			}

			//mutex.ReleaseMutex();
			ReleaseMutex();
			Console.WriteLine("Mutex released");

			#endregion //WRITE

			#region EXTRACT

			EXTRACT:
			Console.WriteLine("Waiting for mutex to extract");
			WaitForMutex();
			//mutex.WaitOne();
			Console.WriteLine("Mutex recovered");

			if (AdminInstaller.JumpToDelete())
			{
				goto DELETE;
			}

			try
			{
				AdminInstaller.Extract();
			}
			catch (Exception lException)
			{
				//mutex.ReleaseMutex();
				ReleaseMutex();
				Console.WriteLine("Failed to extract");
				Console.WriteLine(lException.ToString());
				Console.ReadKey();
				throw;
			}
			Console.WriteLine("Files extracted");
			ReleaseMutex();
			//mutex.ReleaseMutex();
			Console.WriteLine("Mutex released");

			#endregion //EXTRACT

			#region DELETE

			Console.WriteLine("Waiting for mutex to delete");
			WaitForMutex();
			//mutex.WaitOne();
			Console.WriteLine("Mutex recovered");

		DELETE:
			try
			{
				AdminInstaller.DeleteZip();
			}
			catch (Exception lException)
			{
				//mutex.ReleaseMutex();
				ReleaseMutex();
				Console.WriteLine("Failed to delete zip");
				Console.WriteLine(lException.ToString());
				Console.ReadKey();
				throw;
			}
			Console.WriteLine("Zip deleted");
			ReleaseMutex();
			//mutex.ReleaseMutex();
			Console.WriteLine("Mutex released");

			#endregion //DELETE

#if DEBUG
			Console.WriteLine("Process completed");
			Console.ReadKey();
#endif //DEBUG
		}

		private static void OnProcessExit(object? pSender, EventArgs pArgs)
		{
			try
			{
				mutex.ReleaseMutex();
			}
			catch (Exception)
			{
				//Mutex already released
			}
		}

		private static void ReleaseMutex()
		{
			try
			{
				Console.ReadKey();
				mutex.ReleaseMutex();
			}
			catch (Exception lException)
			{
				Console.WriteLine(lException);
				Console.ReadKey();
				throw;
			}
		}

		private static void WaitForMutex()
		{
			try
			{
				mutex.WaitOne();
			}
			catch (Exception lException)
			{
				Console.WriteLine(lException);
				Console.ReadKey();
				throw;
			}
		}
	}
}
