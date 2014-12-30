﻿using System;
using System.IO;
using System.Reflection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OvermanGroup.NuGet.Packager.Tasks;

namespace OvermanGroup.NuGet.Packager.Test
{
	[TestClass]
	public class CreateNuGetPackageTests : TestHelper
	{
		[TestMethod]
		public void FromProjectNoSymbols()
		{
			var name = Assembly.GetExecutingAssembly().GetName().Name;
			var input = Path.Combine(SolutionDir, name, name + ".csproj");
			var outputDirectory = PrepareOutputDirectory();

			var task = new CreateNuGetPackage
			{
				BuildEngine = BuildEngine,
				SolutionDir = SolutionDir,
				InputFile = input,
				Verbosity = "detailed",
				OutputDirectory = outputDirectory,
				Symbols = false,
			};

			VerifyOutput(task, false);
		}

		[TestMethod]
		public void FromProjectWithSymbols()
		{
			var name = Assembly.GetExecutingAssembly().GetName().Name;
			var input = Path.Combine(SolutionDir, name, name + ".csproj");
			var outputDirectory = PrepareOutputDirectory();

			var task = new CreateNuGetPackage
			{
				BuildEngine = BuildEngine,
				SolutionDir = SolutionDir,
				InputFile = input,
				Verbosity = "detailed",
				OutputDirectory = outputDirectory,
				Symbols = true,
			};

			VerifyOutput(task, false);
			VerifyOutput(task, true);
		}

		[TestMethod]
		public void FromProjectOverrideVersionNoSymbols()
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
				Verbosity = "detailed",
				OutputDirectory = outputDirectory,
				Version = version,
				Symbols = false,
			};

			VerifyOutput(task, false, version);
		}

		[TestMethod]
		public void FromProjectOverrideVersionWithSymbols()
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
				Verbosity = "detailed",
				OutputDirectory = outputDirectory,
				Version = version,
				Symbols = true,
			};

			VerifyOutput(task, false, version);
			VerifyOutput(task, true, version);
		}

		[TestMethod]
		public void FromNuSpecNoSymbols()
		{
			var name = Assembly.GetExecutingAssembly().GetName().Name;
			var input = Path.Combine(SolutionDir, name, name + ".nuspec");
			var outputDirectory = PrepareOutputDirectory();

			var task = new CreateNuGetPackage
			{
				BuildEngine = BuildEngine,
				SolutionDir = SolutionDir,
				InputFile = input,
				Verbosity = "detailed",
				OutputDirectory = outputDirectory,
				Symbols = false,
			};

			VerifyOutput(task, false);
		}

		[TestMethod]
		public void FromNuSpecWithSymbols()
		{
			var name = Assembly.GetExecutingAssembly().GetName().Name;
			var input = Path.Combine(SolutionDir, name, name + ".nuspec");
			var outputDirectory = PrepareOutputDirectory();

			var task = new CreateNuGetPackage
			{
				BuildEngine = BuildEngine,
				SolutionDir = SolutionDir,
				Verbosity = "detailed",
				InputFile = input,
				OutputDirectory = outputDirectory,
				Symbols = true,
			};

			VerifyOutput(task, false);
			VerifyOutput(task, true);
		}

		[TestMethod]
		public void FromNuSpecMissing()
		{
			var name = Assembly.GetExecutingAssembly().GetName().Name;
			var input = Path.Combine(SolutionDir, name, name + ".cant-find-me.nuspec");
			var outputDirectory = PrepareOutputDirectory();

			var task = new CreateNuGetPackage
			{
				BuildEngine = BuildEngine,
				SolutionDir = SolutionDir,
				InputFile = input,
				Verbosity = "detailed",
				OutputDirectory = outputDirectory
			};

			var success = task.Execute();
			Assert.IsFalse(success, "Checking task return value");
			Assert.AreNotEqual(0, task.ExitCode, "Checking task ErrorCode");
			Assert.IsNull(task.PackageOutput, "Checking if PackageOutput is null");
			Assert.IsNull(task.PackageSymbols, "Checking if PackageSymbols is null");
		}

	}
}