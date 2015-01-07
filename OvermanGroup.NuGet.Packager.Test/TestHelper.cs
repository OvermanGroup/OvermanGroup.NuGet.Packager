﻿using System;
using System.IO;
using System.Linq;
using System.Reflection;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using NuGet.Packaging;
using NuGet.PackagingCore;
using OvermanGroup.NuGet.Packager.Tasks;

namespace OvermanGroup.NuGet.Packager.Test
{
	[TestClass]
	public abstract class TestHelper
	{
		protected const string ExpectedVersion = "1.1.0-test";

		protected readonly Random mRandom = new Random();
		protected Mock<IBuildEngine> mBuildEngineMock;
		protected TaskLoggingHelper mLogger;
		protected string mSolutionDir;
		protected string mBasePath;

		public virtual TestContext TestContext { get; set; }

		public virtual IBuildEngine BuildEngine
		{
			get { return mBuildEngineMock.Object; }
		}

		public virtual TaskLoggingHelper Logger
		{
			get { return mLogger ?? (mLogger = new TaskLoggingHelper(BuildEngine, "TestHelper")); }
		}

		public virtual string SolutionDir
		{
			get { return mSolutionDir ?? (mSolutionDir = GetSolutionDir()); }
		}

		public virtual string BasePath
		{
			get { return mBasePath ?? (mBasePath = GetBasePath()); }
		}

		[TestInitialize]
		public virtual void HelperInitialize()
		{
			mBuildEngineMock = new Mock<IBuildEngine>(MockBehavior.Loose);
			mBuildEngineMock
				.Setup(_ => _.LogMessageEvent(It.IsAny<BuildMessageEventArgs>()))
				.Callback((BuildMessageEventArgs msg) => OnLogMessageEvent(msg));
		}

		protected virtual void OnLogMessageEvent(BuildMessageEventArgs msg)
		{
			Console.WriteLine(msg.Message);
		}

		private static string GetSolutionDir()
		{
			var dir = Environment.CurrentDirectory;
			var name = typeof(NuGetExeResolver).Assembly.GetName().Name + ".sln";
			var path = Path.Combine(dir, name);
			var exists = File.Exists(path);
			while (!exists && !String.IsNullOrEmpty(dir))
			{
				dir = Path.GetDirectoryName(dir);
				path = Path.Combine(dir ?? String.Empty, name);
				exists = File.Exists(path);
			}
			Assert.IsTrue(exists, "Checking if the solution directory was found ");
			return dir;
		}

		public virtual string GetBasePath()
		{
			var assembly = Assembly.GetExecutingAssembly();
			var assemblyConfigurationAttribute = assembly.GetCustomAttribute<AssemblyConfigurationAttribute>();
			Assert.IsNotNull(assemblyConfigurationAttribute, "Checking AssemblyConfigurationAttribute");
			var configurationName = assemblyConfigurationAttribute.Configuration;
			Assert.IsFalse(String.IsNullOrEmpty(configurationName), "Checking ConfigurationName");

			var solutionDir = SolutionDir;
			var assemblyName = assembly.GetName();
			var projectDir = Path.Combine(solutionDir, assemblyName.Name);
			var baseDir = Path.Combine(projectDir, "bin", configurationName);
			Assert.IsTrue(Directory.Exists(baseDir), "Checking if the base directory exists");

			return baseDir;
		}

		public virtual string PrepareOutputDirectory()
		{
			// so that the test cases don't collide with each other, place the output packages in separate folders
			var baseDir = TestContext.DeploymentDirectory;
			var dir = Path.Combine(baseDir, TestContext.TestName);
			Directory.CreateDirectory(dir);
			return dir;
		}

		public virtual ITaskItem VerifyOutput(CreateNuGetPackage task, bool symbols, string expectedVersion = ExpectedVersion)
		{
			var success = task.Execute();
			Assert.IsTrue(success, "Checking task return value");
			Assert.AreEqual(0, task.ExitCode, "Checking task ErrorCode");

			var package = symbols ? task.PackageSymbols : task.PackageOutput;
			Assert.IsNotNull(package);
			Assert.IsNotNull(task.FilesWritten);

			var isWritten = task.FilesWritten.Any(_ => _.ItemSpec == package.ItemSpec);
			Assert.IsTrue(isWritten, "Checking if package exists in FileWritten array");

			var packagePath = VerifyPackageOutput(package);
			if (!String.IsNullOrEmpty(expectedVersion))
				VerifyPackageVersion(packagePath, expectedVersion);

			return package;
		}

		public virtual string VerifyPackageOutput(ITaskItem item)
		{
			Assert.IsNotNull(item, "Checking if item is null");

			var path = item.ItemSpec;
			Assert.IsFalse(String.IsNullOrEmpty(path), "Checking if the path is null or empty");
			Assert.IsTrue(File.Exists(path), "Checking if the path exists");

			TestContext.AddResultFile(path);

			return path;
		}

		public virtual void VerifyPackageVersion(string packagePath, string expectedVersion)
		{
			using (var stream = File.OpenRead(packagePath))
			using (var fileSystem = new ZipFileSystem(stream))
			using (var reader = new PackageReader(fileSystem))
			{
				var identity = reader.GetIdentity();
				var actualVersion = identity.Version.ToNormalizedString();
				Assert.AreEqual(expectedVersion, actualVersion, "Checking package version");
			}
		}

	}
}