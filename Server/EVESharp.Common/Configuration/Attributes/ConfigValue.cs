using System;

namespace EVESharp.Common.Configuration.Attributes;

[AttributeUsage(AttributeTargets.Property)]
public class ConfigValue : Attribute
{
    public string Name     { get; }
    public bool   Optional { get; }

    public ConfigValue (string name, bool optional = false)
    {
        this.Name     = name;
        this.Optional = optional;
    }
}