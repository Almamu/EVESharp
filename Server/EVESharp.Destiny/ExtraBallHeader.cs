using System.Runtime.InteropServices;

namespace EVESharp.Destiny;

[StructLayout (LayoutKind.Sequential, Pack = 1)]
public class ExtraBallHeader
{
    public long AllianceId;

    public CloakMode CloakMode;
    public int       CorporationId;

    /// <summary>
    /// seen all 0xFF. shield harmonic value of ball.
    /// </summary>
    public float Harmonic;
    /// <summary>
    /// xref from type information
    /// </summary>
    public double Mass;
}