using System.Runtime.InteropServices;

namespace EVESharp.Destiny;

[StructLayout (LayoutKind.Sequential, Pack = 1)]
public struct FormationState
{
    public long  Unk01;
    public float Unk02;
    public float Unk03;
}