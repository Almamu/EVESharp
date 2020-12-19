using PythonTypes.Types.Primitives;

namespace Node.Services.Stations
{
    public class corpStationMgr : BoundService
    {
        public corpStationMgr(ServiceManager manager) : base(manager)
        {
        }

        protected override Service CreateBoundInstance(PyDataType objectData)
        {
            return new corpStationMgr(this.ServiceManager);
        }

        public PyDataType GetCorporateStationOffice(PyDictionary namedPayload, Client client)
        {
            // TODO: IMPLEMENT WHEN CORPORATION SUPPORT IS IN PLACE
            return new PyList();
        }
    }
}