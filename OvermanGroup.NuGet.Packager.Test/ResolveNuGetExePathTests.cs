using System;
using NUnit.Framework;
using OvermanGroup.NuGet.Packager.Tasks;

namespace OvermanGroup.NuGet.Packager.Test
{
	[TestFixture]
	public class ResolveNuGetExePathTests : TestHelper
	{
		[Test]
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