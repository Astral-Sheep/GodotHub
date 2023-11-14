namespace Com.Astral.GodotHub.AdminInstall
{
	internal class Program
	{
		private static readonly Mutex mutex;

		static Program()
		{
			mutex = Mutex.OpenExisting(AdminInstallConstants.MUTEX_NAME);
		}

		//arg[0] = --mode (--write_zip | --unzip)
		public static void Main(string[] pArgs)
		{
			AppDomain.CurrentDomain.ProcessExit += new EventHandler(OnProcessExit);
			AppDomain.CurrentDomain.UnhandledException += new UnhandledExceptionEventHandler(OnUnhandledException);

			if (pArgs.Length == 0)
			{
				throw new ArgumentException("No argument given");
			}

			switch (pArgs[0])
			{
				case AdminInstallConstants.WRITE_ARGUMENT:
					goto WRITE;
				case AdminInstallConstants.EXTRACT_ARGUMENT:
					goto EXTRACT;
				default:
					throw new ArgumentException($"Invalid mode given: {pArgs[0]}");
			}

			#region WRITE

		WRITE:
			mutex.WaitOne();

			try
			{
				AdminInstaller.WriteZip();
			}
			catch (Exception)
			{
				mutex.ReleaseMutex();
				throw;
			}

			mutex.ReleaseMutex();

			#endregion //WRITE

			#region EXTRACT

		EXTRACT:
			mutex.WaitOne();

			switch (AdminInstaller.Jump())
			{
				case (byte)JumpInstruction.Delete:
					goto DELETE;
				case (byte)JumpInstruction.Cancel:
					AdminInstaller.CancelInstall(true, false);
					goto END;
				default:
					break;
			}

			try
			{
				AdminInstaller.Extract();
			}
			catch (Exception)
			{
				mutex.ReleaseMutex();
				throw;
			}

			mutex.ReleaseMutex();

			#endregion //EXTRACT

			#region DELETE

			mutex.WaitOne();

			if (AdminInstaller.Jump() == (byte)JumpInstruction.Cancel)
			{
				AdminInstaller.CancelInstall(true, true);
				goto END;
			}

		DELETE:
			try
			{
				AdminInstaller.DeleteZip();
			}
			catch (Exception)
			{
				mutex.ReleaseMutex();
				throw;
			}

			#endregion //DELETE

		END:
			mutex.ReleaseMutex();
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

		private static void OnUnhandledException(object pSender, EventArgs pArgs)
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
	}
}
