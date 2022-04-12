using IniParser.Model;

namespace EVESharp.Node.Configuration;

public class Character
{
    public double Balance { get; private set; }

    public void Load (KeyDataCollection section)
    {
        if (section.ContainsKey ("balance") == false)
        {
            Balance = 50000.0;

            return;
        }

        Balance = double.Parse (section ["balance"]);
    }
}