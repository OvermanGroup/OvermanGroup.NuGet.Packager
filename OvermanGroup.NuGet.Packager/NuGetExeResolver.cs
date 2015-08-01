using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using CuttingEdge.Conditions;

namespace OvermanGroup.NuGet.Packager
{
	public class NuGetExeResolver
	{
		public virtual ILogger Logger { get; private set; }

		public virtual string SolutionDir { get; private set; }

		public virtual string ProjectDir { get; private set; }

		public virtual string DownloadDir { get; private set; }

		public NuGetExeResolver(ILogger logger, string solutionDir, string projectDir, string downloadDir)
		{
			Condition.Requires(logger, "logger").IsNotNull();
			Condition.Requires(solutionDir, "solutionDir").IsNotNullOrEmpty();
			Condition.Requires(solutionDir, "projectDir").IsNotNullOrEmpty();
			Condition.Requires(downloadDir, "downloadDir").IsNotNullOrEmpty();

			Logger = logger;
			SolutionDir = solutionDir;
			ProjectDir = projectDir;
			DownloadDir = downloadDir;
		}

		public virtual string GetNuGetExePath()
		{
			Logger.LogMessage("Attempting to locate NuGet.exe for '{0}'.", SolutionDir);

			string nuGetExePath;

			if (TryFromSystem(out nuGetExePath))
			{
				Logger.LogMessage("Found NuGet.exe from system at location '{0}'.", nuGetExePath);
				return nuGetExePath;
			}

			if (TryFromPackages(out nuGetExePath))
			{
				Logger.LogMessage("Found NuGet.exe from packages at location '{0}'.", nuGetExePath);
				return nuGetExePath;
			}

			if (TryFromDownload(out nuGetExePath))
			{
				Logger.LogMessage("Found NuGet.exe from download at location '{0}'.", nuGetExePath);
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
			nuGetExePath = Directory
				// search for the NuGet.CommandLine package
				.EnumerateDirectories(Path.Combine(SolutionDir, "packages"), Constants.NuGetPackageName + ".*", SearchOption.TopDirectoryOnly)
				// get the path to NuGet.exe
				.Select(dir => Path.Combine(dir, "tools", Constants.NuGetFileName))
				// make sure the file exists
				.Where(File.Exists)
				// sort descending by product version
				.OrderByDescending(file => Version.Parse(FileVersionInfo.GetVersionInfo(file).ProductVersion))
				// we only want the first result
				.FirstOrDefault();

			return !String.IsNullOrEmpty(nuGetExePath);
		}

		public virtual bool TryFromDownload(out string nuGetExePath)
		{
			var outputFile = Path.Combine(DownloadDir, "NuGet.exe");
			using (var client = new WebClient())
			{
				try
				{
					client.DownloadFile("https://nuget.org/NuGet.exe", outputFile);
				}
				catch (Exception exception)
				{
					Debug.WriteLine(exception);
					nuGetExePath = null;
					return false;
				}
			}
			nuGetExePath = outputFile;
			return true;
		}

	}
}