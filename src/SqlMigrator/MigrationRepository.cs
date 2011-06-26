using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace SqlMigrator
{
	public class MigrationRepository
	{
		private readonly string _migrationsPath;
		private readonly Encoding _encoding;
		private readonly LogTable _logTable;

		public MigrationRepository(string migrationsPath, Encoding encoding, LogTable logTable)
		{
			_migrationsPath = migrationsPath;
			_encoding = encoding;
			_logTable = logTable;
		}

		private bool IsValidMigration(string file)
		{
			var fileInfo = new FileInfo(file);
			Match match = Regex.Match(fileInfo.Name, @"^(\d+)");
			return fileInfo.Exists
			       && fileInfo.Extension.Equals("sql", StringComparison.InvariantCultureIgnoreCase)
			       && match.Success;
		}

		private Migration Create(string file)
		{
			var fileInfo = new FileInfo(file);
			Match match = Regex.Match(fileInfo.Name, @"^(\d+)");
			string[] upDown = SplitScript(File.ReadAllText(file, _encoding));
			return new Migration(Int64.Parse(match.Groups[0].Value), upDown[0], upDown[1]);
		}

		private string[] SplitScript(string script)
		{
			var match = Regex.Match(script, @"^\s*--\s*@DOWN\s*$", RegexOptions.IgnoreCase);
			if(match.Success)
			{
				return new[] { script.Substring(0, match.Index), script.Substring(match.Index + match.Length) };
			}
			return new[] { script, "" };
		}

		public IEnumerable<Migration> GetPendingMigrations()
		{
			return Directory.GetFiles(_migrationsPath)
				.Where(IsValidMigration)
				.Select(Create)
				.Where(_logTable.IsMigrationPending);
		}
	}
}