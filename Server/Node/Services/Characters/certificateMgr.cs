using PythonTypes.Types.Complex;
using PythonTypes.Types.Primitives;

namespace Node.Services.Characters
{
    public class certificateMgr : Service
    {
        public certificateMgr(ServiceManager manager) : base(manager)
        {
        }

        public PyDataType GetAllShipCertificateRecommendations(PyDictionary namedPayload, Client client)
        {
            this.ServiceManager.CacheStorage.Load(
                "certificateMgr",
                "GetAllShipCertificateRecommendations",
                "SELECT shipTypeID, certificateID, recommendationLevel, recommendationID FROM crtRecommendations",
                CacheStorage.CacheObjectType.Rowset
            );

            PyDataType cacheHint = this.ServiceManager.CacheStorage.GetHint("certificateMgr", "GetAllShipCertificateRecommendations");

            return PyCacheMethodCallResult.FromCacheHint(cacheHint);
        }
    }
}