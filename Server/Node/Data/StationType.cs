namespace Node.Data
{
    public class StationType
    {
        private int mID;
        private int? mHangarGraphicID;
        private double mDockEntryX;
        private double mDockEntryY;
        private double mDockEntryZ;
        private double mDockOrientationX;
        private double mDockOrientationY;
        private double mDockOrientationZ;
        private int? mOperationID;
        private int? mOfficeSlots;
        private double? mReprocessingEfficiency;
        private bool mConquerable;

        public int ID => this.mID;
        public int? HangarGraphicID => this.mHangarGraphicID;
        public double DockEntryX => this.mDockEntryX;
        public double DockEntryY => this.mDockEntryY;
        public double DockEntryZ => this.mDockEntryZ;
        public double DockOrientationX => this.mDockOrientationX;
        public double DockOrientationY => this.mDockOrientationY;
        public double DockOrientationZ => this.mDockOrientationZ;
        public int? OperationID => this.mOperationID;
        public int? OfficeSlots => this.mOfficeSlots;
        public double? ReprocessingEfficienty => this.mReprocessingEfficiency;
        public bool Conquerable => this.mConquerable;

        public StationType(int id, int? hangarGraphicId, double dockEntryX, double dockEntryY, double dockEntryZ,
            double dockOrientationX, double dockOrientationY, double dockOrientationZ, int? operationId,
            int? officeSlots, double? reprocessingEfficiency, bool conquerable)
        {
            this.mID = id;
            this.mHangarGraphicID = hangarGraphicId;
            this.mDockEntryX = dockEntryX;
            this.mDockEntryY = dockEntryY;
            this.mDockEntryZ = dockEntryZ;
            this.mDockOrientationX = dockOrientationX;
            this.mDockOrientationY = dockOrientationY;
            this.mDockOrientationZ = dockOrientationZ;
            this.mOperationID = operationId;
            this.mOfficeSlots = officeSlots;
            this.mReprocessingEfficiency = reprocessingEfficiency;
            this.mConquerable = conquerable;
        }
    }
}