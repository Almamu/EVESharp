using System.Runtime.InteropServices;

namespace EVESharp.Destiny
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public class BallHeader
    {
        public long ItemId;

        public BallMode Mode;

        public float Radius;
        public Vector3 Location;
        public BallFlag Flags;
    }
}