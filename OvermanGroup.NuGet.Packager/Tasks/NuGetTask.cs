using System;
using System.IO;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;

namespace OvermanGroup.NuGet.Packager.Tasks
{
	public abstract class NuGetTask : ToolTask
	{
		[Required]
		public virtual string SolutionDir { get; set; }

		public virtual string NuGetExePath { get; set; }

		protected override string ToolName
		{
			get { return Constants.NuGetFileName; }
		}

		protected override string GenerateFullPathToTool()
		{
			var nuGetExePath = NuGetExePath;
			if (!String.IsNullOrEmpty(nuGetExePath) && File.Exists(nuGetExePath))
			{
				Log.LogMessage(MessageImportance.Low, "Using NuGet.exe from '{0}'.", nuGetExePath);
				return nuGetExePath;
			}

			var resolver = new NuGetExeResolver(Log, SolutionDir);
			nuGetExePath = resolver.GetNuGetExePath();

			if (!String.IsNullOrEmpty(nuGetExePath))
				NuGetExePath = nuGetExePath;

			return nuGetExePath ?? Constants.NuGetFileName;
		}

	}
}