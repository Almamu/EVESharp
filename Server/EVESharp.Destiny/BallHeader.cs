using System.Runtime.InteropServices;

namespace EVESharp.Destiny;

[StructLayout (LayoutKind.Sequential, Pack = 1)]
public class BallHeader
{
    public BallFlag Flags;
    public long     ItemId;
    public Vector3  Location;

    public BallMode Mode;

    public float Radius;
}