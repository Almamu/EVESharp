namespace Destiny
{
    public class Ball
    {
        public BallHeader Header { get; set; }
        public ExtraBallHeader ExtraHeader { get; set; }
        public BallData Data { get; set; }
        public byte FormationId { get; set; }
        public string Name { get; set; }
        public MiniBall[] MiniBalls { get; set; }

        public FollowState FollowState { get; set; }
        public FormationState FormationState { get; set; }
        public TrollState TrollState { get; set; }
        public MissileState MissileState { get; set; }
        public GotoState GotoState { get; set; }
        public WarpState WarpState { get; set; }
        public MushroomState MushroomState { get; set; }

        public override string ToString()
        {
            return "(" + Header.ItemId + ((Name == null || Name == "A") ? ")" : " " + Name + ")");
        }

        public override bool Equals(object obj)
        {
            if (obj == null || GetType() != obj.GetType())
                return false;

            return ((Ball)obj).Header.ItemId == Header.ItemId;
        }

        public override int GetHashCode()
        {
            return Header.ItemId.GetHashCode();
        }
    }
}