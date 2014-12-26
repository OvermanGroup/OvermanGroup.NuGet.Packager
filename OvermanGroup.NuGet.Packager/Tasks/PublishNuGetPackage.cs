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

		public virtual ITaskItem ConfigFile { get; set; }

		public virtual string PushOptions { get; set; }

		#endregion

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
			builder.AppendTextUnquotedIfNotNullOrEmpty(PushOptions);

			return builder.ToString();
		}

	}
}