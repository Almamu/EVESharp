using Node.Inventory.Items.Dogma;

namespace Node.StaticData.Dogma
{
    public class Effect
    {
        public int EffectID { get; }
        public string EffectName { get; }
        public EffectCategory EffectCategory { get; }
        public Expression PreExpression { get; }
        public Expression PostExpression { get; }
        public string Description { get; }
        public string GUID { get; }
        public int? GraphicID { get; }
        public bool IsOffensive { get; }
        public bool IsAssistance { get; }
        public int? DurationAttributeID { get; }
        public int? TrackingSpeedAttributeID { get; }
        public int? DischargeAttributeID { get; }
        public int? RangeAttributeID { get; }
        public int? FallofAttributeID { get; }
        public bool DisallowAutoRepeat { get; }
        public bool Published { get; }
        public string DisplayName { get; }
        public bool IsWarpSafe { get; }
        public bool RangeChance { get; }
        public bool ElectronicChance { get; }
        public bool PropulsionChance { get; }
        public int? Distribution { get; }
        public string SFXName { get; }
        public int? NPCUsageChanceAttributeID { get; }
        public int? NPCActivationChangeAttributeID { get; }
        public int? FittingUsageChanceAttributeID { get; }

        public Effect(int effect, string effectName, EffectCategory effectCategory, Expression preExpression,
            Expression postExpression, string description, string guid, int? graphic, bool isOffensive,
            bool isAssistance, int? durationAttribute, int? trackingSpeedAttribute, int? dischargeAttribute,
            int? rangeAttribute, int? fallofAttribute, bool disallowAutoRepeat, bool published, string displayName,
            bool isWarpSafe, bool rangeChance, bool electronicChance, bool propulsionChance, int? distribution,
            string sfxName, int? npcUsageChanceAttribute, int? npcActivationChangeAttribute,
            int? fittingUsageChanceAttribute)
        {
            this.EffectID = effect;
            this.EffectName = effectName;
            this.EffectCategory = effectCategory;
            this.PreExpression = preExpression;
            this.PostExpression = postExpression;
            this.Description = description;
            this.GUID = guid;
            this.GraphicID = graphic;
            this.IsOffensive = isOffensive;
            this.IsAssistance = isAssistance;
            this.DurationAttributeID = durationAttribute;
            this.DischargeAttributeID = dischargeAttribute;
            this.RangeAttributeID = rangeAttribute;
            this.FallofAttributeID = fallofAttribute;
            this.DisallowAutoRepeat = disallowAutoRepeat;
            this.Published = published;
            this.DisplayName = displayName;
            this.IsWarpSafe = isWarpSafe;
            this.RangeChance = rangeChance;
            this.ElectronicChance = electronicChance;
            this.PropulsionChance = propulsionChance;
            this.Distribution = distribution;
            this.SFXName = sfxName;
            this.NPCUsageChanceAttributeID = npcUsageChanceAttribute;
            this.NPCActivationChangeAttributeID = npcActivationChangeAttribute;
            this.FittingUsageChanceAttributeID = fittingUsageChanceAttribute;
        }
    }
}