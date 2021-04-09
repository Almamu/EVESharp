using System.Collections.Generic;
using PythonTypes.Types.Complex;
using PythonTypes.Types.Primitives;

namespace Node.Notifications.Character
{
    public class OnStandingSet : PyNotification
    {
        private const string NOTIFICATION_NAME = "OnStandingSet";
        
        public int FromID { get; init; }
        public int ToID { get; init; }
        public double NewStanding { get; init; }
        
        public OnStandingSet(int fromID, int toID, double newStanding) : base(NOTIFICATION_NAME)
        {
            this.FromID = fromID;
            this.ToID = toID;
            this.NewStanding = newStanding;
        }

        public override List<PyDataType> GetElements()
        {
            return new List<PyDataType>()
            {
                this.FromID,
                this.ToID,
                this.NewStanding
            };
        }
    }
}