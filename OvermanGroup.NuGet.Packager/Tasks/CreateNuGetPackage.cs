using System;
using System.Linq;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;

namespace OvermanGroup.NuGet.Packager.Tasks
{
	public class CreateNuGetPackage : NuGetTask
	{
		#region Properties

		[Required]
		public virtual string ProjectPath { get; set; }

		public virtual string OutputDirectory { get; set; }

		public virtual string BasePath { get; set; }

		public virtual string Version { get; set; }

		public virtual ITaskItem[] Exclude { get; set; }

		public virtual bool Symbols { get; set; }

		public virtual bool Tool { get; set; }

		public virtual bool Build { get; set; }

		public virtual bool NoDefaultExcludes { get; set; }

		public virtual bool NoPackageAnalysis { get; set; }

		public virtual bool IncludeReferencedProjects { get; set; }

		public virtual bool ExcludeEmptyDirectories { get; set; }

		public virtual ITaskItem[] Properties { get; set; }

		public virtual string Verbosity { get; set; }

		public virtual string MinClientVersion { get; set; }

		public virtual string ConfigurationName { get; set; }

		public virtual string PlatformName { get; set; }

		public virtual string ExtraArguments { get; set; }

		#endregion

		protected override string GenerateCommandLineCommands()
		{
			var builder = new CommandLineBuilder();
			builder.AppendSwitch("pack");

			var properties = (Properties ?? Enumerable.Empty<ITaskItem>()).ToList();

			var configurationName = ConfigurationName;
			if (!String.IsNullOrWhiteSpace(configurationName))
			{
				properties.RemoveAll(_ => _.ItemSpec.StartsWith("Configuration=", StringComparison.OrdinalIgnoreCase));
				properties.Add(new TaskItem(String.Format("Configuration=\"{0}\" ", configurationName)));
			}

			var platformName = PlatformName;
			if (!String.IsNullOrWhiteSpace(platformName))
			{
				properties.RemoveAll(_ => _.ItemSpec.StartsWith("Platform=", StringComparison.OrdinalIgnoreCase));
				properties.Add(new TaskItem(String.Format("Platform=\"{0}\" ", platformName)));
			}

			builder.AppendFileNameIfNotNull(ProjectPath);
			builder.AppendSwitch("-NonInteractive ");
			builder.AppendSwitchIfNotNullOrEmpty("-OutputDirectory ", OutputDirectory);
			builder.AppendSwitchIfNotNullOrEmpty("-BasePath ", BasePath);
			builder.AppendSwitchIfNotNullOrEmpty("-Version ", Version);
			builder.AppendSwitchIfNotNull("-Exclude ", Exclude, ";");
			builder.AppendSwitchIfTrue("-Symbols", Symbols);
			builder.AppendSwitchIfTrue("-Tool", Tool);
			builder.AppendSwitchIfTrue("-Build", Build);
			builder.AppendSwitchIfTrue("-NoDefaultExcludes", NoDefaultExcludes);
			builder.AppendSwitchIfTrue("-NoPackageAnalysis", NoPackageAnalysis);
			builder.AppendSwitchIfTrue("-IncludeReferencedProjects", IncludeReferencedProjects);
			builder.AppendSwitchIfTrue("-ExcludeEmptyDirectories", ExcludeEmptyDirectories);
			builder.AppendSwitchIfNotNull("-Properties ", properties.ToArray(), ";");
			builder.AppendSwitchIfNotNullOrEmpty("-Verbosity ", Verbosity);
			builder.AppendSwitchIfNotNullOrEmpty("-MinClientVersion ", MinClientVersion);
			builder.AppendTextUnquotedIfNotNullOrEmpty(ExtraArguments);

			return builder.ToString();
		}

	}
}