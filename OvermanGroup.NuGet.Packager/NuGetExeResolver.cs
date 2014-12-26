using System;
using System.IO;
using System.Linq;
using System.Text;
using CuttingEdge.Conditions;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;
using NuGet.Packaging;

namespace OvermanGroup.NuGet.Packager
{
	public class NuGetExeResolver
	{
		public virtual TaskLoggingHelper Logger { get; private set; }

		public virtual string SolutionDir { get; private set; }

		public NuGetExeResolver(TaskLoggingHelper logger, string solutionDir)
		{
			Condition.Requires(logger, "loggeer").IsNotNull();
			Condition.Requires(solutionDir, "solutionDir").IsNotNullOrEmpty();

			Logger = logger;
			SolutionDir = solutionDir;
		}

		public virtual string GetNuGetExePath()
		{
			Logger.LogMessage(MessageImportance.Low, "Attempting to locate NuGet.exe with solution directory '{0}'.", SolutionDir);

			string nuGetExePath;

			if (TryFromSystem(out nuGetExePath))
			{
				Logger.LogMessage(MessageImportance.Low, "Found NuGet.exe from system at location '{0}'.", nuGetExePath);
				return nuGetExePath;
			}

			if (TryFromPackages(out nuGetExePath))
			{
				Logger.LogMessage(MessageImportance.Low, "Found NuGet.exe from packages at location '{0}'.", nuGetExePath);
				return nuGetExePath;
			}

			Logger.LogWarning("Unable to find NuGet.exe from any location.");
			return null;
		}

		public virtual bool TryFromSystem(out string nuGetExePath)
		{
			IntPtr notused;
			var buffer = new StringBuilder(Constants.MaxPath);
			var cch = NativeMethods.SearchPath(null, Constants.NuGetFileName, null, buffer.Capacity, buffer, out notused);
			if (cch > 0)
			{
				nuGetExePath = buffer.ToString();
				return true;
			}
			nuGetExePath = null;
			return false;
		}

		public virtual bool TryFromPackages(out string nuGetExePath)
		{
			var pathToPackagesConfig = Path.Combine(SolutionDir, Constants.PackagesFileName);
			if (!File.Exists(pathToPackagesConfig))
			{
				nuGetExePath = null;
				return false;
			}

			var stream = File.OpenRead(pathToPackagesConfig);
			var reader = new PackagesConfigReader(stream, false);

			nuGetExePath = reader.GetPackages()
				.Where(_ => _.PackageIdentity.Id == Constants.NuGetPackageName)
				.OrderByDescending(_ => _.PackageIdentity.Version)
				.Select(FormatNuGetExePath)
				.Where(File.Exists)
				.FirstOrDefault();

			return String.IsNullOrEmpty(nuGetExePath);
		}

		public virtual string FormatNuGetExePath(PackageReference packageReference)
		{
			var packageIdentity = packageReference.PackageIdentity;
			var path = Path.Combine(SolutionDir, "packages",
				String.Format(packageIdentity.Id, packageIdentity.Version, "{0}.{1}"),
				"tools", Constants.NuGetFileName);
			return path;
		}

	}
}