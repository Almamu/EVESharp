using Common.Database;
using Common.Services;
using Node.Database;
using PythonTypes.Types.Exceptions;
using PythonTypes.Types.Primitives;
using SimpleInjector;

namespace Node.Services.Corporations
{
    public class corporationSvc : Service
    {
        private CorporationDB DB { get; }
        
        public corporationSvc(CorporationDB db)
        {
            this.DB = db;
        }

        public PyDataType GetFactionInfo(PyDictionary namedPayload, Client client)
        {
            return new PyTuple(new PyDataType[]
            {
                this.DB.ListAllCorpFactions(), this.DB.ListAllFactionRegions(), this.DB.ListAllFactionConstellations(),
                this.DB.ListAllFactionSolarSystems(), this.DB.ListAllFactionRaces(), this.DB.ListAllFactionStationCount(),
                this.DB.ListAllFactionSolarSystemCount(), this.DB.ListAllNPCCorporationInfo()
            });
        }

        public PyDataType GetNPCDivisions(PyDictionary namedPayload, Client client)
        {
            return this.DB.GetNPCDivisions();
        }

        public PyDataType GetMedalsReceived(PyInteger characterID, PyDictionary namedPayload, Client client)
        {
            return new PyTuple(new PyDataType[]
                {
                    this.DB.GetMedalsReceived(characterID),
                    new PyList() // medal data, rowset medalID, part, layer, graphic, color
                }
            );
        }

        public PyDataType GetEmploymentRecord(PyInteger characterID, PyDictionary namedPayload, Client client)
        {
            return this.DB.GetEmploymentRecord(characterID);
        }
    }
}