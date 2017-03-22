using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace SqlMigrator
{
	public class ScriptBuilder
	{
		private readonly IDatabase _database;
		private readonly TextWriter _log;

		public ScriptBuilder(IDatabase database, TextWriter log)
		{
			_database = database;
			_log = log;
		}

		private Migration GetCreateMigrationsTableIfNotExists()
		{
			var ret = new StringBuilder();
			if(!_database.MigrationsTableExists())
			{
				_log.WriteLine("Adding migrations table creation to script");
				ret.AppendLine("-- Migrations table creation")
					.Append(_database.BuildCreateScript())
					.AppendLine(_database.GetStatementDelimiter());
			}
			return new Migration(-1,ret.ToString(),ret.ToString());
		}

		public IEnumerable<Migration> EnumerateUp(IEnumerable<Migration> migrations, int count)
		{
            List<Migration> l = new List<Migration>(migrations);
            // add the "fake" initial migration that creates the migrations table
            l.Add(GetCreateMigrationsTableIfNotExists());
            // add passed in migrations
            l.AddRange(migrations.OrderBy(m => m.Id).Take(count));
            return l;
        }

        public string BuildUp(IEnumerable<Migration> migrations, int count)
        {
            var migs = EnumerateUp(migrations, count);
            return Build(migs, Direction.Up);
        }



        public IEnumerable<Migration> EnumerateDown(IEnumerable<Migration> migrations, int count)
        {
            List <Migration> l = new List<Migration>(migrations);
            // add the "fake" initial migration that creates the migrations table
            l.Add(GetCreateMigrationsTableIfNotExists());
            // add passed in migrations
            l.AddRange(migrations.OrderByDescending(m => m.Id).Take(count));
            return l;
        }

        public string BuildDown(IEnumerable<Migration> migrations, int count)
        {
            var migs = EnumerateDown(migrations, count);
            return Build(migs, Direction.Down);
        }

        public string GetSqlScript(Migration migration, Direction upDown)
        {
            StringBuilder sb = new StringBuilder(); 
            return sb.AppendFormat("-- Migration {0}", migration)
                    .AppendLine()
                    .AppendLine((upDown==Direction.Up)? migration.Up : migration.Down)
                    .Append((upDown == Direction.Up) ? _database.BuildInsertScript(migration) : _database.BuildDeleteScript(migration))
                    .AppendLine(_database.GetStatementDelimiter())
                    .ToString();
        }


        public string Build(IEnumerable<Migration> migrations, Direction upDown)
        {
            var sb = new StringBuilder();
            foreach (Migration migration in migrations)
            {
                sb.Append(GetSqlScript(migration,upDown));
            }
            return sb.ToString();
        }
    }
}