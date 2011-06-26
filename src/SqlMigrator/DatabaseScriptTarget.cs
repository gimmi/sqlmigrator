using System.Data;

namespace SqlMigrator
{
	public class DatabaseScriptTarget : IScriptTarget
	{
		private readonly IDbConnection _conn;

		public DatabaseScriptTarget(IDbConnection conn)
		{
			_conn = conn;
		}

		public void Execute(string script)
		{
			_conn.Open();
			try
			{
				IDbTransaction tran = _conn.BeginTransaction();
				try
				{
					IDbCommand cmd = _conn.CreateCommand();
					cmd.CommandText = script;
					cmd.ExecuteNonQuery();
					tran.Commit();
				}
				catch
				{
					tran.Rollback();
					throw;
				}
			}
			finally
			{
				_conn.Close();
			}
		}
	}
}