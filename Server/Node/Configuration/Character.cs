using IniParser.Model;

namespace Node.Configuration
{
    public class Character
    {
        public double Balance { get; private set; }

        public void Load(KeyDataCollection section)
        {
            if (section.ContainsKey("balance") == false)
            {
                this.Balance = 50000.0;
                return;
            }

            this.Balance = double.Parse(section["balance"]);
        }
    }
}