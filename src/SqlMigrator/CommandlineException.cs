using System;

namespace SqlMigrator
{
	public class CommandlineException : Exception
	{
		public CommandlineException(String format, params Object[] args) : base(string.Format(format, args)) {}
	}
}