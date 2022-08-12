using System;

namespace EVESharp.Common.Configuration.Attributes;

public abstract class TransformConfigValue : ConfigValue
{
    public Func<string, object> Transform { get; }

    protected TransformConfigValue (string name, bool optional, Func<string, object> transform) : base (name, optional)
    {
        this.Transform = transform;
    }
}