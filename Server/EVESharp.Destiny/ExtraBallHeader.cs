using System.Runtime.InteropServices;

namespace EVESharp.Destiny
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public class ExtraBallHeader
    {
        /// <summary>
        /// xref from type information
        /// </summary>
        public double Mass;

        public CloakMode CloakMode;

        public long AllianceId;
        public int CorporationId;

        /// <summary>
        /// seen all 0xFF. shield harmonic value of ball.
        /// </summary>
        public float Harmonic;
    }
}