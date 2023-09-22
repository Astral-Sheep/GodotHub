using System;

namespace Com.Astral.GodotHub.Data
{
	public struct Error
	{
		public bool Ok => exception == null;
		public Exception exception;
		public string message;

		public Error(Exception pException = null)
		{
			exception = pException;
			message = exception.Message;
		}
	}
}
