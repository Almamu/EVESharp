using System.Runtime.InteropServices;

namespace EVESharp.Destiny;

[StructLayout (LayoutKind.Sequential, Pack = 1)]
public class BallData
{
    public float   MaxVelocity;
    public float   SpeedFraction;
    public float   Unk03;
    public Vector3 Velocity;
}