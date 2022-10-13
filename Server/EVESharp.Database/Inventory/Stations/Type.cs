namespace EVESharp.Database.Inventory.Stations;

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
        this.ID                     = id;
        this.HangarGraphicID        = hangarGraphicId;
        this.DockEntryX             = dockEntryX;
        this.DockEntryY             = dockEntryY;
        this.DockEntryZ             = dockEntryZ;
        this.DockOrientationX       = dockOrientationX;
        this.DockOrientationY       = dockOrientationY;
        this.DockOrientationZ       = dockOrientationZ;
        this.OperationID            = operationId;
        this.OfficeSlots            = officeSlots;
        this.ReprocessingEfficienty = reprocessingEfficiency;
        this.Conquerable            = conquerable;
    }
}