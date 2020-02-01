namespace Common.Database
{
    public class DatabaseAccessor
    {
        private DatabaseConnection mDatabase = null;

        protected DatabaseConnection Database
        {
            get { return this.mDatabase; }
            private set { this.mDatabase = value; }
        }
        public DatabaseAccessor(DatabaseConnection db)
        {
            this.Database = db;
        }
    }
}