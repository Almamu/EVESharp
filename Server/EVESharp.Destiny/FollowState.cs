using System.Runtime.InteropServices;

namespace EVESharp.Destiny
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct FollowState
    {
        public long UnkFollowId;
        public float UnkRange;
    }
}