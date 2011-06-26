using System.IO;
using System.Text;

namespace SqlMigrator
{
	public class FileScriptTarget : IScriptTarget
	{
		private readonly string _fileName;
		private readonly Encoding _encoding;

		public FileScriptTarget(string fileName, Encoding encoding)
		{
			_fileName = fileName;
			_encoding = encoding;
		}

		public void Execute(string script)
		{
			File.WriteAllText(_fileName, script, _encoding);
		}
	}
}