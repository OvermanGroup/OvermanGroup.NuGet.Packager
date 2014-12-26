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
		public virtual string NuGetExePath { get; set; }

		public override bool Execute()
		{
			var resolver = new NuGetExeResolver(Log, SolutionDir);
			NuGetExePath = resolver.GetNuGetExePath();
			return !String.IsNullOrEmpty(NuGetExePath);
		}

	}
}