namespace EVESharp.Node.Services.Contracts
{
    public enum ContractStatus : int
    {
        /// <summary>
        /// WARNING: THE EXPIRED STATUS DOES NOT REALLY EXISTS, BUT IS USED INTERNALLY BY THE SERVER CODE
        /// </summary>
        Expired = 8,
        Failed = 7,
        Rejected = 6,
        Cancelled = 5,
        Finished = 4,
        FinishedContractor = 3,
        FinishedIssuer = 2,
        InProgress = 1,
        Outstanding = 0
    }
}