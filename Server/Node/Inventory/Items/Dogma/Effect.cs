namespace Node.Inventory.Items.Dogma
{
    public class Effect
    {
        public int EffectID { get; }
        public string EffectName { get; }
        public int EffectCategory { get; }
        public int PreExpression { get; }
        public int PostExpression { get; }
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

        public Effect(int effect, string effectName, int effectCategory, int preExpression, int postExpression,
            string description, string guid, int? graphic, bool isOffensive, bool isAssistance, int? durationAttribute,
            int? trackingSpeedAttribute, int? dischargeAttribute, int? rangeAttribute, int? fallofAttribute,
            bool disallowAutoRepeat, bool published, string displayName, bool isWarpSafe, bool rangeChance,
            bool electronicChance, bool propulsionChance, int? distribution, string sfxName, int? npcUsageChanceAttribute,
            int? npcActivationChangeAttribute, int? fittingUsageChanceAttribute)
        {
            EffectID = effect;
            EffectName = effectName;
            EffectCategory = effectCategory;
            PreExpression = preExpression;
            PostExpression = postExpression;
            Description = description;
            GUID = guid;
            GraphicID = graphic;
            IsOffensive = isOffensive;
            IsAssistance = isAssistance;
            DurationAttributeID = durationAttribute;
            DischargeAttributeID = dischargeAttribute;
            RangeAttributeID = rangeAttribute;
            FallofAttributeID = fallofAttribute;
            DisallowAutoRepeat = disallowAutoRepeat;
            Published = published;
            DisplayName = displayName;
            IsWarpSafe = isWarpSafe;
            RangeChance = rangeChance;
            ElectronicChance = electronicChance;
            PropulsionChance = propulsionChance;
            Distribution = distribution;
            SFXName = sfxName;
            NPCUsageChanceAttributeID = npcUsageChanceAttribute;
            NPCActivationChangeAttributeID = npcActivationChangeAttribute;
            FittingUsageChanceAttributeID = fittingUsageChanceAttribute;
        }
    }
}