using System;
using Microsoft.Build.Framework;

namespace OvermanGroup.NuGet.Packager
{
	public class Logger : ILogger
	{
		private readonly IBuildEngine mEngine;
		private readonly MessageImportance mDefaultImportance;

		public Logger(IBuildEngine engine, MessageImportance defaultImportance)
		{
			mEngine = engine;
			mDefaultImportance = defaultImportance;
		}

		#region ILogger Members

		public void LogWarning(string format, params object[] args)
		{
			var message = "OverPack: " + String.Format(format, args);
			mEngine.LogWarningEvent(new BuildWarningEventArgs("OverPack", null, null, 0, 0, 0, 0, message, "NuGet", "OverPack"));
		}

		public virtual void LogMessage(string format, params object[] args)
		{
			LogMessage(mDefaultImportance, format, args);
		}

		public virtual void LogMessage(MessageImportance importance, string format, params object[] args)
		{
			var message = "OverPack: " + String.Format(format, args);
			mEngine.LogMessageEvent(new BuildMessageEventArgs(message, "NuGet", "OverPack", importance));
		}

		#endregion
	}
}