using System;
using CuttingEdge.Conditions;
using Microsoft.Build.Utilities;

namespace OvermanGroup.NuGet.Packager
{
	public static class CommandLineBuilderExtensions
	{
		public static void AppendSwitchIfNotNullOrEmpty(this CommandLineBuilder builder, string switchName)
		{
			Condition.Requires(builder, "builder").IsNotNull();

			if (!String.IsNullOrEmpty(switchName))
				builder.AppendSwitch(switchName);
		}

		public static void AppendSwitchIfNotNullOrEmpty(this CommandLineBuilder builder, string switchName, string parameter)
		{
			Condition.Requires(builder, "builder").IsNotNull();
			Condition.Requires(switchName, "switchName").IsNotNullOrEmpty();

			if (!String.IsNullOrEmpty(parameter))
				builder.AppendSwitchIfNotNull(switchName, parameter);
		}

		public static void AppendSwitchIfTrue(this CommandLineBuilder builder, string switchName, bool value)
		{
			Condition.Requires(builder, "builder").IsNotNull();
			Condition.Requires(switchName, "switchName").IsNotNullOrEmpty();

			if (value)
				builder.AppendSwitch(switchName);
		}

	}
}