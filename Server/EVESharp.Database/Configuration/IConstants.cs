namespace EVESharp.Database.Configuration;

public interface IConstants
{
    Constant this [string name] { get; }

#region Easy accessors for constants
    public Constant CostCloneContract => this [Constants.costCloneContract];
    public Constant LocationRecycler  => this [Constants.locationRecycler];
    public Constant LocationSystem    => this [Constants.locationSystem];
    public Constant LocationUniverse  => this [Constants.locationUniverse];
    public Constant LocationMarket    => this [Constants.locationMarket];
    public Constant LocationTemp      => this [Constants.locationTemp];

    public Constant OwnerSecureCommerceCommission => this [Constants.ownerSecureCommerceCommission];
    public Constant OwnerBank                     => this [Constants.ownerBank];

    public Constant MarketTransactionTax       => this [Constants.mktTransactionTax];
    public Constant MarketCommissionPercentage => this [Constants.marketCommissionPercentage];
    public Constant MarketMinimumFee           => this [Constants.mktMinimumFee];
    public Constant MarketModificationDelay    => this [Constants.mktModificationDelay];

    public Constant SkillPointMultiplier => this [Constants.skillPointMultiplier];

    public Constant CorporationStartupCost            => this [Constants.corporationStartupCost];
    public Constant CorporationAdvertisementFlatFee   => this [Constants.corporationAdvertisementFlatFee];
    public Constant CorporationAdvertisementDailyRate => this [Constants.corporationAdvertisementDailyRate];
    public Constant MedalTaxCorporation               => this [Constants.medalTaxCorporation];
    public Constant MedalCost                         => this [Constants.medalCost];
    public Constant WarDeclarationCost                => this [Constants.warDeclarationCost];

    public Constant AllianceCreationCost   => this [Constants.allianceCreationCost];
    public Constant AllianceMembershipCost => this [Constants.allianceMembershipCost];

    public Constant ContractCourierMaxVolume => this [Constants.conCourierMaxVolume];
    public Constant ContractBidMinimum        => this [Constants.conBidMinimum];

#endregion
}