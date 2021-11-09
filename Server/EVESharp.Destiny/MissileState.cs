using System.Runtime.InteropServices;

namespace EVESharp.Destiny
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct MissileState
    {
        public long UnkFollowId;
        public float Unk01;
        public long UnkSourceId;
        public float Unk02;
        public Vector3 Unk03;
    }
}