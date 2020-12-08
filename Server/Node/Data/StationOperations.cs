using System.Collections.Generic;

namespace Node.Data
{
    public class StationOperations
    {
        private List<int> mServices;
        private int mOperationID;

        public StationOperations(int operationID, List<int> services)
        {
            this.mOperationID = operationID;
            this.mServices = services;
        }

        public int OperationID => this.mOperationID;

        public int ServiceMask
        {
            get
            {
                int value = 0;

                foreach (int service in this.mServices)
                    value |= service;

                return value;
            }
        }
    }
}