using System;

namespace EVESharp.Common.Configuration.Attributes;

public class EnumConfigValue : TransformConfigValue
{
    public EnumConfigValue (string name, Type enumType, bool optional = false) : base (name, optional, (value) =>
    {
        // get all the values in the enum
        return Enum.Parse (enumType, value, true);
    })
    {
        
    }
}