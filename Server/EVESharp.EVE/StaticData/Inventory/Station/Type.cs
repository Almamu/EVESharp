namespace EVESharp.EVE.StaticData.Inventory.Station;

public class Type
{
    public int     ID                     { get; }
    public int?    HangarGraphicID        { get; }
    public double  DockEntryX             { get; }
    public double  DockEntryY             { get; }
    public double  DockEntryZ             { get; }
    public double  DockOrientationX       { get; }
    public double  DockOrientationY       { get; }
    public double  DockOrientationZ       { get; }
    public int?    OperationID            { get; }
    public int?    OfficeSlots            { get; }
    public double? ReprocessingEfficienty { get; }
    public bool    Conquerable            { get; }

    public Type (
        int    id,               int?    hangarGraphicId,        double dockEntryX,       double dockEntryY, double dockEntryZ,
        double dockOrientationX, double  dockOrientationY,       double dockOrientationZ, int?   operationId,
        int?   officeSlots,      double? reprocessingEfficiency, bool   conquerable
    )
    {
        ID                     = id;
        HangarGraphicID        = hangarGraphicId;
        DockEntryX             = dockEntryX;
        DockEntryY             = dockEntryY;
        DockEntryZ             = dockEntryZ;
        DockOrientationX       = dockOrientationX;
        DockOrientationY       = dockOrientationY;
        DockOrientationZ       = dockOrientationZ;
        OperationID            = operationId;
        OfficeSlots            = officeSlots;
        ReprocessingEfficienty = reprocessingEfficiency;
        Conquerable            = conquerable;
    }
}