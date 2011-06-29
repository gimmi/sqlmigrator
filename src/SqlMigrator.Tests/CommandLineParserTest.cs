using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using NUnit.Framework;
using SharpTestsEx;

namespace SqlMigrator.Tests
{
	[TestFixture]
	public class CommandLineParserTest
	{
		public enum SwitchOption
		{
			Value1,
			Value2,
			Value3
		}

		public class Options
		{
			[System.ComponentModel.Description("StringOpt description")]
			public string StringOpt = "default value";

			[System.ComponentModel.Description("IntOpt description")]
			public int IntOpt;
			public bool BoolOpt;
			public SwitchOption EnumOpt;
		}

		private CommandLineParser<Options> _target;

		[SetUp]
		public void SetUp()
		{
			_target = new CommandLineParser<Options>();
		}

		[Test]
		public void Should_parse_arguments()
		{
			IDictionary<string, string> args = _target.ParseArgs(new[] { "/p1", "v1", "/p2", "v2" });
			args.Should().Have.SameValuesAs(new[] {
				new KeyValuePair<string, string>("p1", "v1"),
				new KeyValuePair<string, string>("p2", "v2")
			});
		}

		[Test]
		public void Should_fail_when_no_option_name_provided()
		{
			Executing.This(() => _target.ParseArgs(new[] { "/p1", "v1", "v2" })).Should().Throw<CommandlineException>().And.ValueOf.Message.Should().Be.EqualTo("Expected option name, found 'v2'");
		}

		[Test]
		public void Should_fail_when_option_without_value_provided()
		{
			Executing.This(() => _target.ParseArgs(new[] { "/p1", "v1", "/p2" })).Should().Throw<CommandlineException>().And.ValueOf.Message.Should().Be.EqualTo("No value for option 'p2'");
		}

		[Test]
		public void Should_fill_option_object()
		{
			Options instance = _target.Parse(new[] { "/stringopt", "a string", "/IntOpt", "123", "/BOOLOPT", "true" }, new Options());
			instance.StringOpt.Should().Be.EqualTo("a string");
			instance.IntOpt.Should().Be.EqualTo(123);
			instance.BoolOpt.Should().Be.EqualTo(true);
		}

		[Test]
		public void Should_leave_default_value_when_no_option_provided()
		{
			Options instance = _target.Parse(new string[0], new Options());
			instance.StringOpt.Should().Be.EqualTo("default value");
		}

		[Test]
		public void Should_fail_with_unknown_option()
		{
			Executing.This(() => _target.Parse(new[] { "/unknown", "value" }, new Options())).Should().Throw<CommandlineException>().And.ValueOf.Message.Should().Be.EqualTo("Unknown or ambiguous option 'unknown'");
		}

		[Test]
		public void Should_support_enums()
		{
			Options instance = _target.Parse(new[] { "/enumopt", "Value1" }, new Options());
			instance.EnumOpt.Should().Be.EqualTo(SwitchOption.Value1);
		}

		[Test]
		public void Should_fail_when_value_cannot_be_converted_to_target_type()
		{
			Executing.This(() => _target.Parse(new[] { "/intopt", "nan" }, new Options())).Should().Throw<CommandlineException>().And.ValueOf.Message.Should().Be.EqualTo("Cannot convert value 'nan' for option 'intopt' to 'System.Int32'");
		}

		[Test]
		public void Should_build_help()
		{
			var expected = @"
/StringOpt  StringOpt description
/IntOpt     IntOpt description
/BoolOpt    
/EnumOpt    
".TrimStart('\n', '\r');

			var sb = new StringBuilder();
			_target.PrintHelp(new StringWriter(sb));
			var actual = sb.ToString();

			actual.Should().Be.EqualTo(expected);
		}
	}
}