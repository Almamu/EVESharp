using System;

namespace EVESharp.Destiny
{
    [Flags]
    public enum BallFlag : byte
    {
        /// <summary>
        /// set if ball is free to move, has extra BallData
        /// </summary>
        IsFree = 1,

        /// <summary>
        /// set if ball should be visible from all
        /// </summary>
        IsGlobal = 1 << 1,

        /// <summary>
        /// set if ball is solid
        /// </summary>
        IsMassive = 1 << 2,

        /// <summary>
        /// set if ball is interactive
        /// </summary>
        IsInteractive = 1 << 3,

        /// <summary>
        /// if set, the reader tries to read extra mini balls
        /// </summary>
        HasMiniBalls = 1 << 6,
    }
}