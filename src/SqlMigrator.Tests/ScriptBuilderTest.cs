using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;

namespace SqlMigrator.Tests
{
	[TestFixture]
	public class ScriptBuilderTest
	{
		private ScriptBuilder _target;
		private IDatabase _database;

		[SetUp]
		public void SetUp()
		{
			_database = MockRepository.GenerateStub<IDatabase>();
			_target = new ScriptBuilder(_database);
		}

		[Test]
		public void Shuld_build_up_script_in_order()
		{
			_database.Stub(x => x.BuildInsertScript(Arg<Migration>.Is.Anything)).Return("InsScript");
			var actual = _target.BuildUp(new[] {
				new Migration(2, "2-up", "2-down"),
				new Migration(3, "3-up", "3-down"),
				new Migration(1, "1-up", "1-down")
			}, 2);
			actual.Should().Be.EqualTo(@"-- Migration #1
1-up
InsScript
-- Migration #2
2-up
InsScript
");
		}

		[Test]
		public void Shuld_build_down_script_in_order()
		{
			_database.Stub(x => x.BuildDeleteScript(Arg<Migration>.Is.Anything)).Return("DelScript");
			var actual = _target.BuildDown(new[] {
				new Migration(2, "2-up", "2-down"),
				new Migration(1, "1-up", "1-down"),
				new Migration(3, "3-up", "3-down")
			}, 2);
			actual.Should().Be.EqualTo(@"-- Migration #3
3-down
DelScript
-- Migration #2
2-down
DelScript
");
		}
	}
}