using System.Runtime.InteropServices;

namespace Destiny
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct MiniBall
    {
        /// <summary>
        /// relative to owner location
        /// </summary>
        public Vector3 Offset;

        public float Radius;
    }
}