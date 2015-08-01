using System;
using System.IO;
using Moq;
using NUnit.Framework;

namespace OvermanGroup.NuGet.Packager.Test
{
	[TestFixture]
	public class NuGetExeResolverTests : TestHelper
	{
		private Mock<NuGetExeResolver> mResolverMock;

		public virtual NuGetExeResolver Resolver
		{
			get { return mResolverMock.Object; }
		}

		[SetUp]
		public virtual void TestInitialize()
		{
			mResolverMock = new Mock<NuGetExeResolver>(MockBehavior.Loose, Logger, SolutionDir, ProjectDir, BasePath) { CallBase = true };
		}

		[Test]
		public void TryFromSystem()
		{
			string path;
			var b = Resolver.TryFromSystem(out path);
			Console.WriteLine("Path: {0}", path);
			Assert.IsTrue(b, "Checking return value from TryFromSystem");
			Assert.IsNotNull(path, "Checking if NuExePath is null");
			Assert.IsTrue(File.Exists(path), "Checking if NuExePath exists");
		}

		[Test]
		public void TryFromPackages()
		{
			string path;
			var b = Resolver.TryFromPackages(out path);
			Console.WriteLine("Path: {0}", path);
			Assert.IsTrue(b, "Checking return value from TryFromPackages");
			Assert.IsNotNull(path, "Checking if NuExePath is null");
			Assert.IsTrue(File.Exists(path), "Checking if NuExePath exists");
		}

		[Test]
		public void TryFromDownload()
		{
			File.Delete(Path.Combine(BasePath, "NuGet.exe"));

			string path;
			var b = Resolver.TryFromDownload(out path);
			Console.WriteLine("Path: {0}", path);
			Assert.IsTrue(b, "Checking return value from TryFromDownload");
			Assert.IsNotNull(path, "Checking if NuExePath is null");
			Assert.IsTrue(File.Exists(path), "Checking if NuExePath exists");
		}

		[Test]
		public void GetNuGetExePathPassWhenBothFail()
		{
			string nullValue;
			mResolverMock.Setup(_ => _.TryFromPackages(out nullValue)).Returns(false);
			mResolverMock.Setup(_ => _.TryFromSystem(out nullValue)).Returns(false);

			var path = Resolver.GetNuGetExePath();
			Console.WriteLine("Path: {0}", path);
			Assert.IsNotNull(path, "Checking if NuExePath is null");
			Assert.IsTrue(File.Exists(path), "Checking if NuExePath exists");
		}

	}
}