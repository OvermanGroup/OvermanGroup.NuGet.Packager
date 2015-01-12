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
		private static readonly Regex mPackageFileRegex = new Regex("(?i)(Successfully created package '(?<FilePath>.*?)'.)");

		private ITaskItem mPackageOutput;
		private ITaskItem mPackageSymbols;
		private ITaskItem[] mFilesWritten;

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

		public virtual string MinClientVersion { get; set; }

		public virtual string ExtraArguments { get; set; }

		[Output]
		public virtual ITaskItem PackageOutput
		{
			get { return mPackageOutput ?? (mPackageOutput = DeterminePackage(false)); }
		}

		[Output]
		public virtual ITaskItem PackageSymbols
		{
			get { return mPackageSymbols ?? (mPackageSymbols = DeterminePackage(true)); }
		}

		[Output]
		public virtual ITaskItem[] FilesWritten
		{
			get { return mFilesWritten ?? (mFilesWritten = new[] { PackageOutput, PackageSymbols }.Where(_ => _ != null).ToArray()); }
		}

		#endregion

		protected override string NuGetVerb
		{
			get { return "pack"; }
		}

		protected override void LogArguments(LogArgumentHandler logger)
		{
			var excludes = String.Join(";", (Exclude ?? Enumerable.Empty<ITaskItem>())
				.Select(_ => _.ItemSpec)
				.ToArray());

			logger("InputFile", InputFile);
			logger("OutputDirectory", OutputDirectory);
			logger("BasePath", BasePath);
			logger("Version", Version);
			logger("Exclude", excludes);
			logger("Symbols", Symbols);
			logger("Tool", Tool);
			logger("NoDefaultExcludes", NoDefaultExcludes);
			logger("NoPackageAnalysis", NoPackageAnalysis);
			logger("IncludeReferencedProjects", IncludeReferencedProjects);
			logger("ExcludeEmptyDirectories", ExcludeEmptyDirectories);
			logger("Verbosity", Verbosity);
			logger("MinClientVersion", MinClientVersion);
			logger("ExtraArguments", ExtraArguments);
		}

		protected override string GenerateCommandLineCommands()
		{
			var builder = new CommandLineBuilder();
			builder.AppendSwitch(NuGetVerb);

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

			var match = mPackageFileRegex.Match(singleLine);
			if (!match.Success) return;

			var file = match.Groups["FilePath"].Value;
			Logger.LogMessage(messageImportance, "Found package '{0}' by parsing NuGet.exe output.", file);

			var title = Path.GetFileNameWithoutExtension(file);
			var isSymbols = title.EndsWith("symbols", StringComparison.OrdinalIgnoreCase);

			var item = new TaskItem(file);
			if (isSymbols)
				mPackageSymbols = item;
			else
				mPackageOutput = item;
		}

		public virtual ITaskItem DeterminePackage(bool symbols)
		{
			if (ExitCode != 0)
			{
				Logger.LogWarning("Unable to determine package since NuGet.exe exited with a non-zero code.");
				return null;
			}
			if (symbols && !Symbols)
			{
				// don't bother searching for symbols if we didn't generate them
				return null;
			}

			var version = Version;
			var title = Path.GetFileNameWithoutExtension(InputFile) ?? InputFile;
			var filter = String.IsNullOrEmpty(version)
				? title + "*.nupkg"
				: title + "." + version + "*.nupkg";

			var dir = String.IsNullOrEmpty(OutputDirectory)
				? Environment.CurrentDirectory
				: OutputDirectory;

			Logger.LogMessage("Searching for packages using filter '{0}' in directory '{1}' with symbols={2}.", filter, dir, symbols);

			var package = Directory
				.EnumerateFiles(dir, filter, SearchOption.TopDirectoryOnly)
				.Where(path => (Path.GetFileNameWithoutExtension(path) ?? path).EndsWith(".symbols", StringComparison.OrdinalIgnoreCase) == symbols)
				.OrderByDescending(File.GetCreationTimeUtc)
				.Select(_ => new TaskItem(_))
				.FirstOrDefault();

			return package;
		}

		public override bool Execute()
		{
			var name = Path.GetFileName(InputFile);
			Logger.LogMessage(MainMessageImportance, "Creating NuGet package for '{0}'...", name);

			var success = base.Execute();
			if (!success) return false;

			foreach (var item in FilesWritten)
			{
				Logger.LogMessage(MainMessageImportance, "Successfully created NuGet package '{0}'.", item);
			}

			return true;
		}

	}
}