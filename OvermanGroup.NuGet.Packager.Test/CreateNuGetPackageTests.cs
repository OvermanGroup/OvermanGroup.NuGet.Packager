using System;
using System.IO;
using System.Reflection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OvermanGroup.NuGet.Packager.Tasks;

namespace OvermanGroup.NuGet.Packager.Test
{
	[TestClass]
	public class CreateNuGetPackageTests : TestHelper
	{
		private const string ExpectedVersion = "1.0.0-test";

		[TestMethod]
		public void FromProject()
		{
			var name = Assembly.GetExecutingAssembly().GetName().Name;
			var input = Path.Combine(SolutionDir, name, name + ".csproj");
			var outputDirectory = TestContext.DeploymentDirectory;

			var task = new CreateNuGetPackage
			{
				// common
				BuildEngine = BuildEngine,
				SolutionDir = SolutionDir,
				// required
				InputFile = input,
				// optional
				Verbosity = "detailed",
				OutputDirectory = outputDirectory
			};

			var success = task.Execute();
			Assert.IsTrue(success, "Checking task return value");
			Assert.AreEqual(0, task.ExitCode, "Checking task ErrorCode");

			var outputPath = VerifyPackageOutput(task.PackageOutput);
			VerifyPackageVersion(outputPath, ExpectedVersion);
		}

		[TestMethod]
		public void FromProjectVersionOverride()
		{
			var name = Assembly.GetExecutingAssembly().GetName().Name;
			var input = Path.Combine(SolutionDir, name, name + ".csproj");
			var outputDirectory = TestContext.DeploymentDirectory;

			var version = String.Format(
				"{0}.{1}.{2}-test",
				mRandom.Next(1024),
				mRandom.Next(1024),
				mRandom.Next(1024));

			var task = new CreateNuGetPackage
			{
				// common
				BuildEngine = BuildEngine,
				SolutionDir = SolutionDir,
				// required
				InputFile = input,
				// optional
				Verbosity = "detailed",
				OutputDirectory = outputDirectory,
				// test specific
				Version = version,
			};

			var success = task.Execute();
			Assert.IsTrue(success, "Checking task return value");
			Assert.AreEqual(0, task.ExitCode, "Checking task ErrorCode");

			var outputPath = VerifyPackageOutput(task.PackageOutput);
			VerifyPackageVersion(outputPath, version);
		}

		[TestMethod]
		public void FromNuSpecPass()
		{
			var name = Assembly.GetExecutingAssembly().GetName().Name;
			var input = Path.Combine(SolutionDir, name, name + ".nuspec");
			var outputDirectory = TestContext.DeploymentDirectory;

			var task = new CreateNuGetPackage
			{
				// common
				BuildEngine = BuildEngine,
				SolutionDir = SolutionDir,
				// required
				InputFile = input,
				// optional
				Verbosity = "detailed",
				OutputDirectory = outputDirectory
			};

			var success = task.Execute();
			Assert.IsTrue(success, "Checking task return value");
			Assert.AreEqual(0, task.ExitCode, "Checking task ErrorCode");

			var outputPath = VerifyPackageOutput(task.PackageOutput);
			VerifyPackageVersion(outputPath, ExpectedVersion);
		}

		[TestMethod]
		public void FromNuSpecFail()
		{
			var name = Assembly.GetExecutingAssembly().GetName().Name;
			var input = Path.Combine(SolutionDir, name, name + ".cant-find-me.nuspec");
			var outputDirectory = TestContext.DeploymentDirectory;

			var task = new CreateNuGetPackage
			{
				// common
				BuildEngine = BuildEngine,
				SolutionDir = SolutionDir,
				// required
				InputFile = input,
				// optional
				Verbosity = "detailed",
				OutputDirectory = outputDirectory
			};

			var success = task.Execute();
			Assert.IsFalse(success, "Checking task return value");
			Assert.AreNotEqual(0, task.ExitCode, "Checking task ErrorCode");
			Assert.IsNull(task.PackageOutput, "Checking if PackageOutput is null");
		}

	}
}