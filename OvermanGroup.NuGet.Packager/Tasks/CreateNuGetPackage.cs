using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;

namespace OvermanGroup.NuGet.Packager.Tasks
{
	public class CreateNuGetPackage : NuGetTask
	{
		private readonly Regex mPackageFileRegex = new Regex("(?i)(Successfully created package '(?<FilePath>.*?)'.)");
		private ITaskItem mPackageOutput;

		#region Properties

		[Required]
		public virtual string InputFile { get; set; }

		public virtual string OutputDirectory { get; set; }

		public virtual string BasePath { get; set; }

		public virtual string Version { get; set; }

		public virtual ITaskItem[] Exclude { get; set; }

		public virtual bool Symbols { get; set; }

		public virtual bool Tool { get; set; }

		public virtual bool NoDefaultExcludes { get; set; }

		public virtual bool NoPackageAnalysis { get; set; }

		public virtual bool IncludeReferencedProjects { get; set; }

		public virtual bool ExcludeEmptyDirectories { get; set; }

		public virtual string Verbosity { get; set; }

		public virtual string MinClientVersion { get; set; }

		public virtual string ExtraArguments { get; set; }

		[Output]
		public virtual ITaskItem PackageOutput
		{
			get { return mPackageOutput ?? (mPackageOutput = GetPackageOutput()); }
			set { mPackageOutput = value; }
		}

		#endregion

		protected override void LogArguments(MessageImportance importance)
		{
			var excludes = String.Join(";", (Exclude ?? Enumerable.Empty<ITaskItem>())
				.Select(_ => _.ItemSpec)
				.ToArray());

			Log.LogMessage(importance, "InputFile: {0}", InputFile);
			Log.LogMessage(importance, "OutputDirectory: {0}", OutputDirectory);
			Log.LogMessage(importance, "BasePath: {0}", BasePath);
			Log.LogMessage(importance, "Version: {0}", Version);
			Log.LogMessage(importance, "Exclude: {0}", excludes);
			Log.LogMessage(importance, "Symbols: {0}", Symbols);
			Log.LogMessage(importance, "Tool: {0}", Tool);
			Log.LogMessage(importance, "NoDefaultExcludes: {0}", NoDefaultExcludes);
			Log.LogMessage(importance, "NoPackageAnalysis: {0}", NoPackageAnalysis);
			Log.LogMessage(importance, "IncludeReferencedProjects: {0}", IncludeReferencedProjects);
			Log.LogMessage(importance, "ExcludeEmptyDirectories: {0}", ExcludeEmptyDirectories);
			Log.LogMessage(importance, "Verbosity: {0}", Verbosity);
			Log.LogMessage(importance, "MinClientVersion: {0}", MinClientVersion);
			Log.LogMessage(importance, "ExtraArguments: {0}", ExtraArguments);
		}

		protected override string GenerateCommandLineCommands()
		{
			var builder = new CommandLineBuilder();
			builder.AppendSwitch("pack");

			// We don't allow the 'Build' argument because an infinite loop will occur
			// because Build will trigger our Post-Build which then will trigger
			// another Build again and again. Also, since 'Build' isn't allowed, then
			// the 'Properties' argument isn't needed either.
			// http://nuget.codeplex.com/workitem/1036

			var extraArguments = SanitizeExtraArguments();

			builder.AppendFileNameIfNotNull(InputFile);
			builder.AppendSwitch("-NonInteractive");
			builder.AppendSwitchIfNotNullOrEmpty("-OutputDirectory ", OutputDirectory);
			builder.AppendSwitchIfNotNullOrEmpty("-BasePath ", BasePath);
			builder.AppendSwitchIfNotNullOrEmpty("-Version ", Version);
			builder.AppendSwitchIfNotNull("-Exclude ", Exclude, ";");
			builder.AppendSwitchIfTrue("-Symbols", Symbols);
			builder.AppendSwitchIfTrue("-Tool", Tool);
			builder.AppendSwitchIfTrue("-NoDefaultExcludes", NoDefaultExcludes);
			builder.AppendSwitchIfTrue("-NoPackageAnalysis", NoPackageAnalysis);
			builder.AppendSwitchIfTrue("-IncludeReferencedProjects", IncludeReferencedProjects);
			builder.AppendSwitchIfTrue("-ExcludeEmptyDirectories", ExcludeEmptyDirectories);
			builder.AppendSwitchIfNotNullOrEmpty("-Verbosity ", Verbosity);
			builder.AppendSwitchIfNotNullOrEmpty("-MinClientVersion ", MinClientVersion);
			builder.AppendSwitchIfNotNullOrEmpty(extraArguments);

			return builder.ToString();
		}

		public virtual string SanitizeExtraArguments()
		{
			var extraArguments = ExtraArguments;
			return String.IsNullOrEmpty(extraArguments) ? null
				: extraArguments.Replace("-Build", String.Empty).Trim();
		}

		protected override void LogEventsFromTextOutput(string singleLine, MessageImportance messageImportance)
		{
			base.LogEventsFromTextOutput(singleLine, messageImportance);

			var item = mPackageOutput;
			if (item != null && !String.IsNullOrEmpty(item.ItemSpec)) return;

			var match = mPackageFileRegex.Match(singleLine);
			if (!match.Success) return;

			var file = match.Groups["FilePath"].Value;
			Log.LogMessage(Constants.MessageImportance, "Detected creation of package '{0}' by parsing NuGet.exe output.", file);

			mPackageOutput = new TaskItem(file);
		}

		public virtual ITaskItem GetPackageOutput()
		{
			var dir = String.IsNullOrEmpty(OutputDirectory) ? Environment.CurrentDirectory : OutputDirectory;
			var files = Directory.GetFiles(dir, "*.nupkg");
			switch (files.Length)
			{
				case 0:
					Log.LogWarning("Unable to determine package output.");
					return null;

				case 1:
					Log.LogMessage(Constants.MessageImportance, "Detected creation of package '{0}' from searching '*.nupkg' with a single result.", files[0]);
					return new TaskItem(files[0]);
			}

			var prefix = Path.GetFileNameWithoutExtension(InputFile) ?? InputFile;
			var file = files.Where(_ => _.StartsWith(prefix)).OrderByDescending(File.GetCreationTimeUtc).First();
			Log.LogMessage(Constants.MessageImportance, "Detected creation of package '{0}' from searching '*.nupkg' with the latest result.", file);

			var item = new TaskItem(file);
			return item;
		}

	}
}