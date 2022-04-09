using System.Collections.Generic;

namespace EVESharp.EVE.StaticData.Inventory.Station;

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

            foreach (int service in Services)
                value |= service;

            return value;
        }
    }

    public Operation (int operationID, string name, string description, List <int> services)
    {
        Name        = name;
        Description = description;
        OperationID = operationID;
        Services    = services;
    }
}