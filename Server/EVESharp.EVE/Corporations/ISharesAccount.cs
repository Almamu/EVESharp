using System;

namespace EVESharp.EVE.Corporations;

public interface ISharesAccount : IDisposable
{
    public int OwnerID { get; set; }
}