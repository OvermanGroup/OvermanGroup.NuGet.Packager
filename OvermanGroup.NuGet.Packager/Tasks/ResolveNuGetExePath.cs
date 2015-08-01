using System;
using System.IO;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;

namespace OvermanGroup.NuGet.Packager.Tasks
{
	public class ResolveNuGetExePath : Task
	{
		[Required]
		public virtual string SolutionDir { get; set; }

		[Required]
		public virtual string ProjectDir { get; set; }

		[Output]
		public virtual ITaskItem NuGetExePath { get; set; }

		public override bool Execute()
		{
			var logger = new Logger(BuildEngine, MessageImportance.High);
			var downloadDir = Path.GetDirectoryName(BuildEngine.ProjectFileOfTaskNode);
			var resolver = new NuGetExeResolver(logger, SolutionDir, ProjectDir, downloadDir);
			var path = resolver.GetNuGetExePath();
			if (String.IsNullOrEmpty(path)) return false;

			NuGetExePath = new TaskItem(path);
			return true;
		}

	}
}