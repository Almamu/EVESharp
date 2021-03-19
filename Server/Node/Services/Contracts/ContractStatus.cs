namespace Node.Services.Contracts
{
    public enum ContractStatus : int
    {
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