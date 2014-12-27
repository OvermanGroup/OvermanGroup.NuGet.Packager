using System;
using System.IO;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace OvermanGroup.NuGet.Packager.Test
{
	[TestClass]
	public class NuGetExeResolverTests
	{
		private Mock<IBuildEngine> mBuildEngineMock;
		private Mock<NuGetExeResolver> mResolverMock;
		private TaskLoggingHelper mLogger;
		private string mSolutionDir;

		public virtual IBuildEngine BuildEngine
		{
			get { return mBuildEngineMock.Object; }
		}

		public virtual TaskLoggingHelper Logger
		{
			get { return mLogger ?? (mLogger = new TaskLoggingHelper(BuildEngine, "NuGetExeResolver")); }
		}

		public virtual NuGetExeResolver Resolver
		{
			get { return mResolverMock.Object; }
		}

		public virtual string SolutionDir
		{
			get { return mSolutionDir ?? (mSolutionDir = GetSolutionDir()); }
		}

		[TestInitialize]
		public virtual void TestInitialize()
		{
			mBuildEngineMock = new Mock<IBuildEngine>(MockBehavior.Loose);
			mResolverMock = new Mock<NuGetExeResolver>(MockBehavior.Loose, Logger, SolutionDir) { CallBase = true };
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

		[TestMethod]
		public void TryFromSystem()
		{
			string path;
			var b = Resolver.TryFromSystem(out path);
			Assert.IsTrue(b, "Checking return value from TryFromSystem");
			Assert.IsNotNull(path, "Checking if NuExePath is null");
			Assert.IsTrue(File.Exists(path), "Checking if NuExePath exists");
		}

		[TestMethod]
		public void TryFromPackages()
		{
			string path;
			var b = Resolver.TryFromPackages(out path);
			Assert.IsTrue(b, "Checking return value from TryFromPackages");
			Assert.IsNotNull(path, "Checking if NuExePath is null");
			Assert.IsTrue(File.Exists(path), "Checking if NuExePath exists");
		}

		[TestMethod]
		public void GetNuGetExePathReturnsNullWhenBothFail()
		{
			string nullValue;
			mResolverMock.Setup(_ => _.TryFromPackages(out nullValue)).Returns(false);
			mResolverMock.Setup(_ => _.TryFromSystem(out nullValue)).Returns(false);

			var path = Resolver.GetNuGetExePath();
			Assert.IsNull(path);
		}

	}
}