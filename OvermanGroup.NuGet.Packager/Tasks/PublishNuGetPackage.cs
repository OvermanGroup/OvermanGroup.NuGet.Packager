using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;

namespace OvermanGroup.NuGet.Packager.Tasks
{
	public class PublishNuGetPackage : NuGetTask
	{
		#region Properties

		[Required]
		public virtual ITaskItem PackagePath { get; set; }

		public virtual string Source { get; set; }

		public virtual string ApiKey { get; set; }

		public virtual string Verbosity { get; set; }

		public virtual string ConfigFile { get; set; }

		public virtual string PushArguments { get; set; }

		#endregion

		protected override void LogArguments(MessageImportance importance)
		{
			Log.LogMessage(importance, "PackagePath: {0}", PackagePath);
			Log.LogMessage(importance, "Source: {0}", Source);
			Log.LogMessage(importance, "ApiKey: {0}", ApiKey);
			Log.LogMessage(importance, "Verbosity: {0}", Verbosity);
			Log.LogMessage(importance, "ConfigFile: {0}", ConfigFile);
			Log.LogMessage(importance, "PushArguments: {0}", PushArguments);
		}

		protected override string GenerateCommandLineCommands()
		{
			var builder = new CommandLineBuilder();
			builder.AppendSwitch("push");

			builder.AppendFileNameIfNotNull(PackagePath);
			builder.AppendSwitch("-NonInteractive");
			builder.AppendSwitchIfNotNull("-Source ", Source);
			builder.AppendSwitchIfNotNull("-ApiKey ", ApiKey);
			builder.AppendSwitchIfNotNull("-Verbosity ", Verbosity);
			builder.AppendSwitchIfNotNull("-ConfigFile ", ConfigFile);
			builder.AppendSwitchIfNotNullOrEmpty(PushArguments);

			return builder.ToString();
		}

	}
}