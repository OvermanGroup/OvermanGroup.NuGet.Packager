using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace OvermanGroup.NuGet.Packager.Test
{
	[TestClass]
	public class NuGetExeResolverTests : TestHelper
	{
		private Mock<NuGetExeResolver> mResolverMock;

		public virtual NuGetExeResolver Resolver
		{
			get { return mResolverMock.Object; }
		}

		[TestInitialize]
		public virtual void TestInitialize()
		{
			mResolverMock = new Mock<NuGetExeResolver>(MockBehavior.Loose, Logger, SolutionDir) { CallBase = true };
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