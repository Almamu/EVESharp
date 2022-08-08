using System.Collections.Generic;
using EVESharp.EVE.Data;
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

    public Constant CostCloneContract => this [EVE.Data.Constants.costCloneContract];
    public Constant LocationRecycler  => this [EVE.Data.Constants.locationRecycler];
    public Constant LocationSystem    => this [EVE.Data.Constants.locationSystem];
    public Constant LocationUniverse  => this [EVE.Data.Constants.locationUniverse];
    public Constant LocationMarket    => this [EVE.Data.Constants.locationMarket];
    public Constant LocationTemp      => this [EVE.Data.Constants.locationTemp];

    public Constant OwnerSecureCommerceCommission => this [EVE.Data.Constants.ownerSecureCommerceCommission];
    public Constant OwnerBank                     => this [EVE.Data.Constants.ownerBank];

    public Constant MarketTransactionTax       => this [EVE.Data.Constants.mktTransactionTax];
    public Constant MarketCommissionPercentage => this [EVE.Data.Constants.marketCommissionPercentage];
    public Constant MarketMinimumFee           => this [EVE.Data.Constants.mktMinimumFee];
    public Constant MarketModificationDelay    => this [EVE.Data.Constants.mktModificationDelay];

    public Constant SkillPointMultiplier => this [EVE.Data.Constants.skillPointMultiplier];

    public Constant CorporationStartupCost            => this [EVE.Data.Constants.corporationStartupCost];
    public Constant CorporationAdvertisementFlatFee   => this [EVE.Data.Constants.corporationAdvertisementFlatFee];
    public Constant CorporationAdvertisementDailyRate => this [EVE.Data.Constants.corporationAdvertisementDailyRate];
    public Constant MedalTaxCorporation               => this [EVE.Data.Constants.medalTaxCorporation];
    public Constant MedalCost                         => this [EVE.Data.Constants.medalCost];
    public Constant WarDeclarationCost                => this [EVE.Data.Constants.warDeclarationCost];

    public Constant AllianceCreationCost   => this [EVE.Data.Constants.allianceCreationCost];
    public Constant AllianceMembershipCost => this [EVE.Data.Constants.allianceMembershipCost];

#endregion
}