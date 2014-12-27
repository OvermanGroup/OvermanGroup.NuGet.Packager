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

	public class CommandLineBuilderWrapper : CommandLineBuilder
	{
		public bool IsQuotingRequiredWrapper(string parameter)
		{
			return base.IsQuotingRequired(parameter);
		}
	}

	public static class CommandLineBuilderHelper
	{
		private static readonly CommandLineBuilderWrapper mSingleton = new CommandLineBuilderWrapper();

		public static bool IsQuotingRequired(string parameter)
		{
			return mSingleton.IsQuotingRequiredWrapper(parameter);
		}

		public static string FormatQuotedArgument(string name, string value)
		{
			var requiresQuotes = mSingleton.IsQuotingRequiredWrapper(value);
			var quotes = requiresQuotes ? "\"" : String.Empty;
			return String.Format("{0}={2}{1}{2}", name, value, quotes);
		}
	}
}