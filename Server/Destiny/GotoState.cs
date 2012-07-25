using System.Runtime.InteropServices;

namespace Destiny
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct GotoState
    {
        public Vector3 Location;
    }
}