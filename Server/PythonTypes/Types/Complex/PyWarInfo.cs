using PythonTypes.Types.Database;
using PythonTypes.Types.Primitives;

namespace PythonTypes.Types.Complex
{
    public class PyWarInfo : IndexRowset
    {
        public PyWarInfo() : base("warID", new string[]
        {
            "warID", "declaredByID", "againstID", "timeDeclared", "timeFinished", "retracted", "retractedBy", "billID", "mutual"
        })
        {
        }

        public void AddRow(int warID, int declaredByID, int againstID, long timeDeclared, long timeFinished,
            int retracted, int retractedBy, int billID, int mutual)
        {
            this.AddRow(warID, new PyList(new PyDataType[]
            {
                warID, declaredByID, againstID, timeDeclared, timeFinished, retracted, retractedBy, billID, mutual
            }));
        }
    }
}