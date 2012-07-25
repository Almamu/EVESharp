using System.Runtime.InteropServices;

namespace Destiny
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct WarpState
    {
        public Vector3 Location;
        public int EffectStamp;
        public long Unk01;
        public long FollowId;
        public long OwnerId;
    }
}