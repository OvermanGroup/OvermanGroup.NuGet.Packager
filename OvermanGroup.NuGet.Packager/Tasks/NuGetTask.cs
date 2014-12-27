using System;
using System.IO;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;

namespace OvermanGroup.NuGet.Packager.Tasks
{
	public abstract class NuGetTask : ToolTask
	{
		private string mNuGetExePathSpecified;

		// This field is static because different task instances will need to use
		// the same path to NuGet, so optimize the resolution logic.
		private static string mNuGetExePathResolved;

		[Required]
		public virtual string SolutionDir { get; set; }

		public virtual string NuGetExePath
		{
			get { return mNuGetExePathSpecified; }
			set { mNuGetExePathSpecified = value; }
		}

		protected override string ToolName
		{
			get { return Constants.NuGetFileName; }
		}

		protected override string GenerateFullPathToTool()
		{
			var nuGetExePath = mNuGetExePathSpecified;
			if (!String.IsNullOrEmpty(nuGetExePath) && File.Exists(nuGetExePath))
			{
				Log.LogMessage(Constants.MessageImportance, "Using NuGet.exe from '{0}'.", nuGetExePath);
				mNuGetExePathResolved = nuGetExePath;
				return nuGetExePath;
			}

			nuGetExePath = mNuGetExePathResolved;
			if (!String.IsNullOrEmpty(nuGetExePath) && File.Exists(nuGetExePath))
				return nuGetExePath;

			var resolver = new NuGetExeResolver(Log, SolutionDir);
			nuGetExePath = resolver.GetNuGetExePath();
			mNuGetExePathResolved = nuGetExePath;

			return nuGetExePath ?? Constants.NuGetFileName;
		}

		protected override int ExecuteTool(string pathToTool, string responseFileCommands, string commandLineCommands)
		{
			const MessageImportance importance = Constants.MessageImportance;

			Log.LogMessage(importance, "---- Arguments ----");
			Log.LogMessage(importance, "SolutionDir: {0}", SolutionDir);
			Log.LogMessage(importance, "NuGetExePath (specified): {0}", mNuGetExePathSpecified);
			Log.LogMessage(importance, "NuGetExePath (resolved): {0}", mNuGetExePathResolved);
			LogArguments(importance);
			Log.LogMessage(importance, "Tool Task Path: {0}", pathToTool);
			Log.LogMessage(importance, "Tool Task Arguments: {0}", commandLineCommands);
			Log.LogMessage(importance, "-------------------");

			return base.ExecuteTool(pathToTool, responseFileCommands, commandLineCommands);
		}

		protected abstract void LogArguments(MessageImportance importance);
	}
}