namespace SqlMigrator
{
    public enum Direction
    {
        Up,
        Down
    }

	public class Migration
	{
		private readonly long _id;
		private readonly string _up;
		private readonly string _down;

		public Migration(long id, string up, string down)
		{
			_id = id;
			_up = up;
			_down = down;
		}

		public long Id
		{
			get { return _id; }
		}

		public string Up
		{
			get { return _up; }
		}

		public string Down
		{
			get { return _down; }
		}

		public override string ToString()
		{
			return string.Format("#{0}", Id);
		}
	}
}