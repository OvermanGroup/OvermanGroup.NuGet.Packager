using System.IO;
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

		public virtual string ConfigFile { get; set; }

		public virtual string PushArguments { get; set; }

		#endregion

		protected override string NuGetVerb
		{
			get { return "push"; }
		}

		protected override void LogArguments(LogArgumentHandler logger)
		{
			logger("PackagePath", PackagePath);
			logger("Source", Source);
			logger("ApiKey", ApiKey);
			logger("ConfigFile", ConfigFile);
			logger("PushArguments", PushArguments);
		}

		protected override string GenerateCommandLineCommands()
		{
			var builder = new CommandLineBuilder();
			builder.AppendSwitch(NuGetVerb);

			builder.AppendFileNameIfNotNull(PackagePath);
			builder.AppendSwitch("-NonInteractive");
			builder.AppendSwitchIfNotNull("-Source ", Source);
			builder.AppendSwitchIfNotNull("-ApiKey ", ApiKey);
			builder.AppendSwitchIfNotNull("-Verbosity ", Verbosity);
			builder.AppendSwitchIfNotNull("-ConfigFile ", ConfigFile);
			builder.AppendSwitchIfNotNullOrEmpty(PushArguments);

			return builder.ToString();
		}

		public override bool Execute()
		{
			var name = Path.GetFileName(PackagePath.ItemSpec);
			Logger.LogMessage(MainMessageImportance, "Publishing NuGet package '{0}'...", name);

			var success = base.Execute();
			if (success)
				Logger.LogMessage(MainMessageImportance, "Successfully published NuGet package to '{0}'.", Source);

			return success;
		}
	}
}