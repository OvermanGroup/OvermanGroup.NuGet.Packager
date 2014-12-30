using System;
using System.IO;
using System.Reflection;
using Microsoft.Build.Framework;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OvermanGroup.NuGet.Packager.Tasks;

namespace OvermanGroup.NuGet.Packager.Test
{
	[TestClass]
	public class PublishNuGetPackageTests : TestHelper
	{
		public virtual ITaskItem CreatePackageHelper()
		{
			var name = Assembly.GetExecutingAssembly().GetName().Name;
			var input = Path.Combine(SolutionDir, name, name + ".csproj");
			var outputDirectory = PrepareOutputDirectory();

			var version = String.Format(
				"{0}.{1}.{2}-test",
				mRandom.Next(1024),
				mRandom.Next(1024),
				mRandom.Next(1024));

			var task = new CreateNuGetPackage
			{
				BuildEngine = BuildEngine,
				SolutionDir = SolutionDir,
				InputFile = input,
				Verbosity = "quiet",
				OutputDirectory = outputDirectory,
				Version = version,
			};

			var package = VerifyOutput(task, false, version);
			return package;
		}

		[TestMethod]
		public void PushToFolder()
		{
			var package = CreatePackageHelper();
			var source = Path.Combine(TestContext.DeploymentDirectory, Guid.NewGuid().ToString("N"));

			var task = new PublishNuGetPackage
			{
				BuildEngine = BuildEngine,
				SolutionDir = SolutionDir,
				PackagePath = package,
				Source = source,
				Verbosity = "detailed",
			};

			var success = task.Execute();
			Assert.IsTrue(success, "Checking task return value");
			Assert.AreEqual(0, task.ExitCode, "Checking task ErrorCode");

			var name = Path.GetFileName(package.ItemSpec) ?? package.ItemSpec;
			var path = Path.Combine(source, name);
			var exits = File.Exists(path);
			Assert.IsTrue(exits, "Checking if the package was published");
		}

	}
}