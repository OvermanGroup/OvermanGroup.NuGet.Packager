using System;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;

namespace OvermanGroup.NuGet.Packager.Tasks
{
	public class ResolveNuGetExePath : Task
	{
		[Required]
		public virtual string SolutionDir { get; set; }

		[Output]
		public virtual ITaskItem NuGetExePath { get; set; }

		public override bool Execute()
		{
			var logger = new Logger(BuildEngine, MessageImportance.High);
			var resolver = new NuGetExeResolver(logger, SolutionDir);
			var path = resolver.GetNuGetExePath();
			if (String.IsNullOrEmpty(path)) return false;

			NuGetExePath = new TaskItem(path);
			return true;
		}

	}
}