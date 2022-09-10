using System.Reflection;
using EVESharp.Common.Configuration.Attributes;

namespace EVESharp.Orchestrator.Providers;

public class DatabaseConfiguration : Common.Configuration.Database
{
    public DatabaseConfiguration (IConfiguration configuration)
    {
        Load (configuration, this);
    }
    
    private static void Load (IConfiguration configuration, DatabaseConfiguration result)
    {       
        // get section attribute
        ConfigSection section = typeof (DatabaseConfiguration).GetCustomAttribute <ConfigSection> ();
        
        if (section is null)
            return;
        
        IConfigurationSection collection = configuration.GetSection (section.Section);

        PropertyInfo [] values = typeof (DatabaseConfiguration).GetProperties ();

        foreach (PropertyInfo value in values)
        {
            Type valueType = value.PropertyType;
            
            // get the ConfigValue attribute
            ConfigValue configValue = value.GetCustomAttribute <ConfigValue> ();

            if (configValue is null)
                continue;
            
            string iniValue = collection [configValue.Name];
            
            // got the value name, set based on type
            // first check if the configValue is a transform
            if (configValue is TransformConfigValue transform)
            {
                value.SetValue (result, transform.Transform (iniValue));
            }
            else if (typeof (bool) == valueType)
            {
                iniValue = iniValue.ToUpper ();
                // booleans can be casted based on different values, just in case
                value.SetValue (result, iniValue == "YES" || iniValue == "1" || iniValue == "TRUE");
            }
            else if (typeof (List <string>) == valueType)
            {
                // comma-separated list
                value.SetValue (
                    result, 
                    iniValue
                        .Split (",")
                        .Select (x => x.Trim())
                        .ToList ()
                );
            }
            else if (typeof (uint) == valueType)
                value.SetValue (result, uint.Parse (iniValue));
            else if (typeof (int) == valueType)
                value.SetValue (result, int.Parse (iniValue));
            else if (typeof (ulong) == valueType)
                value.SetValue (result, ulong.Parse (iniValue));
            else if (typeof (long) == valueType)
                value.SetValue (result, long.Parse (iniValue));
            else if (typeof (ushort) == valueType)
                value.SetValue (result, ushort.Parse (iniValue));
            else if (typeof (short) == valueType)
                value.SetValue (result, short.Parse (iniValue));
            else if (typeof (byte) == valueType)
                value.SetValue (result, byte.Parse (iniValue));
            else if (typeof (sbyte) == valueType)
                value.SetValue (result, sbyte.Parse (iniValue));
            else if (typeof (float) == valueType)
                value.SetValue (result, float.Parse (iniValue));
            else if (typeof (double) == valueType)
                value.SetValue (result, double.Parse (iniValue));
            else if (typeof (string) == valueType)
            {
                // try to set it directly
                value.SetValue (result, iniValue);
            }
            else
            {
                throw new Exception ($"Cannot convert ini value {section.Section}.{configValue.Name} to type");
            }
        }
    }
}