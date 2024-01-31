namespace Com.Astral.GodotHub.AdminInstall
{
	internal class InstallProgram
	{
		//arg[0] = --mode (--write_zip | --unzip)
		public static void Main(string[] pArgs)
		{
			using (Mutex lMutex = Mutex.OpenExisting(AdminInstallConstants.MUTEX_NAME))
			{
				if (pArgs.Length == 0)
				{
					Console.WriteLine("No argument given");
					Console.ReadKey();
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
				lMutex.WaitOne();

				try
				{
					AdminInstaller.WriteZip();
				}
				catch (Exception e)
				{
					Console.WriteLine($"Writing exception:\n{e}");
					Console.ReadKey();
					lMutex.ReleaseMutex();
					throw;
				}

				lMutex.ReleaseMutex();

			#endregion //WRITE
				
			#region EXTRACT

			EXTRACT:
				lMutex.WaitOne();

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
				catch (Exception e)
				{
					Console.WriteLine($"Extraction exception:\n{e}");
					Console.ReadKey();
					lMutex.ReleaseMutex();
					throw;
				}

				lMutex.ReleaseMutex();

				#endregion //EXTRACT

			#region DELETE

				lMutex.WaitOne();

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
				catch (Exception e)
				{
					Console.WriteLine($"Deletion exception:\n{e}");
					Console.ReadKey();
					lMutex.ReleaseMutex();
					throw;
				}

			#endregion //DELETE

			END:
				lMutex.ReleaseMutex();
			}
		}
	}
}
