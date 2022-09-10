using EVESharp.EVE.Types;
using EVESharp.Types;
using EVESharp.Types.Collections;

namespace EVESharp.EVE.Packets.Complex;

/// <summary>
/// Simple base class to simplify working with Item information
/// </summary>
public class WarInfo : IndexRowset
{
    public WarInfo() : base("warID", new PyList<PyString>(9)
    {
        [0] = "warID",
        [1] = "declaredByID",
        [2] = "againstID",
        [3] = "timeDeclared",
        [4] = "timeFinished",
        [5] = "retracted",
        [6] = "retractedBy",
        [7] = "billID",
        [8] = "mutual"
    })
    {
    }

    public void AddRow(int warID,     int declaredByID, int againstID, long timeDeclared, long timeFinished,
                       int retracted, int retractedBy,  int billID,    int  mutual)
    {
        this.AddRow(warID, new PyList(9)
        {
            [0] = warID,
            [1] = declaredByID,
            [2] = againstID,
            [3] = timeDeclared,
            [4] = timeFinished,
            [5] = retracted,
            [6] = retractedBy,
            [8] = billID,
            [9] = mutual
        });
    }
}