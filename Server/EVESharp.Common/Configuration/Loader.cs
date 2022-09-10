using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using EVESharp.Common.Configuration.Attributes;
using IniParser;
using IniParser.Model;

namespace EVESharp.Common.Configuration;

public static class Loader
{
    public static T Load <T> (string filename) where T : class
    {
        // parse the ini file first
        FileIniDataParser parser = new FileIniDataParser ();
        IniData           data   = parser.ReadFile (filename);
        
        // create an instance of the result type
        T result = Activator.CreateInstance <T> ();
        
        // get members inside
        PropertyInfo[] properties = typeof (T).GetProperties ();
        
        // parse each member
        foreach (PropertyInfo property in properties)
        {
            Type propertyType = property.PropertyType;
            
            // create a new instance of the properties' type
            object newObject = Activator.CreateInstance (propertyType);

            // store the new property back
            property.SetValue (result, newObject);
            
            // get section attribute
            ConfigSection section = propertyType.GetCustomAttribute <ConfigSection> ();

            // cannot load members that do not have a section name
            if (section is null)
                continue;
            
            // ensure the section exists if required, otherwise leave the defaults
            if (data.Sections.ContainsSection (section.Section) == false)
            {
                if (section.Optional == false)
                    throw new KeyNotFoundException ($"Cannot find ini section {section.Section}");

                continue;
            }
            
            KeyDataCollection collection = data [section.Section];
            
            // get all the members and check their ConfigValue attribute
            PropertyInfo [] values = propertyType.GetProperties ();

            foreach (PropertyInfo value in values)
            {
                Type valueType = value.PropertyType;
                
                // get the ConfigValue attribute
                ConfigValue configValue = value.GetCustomAttribute <ConfigValue> ();

                if (configValue is null)
                    continue;
                
                // check if the value exists in the file, otherwise go with the default
                if (collection.ContainsKey (configValue.Name) == false)
                {
                    if (configValue.Optional == false)
                        throw new Exception ($"Expected {configValue.Name} inside {section.Section}");

                    continue;
                }

                string iniValue = collection [configValue.Name];
                
                // got the value name, set based on type
                // first check if the configValue is a transform
                if (configValue is TransformConfigValue transform)
                {
                    value.SetValue (newObject, transform.Transform (iniValue));
                }
                else if (typeof (bool) == valueType)
                {
                    iniValue = iniValue.ToUpper ();
                    // booleans can be casted based on different values, just in case
                    value.SetValue (newObject, iniValue == "YES" || iniValue == "1" || iniValue == "TRUE");
                }
                else if (typeof (List <string>) == valueType)
                {
                    // comma-separated list
                    value.SetValue (
                        newObject, 
                        iniValue
                            .Split (",")
                            .Select (x => x.Trim())
                            .ToList ()
                    );
                }
                else if (typeof (uint) == valueType)
                    value.SetValue (newObject, uint.Parse (iniValue));
                else if (typeof (int) == valueType)
                    value.SetValue (newObject, int.Parse (iniValue));
                else if (typeof (ulong) == valueType)
                    value.SetValue (newObject, ulong.Parse (iniValue));
                else if (typeof (long) == valueType)
                    value.SetValue (newObject, long.Parse (iniValue));
                else if (typeof (ushort) == valueType)
                    value.SetValue (newObject, ushort.Parse (iniValue));
                else if (typeof (short) == valueType)
                    value.SetValue (newObject, short.Parse (iniValue));
                else if (typeof (byte) == valueType)
                    value.SetValue (newObject, byte.Parse (iniValue));
                else if (typeof (sbyte) == valueType)
                    value.SetValue (newObject, sbyte.Parse (iniValue));
                else if (typeof (float) == valueType)
                    value.SetValue (newObject, float.Parse (iniValue));
                else if (typeof (double) == valueType)
                    value.SetValue (newObject, double.Parse (iniValue));
                else if (typeof (string) == valueType)
                {
                    // try to set it directly
                    value.SetValue (newObject, iniValue);
                }
                else
                {
                    throw new Exception ($"Cannot convert ini value {section.Section}.{configValue.Name} to type");
                }
            }
        }
        
        return result;
    }
}