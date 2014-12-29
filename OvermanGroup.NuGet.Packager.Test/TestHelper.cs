using System;
using System.IO;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using NuGet.Packaging;
using NuGet.PackagingCore;

namespace OvermanGroup.NuGet.Packager.Test
{
	[TestClass]
	public abstract class TestHelper
	{
		protected readonly Random mRandom = new Random();
		protected Mock<IBuildEngine> mBuildEngineMock;
		protected TaskLoggingHelper mLogger;
		protected string mSolutionDir;

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

		public virtual string VerifyPackageOutput(ITaskItem outputItem)
		{
			Assert.IsNotNull(outputItem, "Checking if PackageOutput is null");

			var outputPath = outputItem.ItemSpec;
			Assert.IsFalse(String.IsNullOrEmpty(outputPath), "Checking if the output path is null or empty");
			Assert.IsTrue(File.Exists(outputPath), "Checking if the output path exists");

			TestContext.AddResultFile(outputPath);

			return outputPath;
		}

		public virtual void VerifyPackageVersion(string outputPath, string expectedVersion)
		{
			using (var stream = File.OpenRead(outputPath))
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