namespace Common.Database
{
    public class DatabaseAccessor
    {
        private DatabaseConnection mDatabase = null;

        protected DatabaseConnection Database
        {
            get => this.mDatabase;
            private set => this.mDatabase = value;
        }

        public DatabaseAccessor(DatabaseConnection db)
        {
            this.Database = db;
        }
    }
}