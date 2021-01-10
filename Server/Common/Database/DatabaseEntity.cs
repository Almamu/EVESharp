namespace Common.Database
{
    public abstract class DatabaseEntity
    {
        /// <summary>
        /// Indicates if the object was modified or not since the last read/write from the database
        /// </summary>
        public bool Dirty { get; set; }
        
        /// <summary>
        /// Indicates if the object is new in the database or not
        /// </summary>
        public bool New { get; set; }

        /// <summary>
        /// Writes the object changes to the database
        /// </summary>
        protected abstract void SaveToDB();
   
        /// <summary>
        /// Checks if the object has been modified and writes it's changes to the database
        /// </summary>
        public virtual void Persist()
        {
            if(this.Dirty || this.New)
                this.SaveToDB();

            this.Dirty = false;
            this.New = false;
        }

        public virtual void Dispose()
        {
            
        }
    }
}