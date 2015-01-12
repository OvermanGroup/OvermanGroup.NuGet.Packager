using Microsoft.Build.Framework;

namespace OvermanGroup.NuGet.Packager
{
	public interface ILogger
	{
		void LogWarning(string format, params object[] args);

		void LogMessage(string format, params object[] args);

		void LogMessage(MessageImportance importance, string format, params object[] args);
	}
}