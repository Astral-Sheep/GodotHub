using System;

namespace Com.Astral.GodotHub.Utils
{
	/// <summary>
	/// Structure used to approximate a Rust enum: <c>Error(string)</c>
	/// </summary>
	public struct Error
	{
		/// <summary>
		/// If true, everything went well
		/// </summary>
		public bool Ok => _exception == null;
		/// <summary>
		/// The <see cref="Exception"/> that happened. <c>null</c> if everything went well
		/// </summary>
		public Exception Exception => _exception;
		/// <summary>
		/// Message that describes the current exception. Empty if there's no <see cref="Exception"/>
		/// </summary>
		public string Message => _exception.Message;

		private Exception _exception;

		public Error()
		{
			_exception = null;
		}

		public Error(Exception pException)
		{
			_exception = pException;
		}
	}
}
