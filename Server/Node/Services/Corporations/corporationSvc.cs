using Common.Database;
using Common.Services;
using Node.Database;
using Node.Network;
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

        public PyDataType GetFactionInfo(CallInformation call)
        {
            return new PyTuple(new PyDataType[]
            {
                this.DB.ListAllCorpFactions(), this.DB.ListAllFactionRegions(), this.DB.ListAllFactionConstellations(),
                this.DB.ListAllFactionSolarSystems(), this.DB.ListAllFactionRaces(), this.DB.ListAllFactionStationCount(),
                this.DB.ListAllFactionSolarSystemCount(), this.DB.ListAllNPCCorporationInfo()
            });
        }

        public PyDataType GetNPCDivisions(CallInformation call)
        {
            return this.DB.GetNPCDivisions();
        }

        public PyDataType GetMedalsReceived(PyInteger characterID, CallInformation call)
        {
            return new PyTuple(new PyDataType[]
                {
                    this.DB.GetMedalsReceived(characterID),
                    new PyList() // medal data, rowset medalID, part, layer, graphic, color
                }
            );
        }

        public PyDataType GetEmploymentRecord(PyInteger characterID, CallInformation call)
        {
            return this.DB.GetEmploymentRecord(characterID);
        }
    }
}