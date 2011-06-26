namespace SqlMigrator
{
	public interface IScriptTarget
	{
		void Execute(string script);
	}
}