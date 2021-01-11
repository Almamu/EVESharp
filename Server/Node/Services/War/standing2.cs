using Common.Database;
using Node.Database;
using PythonTypes.Types.Complex;
using PythonTypes.Types.Exceptions;
using PythonTypes.Types.Primitives;

namespace Node.Services.War
{
    public class standing2 : Service
    {
        private StandingDB mDB = null;
        
        public standing2(DatabaseConnection db, ServiceManager manager) : base(manager)
        {
            this.mDB = new StandingDB(db);
        }

        public PyDataType GetMyKillRights(PyDictionary namedPayload, Client client)
        {
            PyDictionary killRights = new PyDictionary();
            PyDictionary killedRights = new PyDictionary();

            return new PyTuple(new PyDataType[]
            {
                killRights, killedRights
            });
        }

        public PyDataType GetNPCNPCStandings(PyDictionary namedPayload, Client client)
        {
            this.ServiceManager.CacheStorage.Load(
                "standing2",
                "GetNPCNPCStandings",
                "SELECT fromID, toID, standing FROM npcStandings",
                CacheStorage.CacheObjectType.Rowset
            );

            PyDataType cacheHint = this.ServiceManager.CacheStorage.GetHint("standing2", "GetNPCNPCStandings");

            return PyCacheMethodCallResult.FromCacheHint(cacheHint);
        }

        public PyDataType GetCharStandings(PyDictionary namedPayload, Client client)
        {
            if (client.CharacterID == null)
                throw new UserError("NoCharacterSelected");

            return new PyTuple(new PyDataType[]
            {
                this.mDB.GetCharStandings((int) client.CharacterID),
                this.mDB.GetCharPrime((int) client.CharacterID),
                this.mDB.GetCharNPCStandings((int) client.CharacterID)
            });
        }

        public PyDataType GetStandingTransactions(PyInteger from, PyInteger to, PyInteger direction, PyInteger eventID,
            PyInteger eventTypeID, PyInteger eventDateTime, PyDictionary namedPayload, Client client)
        {
            return this.mDB.GetStandingTransactions(from, to, direction, eventID, eventTypeID, eventDateTime);
        }
    }
}