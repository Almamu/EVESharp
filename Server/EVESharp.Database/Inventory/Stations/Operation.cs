using System.Collections.Generic;

namespace EVESharp.Database.Inventory.Stations;

public class Operation
{
    public int        OperationID { get; }
    public string     Name        { get; }
    public string     Description { get; }
    public List <int> Services    { get; }
    public int ServiceMask
    {
        get
        {
            int value = 0;

            foreach (int service in this.Services)
                value |= service;

            return value;
        }
    }

    public Operation (int operationID, string name, string description, List <int> services)
    {
        this.Name        = name;
        this.Description = description;
        this.OperationID = operationID;
        this.Services    = services;
    }
}