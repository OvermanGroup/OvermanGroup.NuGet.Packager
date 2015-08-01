using System;
using System.IO;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;

namespace OvermanGroup.NuGet.Packager.Tasks
{
	public delegate void LogArgumentHandler(string argumentName, object argumentValue);

	public abstract class NuGetTask : ToolTask
	{
		private string mNuGetExePathSpecified;

		// This field is static because different task instances will need to use
		// the same path to NuGet, so optimize the resolution logic.
		private static string mNuGetExePathResolved;

		private ILogger mLogger;
		private MessageImportance? mMessageImportance;
		private MessageImportance? mMainMessageImportance;

		[Required]
		public virtual string SolutionDir { get; set; }

		[Required]
		public virtual string ProjectDir { get; set; }

		public virtual string Verbosity { get; set; }

		public virtual string NuGetExePath
		{
			get { return mNuGetExePathSpecified; }
			set { mNuGetExePathSpecified = value; }
		}

		protected virtual ILogger Logger
		{
			get { return mLogger ?? (mLogger = new Logger(BuildEngine, MessageImportance)); }
		}

		protected virtual MessageImportance MainMessageImportance
		{
			get
			{
				return (mMainMessageImportance ?? (mMainMessageImportance =
					String.Equals(Verbosity, "quiet", StringComparison.OrdinalIgnoreCase)
						? MessageImportance.Low
						: MessageImportance.High)).Value;
			}
		}

		protected virtual MessageImportance MessageImportance
		{
			get
			{
				if (mMessageImportance.HasValue)
					return mMessageImportance.Value;

				var messageImportance = MessageImportance.Normal;
				switch ((Verbosity ?? String.Empty).ToLowerInvariant())
				{
					case "detailed":
						messageImportance = MessageImportance.High;
						break;

					case "normal":
						messageImportance = MessageImportance.Normal;
						break;

					case "quiet:":
						messageImportance = MessageImportance.Low;
						break;
				}

				mMessageImportance = messageImportance;
				return messageImportance;
			}
		}

		protected override string ToolName
		{
			get { return Constants.NuGetFileName; }
		}

		public override string ToolExe
		{
			get { return Constants.NuGetFileName; }
		}

		protected override bool ValidateParameters()
		{
			StandardErrorImportance = "high";
			StandardOutputImportance = MessageImportance.ToString();
			return true;
		}

		protected override string GenerateFullPathToTool()
		{
			var nuGetExePath = mNuGetExePathSpecified;
			if (!String.IsNullOrEmpty(nuGetExePath) && File.Exists(nuGetExePath))
			{
				Logger.LogMessage(MessageImportance, "Using NuGet.exe from '{0}'.", nuGetExePath);
				mNuGetExePathResolved = nuGetExePath;
				return nuGetExePath;
			}

			nuGetExePath = mNuGetExePathResolved;
			if (!String.IsNullOrEmpty(nuGetExePath) && File.Exists(nuGetExePath))
				return nuGetExePath;

			var downloadDir = Path.GetDirectoryName(BuildEngine.ProjectFileOfTaskNode);
			var resolver = new NuGetExeResolver(Logger, SolutionDir, ProjectDir, downloadDir);
			nuGetExePath = resolver.GetNuGetExePath();
			mNuGetExePathResolved = nuGetExePath;

			return nuGetExePath ?? Constants.NuGetFileName;
		}

		protected override int ExecuteTool(string pathToTool, string responseFileCommands, string commandLineCommands)
		{
			var separator = new String('=', 80);
			LogArgumentHandler logger = (name, value) => Logger.LogMessage("{0}='{1}'", name, value);

			Logger.LogMessage(separator);
			logger("Verbosity", Verbosity);
			logger("SolutionDir", SolutionDir);
			logger("ProjectDir", ProjectDir);
			logger("NuGetExePathSpecified", mNuGetExePathSpecified);
			logger("NuGetExePathResolved", mNuGetExePathResolved);
			LogArguments(logger);

			Logger.LogMessage(separator);
			Logger.LogMessage(pathToTool + commandLineCommands);
			Logger.LogMessage(separator);
			var retval = base.ExecuteTool(pathToTool, responseFileCommands, commandLineCommands);
			Logger.LogMessage(separator);

			return retval;
		}

		protected abstract string NuGetVerb { get; }

		protected abstract void LogArguments(LogArgumentHandler logger);
	}
}