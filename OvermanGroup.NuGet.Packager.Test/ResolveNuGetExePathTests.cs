using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OvermanGroup.NuGet.Packager.Tasks;

namespace OvermanGroup.NuGet.Packager.Test
{
	[TestClass]
	public class ResolveNuGetExePathTests : TestHelper
	{
		[TestMethod]
		public void TestExecuteTask()
		{
			var task = new ResolveNuGetExePath
			{
				BuildEngine = BuildEngine,
				SolutionDir = SolutionDir
			};

			var success = task.Execute();
			Assert.IsTrue(success, "Checking task return value");

			var item = task.NuGetExePath;
			Assert.IsNotNull(item, "Checking if TaskItem is null");

			var path = item.ItemSpec;
			Assert.IsFalse(String.IsNullOrEmpty(path), "Checking if NuGetExePath is null or empty");
		}

	}
}