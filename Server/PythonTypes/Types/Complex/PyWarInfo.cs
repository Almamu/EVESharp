using PythonTypes.Types.Database;
using PythonTypes.Types.Primitives;

namespace PythonTypes.Types.Complex
{
    public class PyWarInfo : IndexRowset
    {
        public PyWarInfo() : base("warID", new PyDataType[]
        {
            "warID", "declaredByID", "againstID", "timeDeclared", "timeFinished", "retracted", "retractedBy", "billID", "mutual"
        })
        {
        }

        public void AddRow(int warID, int declaredByID, int againstID, long timeDeclared, long timeFinished,
            int retracted, int retractedBy, int billID, int mutual)
        {
            this.AddRow(warID, (PyList) new PyDataType[]
            {
                warID, declaredByID, againstID, timeDeclared, timeFinished, retracted, retractedBy, billID, mutual
            });
        }
    }
}