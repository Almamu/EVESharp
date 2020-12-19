using PythonTypes.Types.Primitives;

namespace Node.Services.War
{
    public class standing2 : Service
    {
        public standing2(ServiceManager manager) : base(manager)
        {
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
    }
}