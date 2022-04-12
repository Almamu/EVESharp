using System;
using System.Runtime.InteropServices;

namespace EVESharp.Destiny;

[StructLayout (LayoutKind.Sequential, Pack = 1)]
public struct Vector3
{
    public double X;
    public double Y;
    public double Z;

    public double DistanceSquare (Vector3 b)
    {
        return Math.Pow (b.X - this.X, 2) + Math.Pow (b.Y - this.Y, 2) + Math.Pow (b.Z - this.Z, 2);
    }

    public double Distance (Vector3 b)
    {
        return Math.Sqrt (this.DistanceSquare (b));
    }

    public override string ToString ()
    {
        return "(" + Math.Round (this.X) + ", " + Math.Round (this.Y) + ", " + Math.Round (this.Z) + ")";
    }
}