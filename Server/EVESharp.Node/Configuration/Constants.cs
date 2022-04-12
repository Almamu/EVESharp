using System.Collections.Generic;
using EVESharp.EVE.StaticData;
using EVESharp.Node.Database;

namespace EVESharp.Node.Configuration;

/// <summary>
/// List of constants for the EVE Server
/// </summary>
public class Constants
{
    private readonly Dictionary <string, Constant> mConstants;

    public Constant this [string name] => this.mConstants [name];

    public Constants (GeneralDB generalDB)
    {
        this.mConstants = generalDB.LoadConstants ();
    }

#region Easy accessors for constants

    public Constant CostCloneContract => this [EVE.StaticData.Constants.costCloneContract];
    public Constant LocationRecycler  => this [EVE.StaticData.Constants.locationRecycler];
    public Constant LocationSystem    => this [EVE.StaticData.Constants.locationSystem];
    public Constant LocationUniverse  => this [EVE.StaticData.Constants.locationUniverse];
    public Constant LocationMarket    => this [EVE.StaticData.Constants.locationMarket];
    public Constant LocationTemp      => this [EVE.StaticData.Constants.locationTemp];

    public Constant OwnerSecureCommerceCommission => this [EVE.StaticData.Constants.ownerSecureCommerceCommission];
    public Constant OwnerBank                     => this [EVE.StaticData.Constants.ownerBank];

    public Constant MarketTransactionTax       => this [EVE.StaticData.Constants.mktTransactionTax];
    public Constant MarketCommissionPercentage => this [EVE.StaticData.Constants.marketCommissionPercentage];
    public Constant MarketMinimumFee           => this [EVE.StaticData.Constants.mktMinimumFee];
    public Constant MarketModificationDelay    => this [EVE.StaticData.Constants.mktModificationDelay];

    public Constant SkillPointMultiplier => this [EVE.StaticData.Constants.skillPointMultiplier];

    public Constant CorporationStartupCost            => this [EVE.StaticData.Constants.corporationStartupCost];
    public Constant CorporationAdvertisementFlatFee   => this [EVE.StaticData.Constants.corporationAdvertisementFlatFee];
    public Constant CorporationAdvertisementDailyRate => this [EVE.StaticData.Constants.corporationAdvertisementDailyRate];
    public Constant MedalTaxCorporation               => this [EVE.StaticData.Constants.medalTaxCorporation];
    public Constant MedalCost                         => this [EVE.StaticData.Constants.medalCost];
    public Constant WarDeclarationCost                => this [EVE.StaticData.Constants.warDeclarationCost];

    public Constant AllianceCreationCost   => this [EVE.StaticData.Constants.allianceCreationCost];
    public Constant AllianceMembershipCost => this [EVE.StaticData.Constants.allianceMembershipCost];

#endregion
}