using System.Collections.Generic;
using EVESharp.EVE.Data;
using EVESharp.EVE.Data.Configuration;
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

    public Constant CostCloneContract => this [EVE.Data.Configuration.Constants.costCloneContract];
    public Constant LocationRecycler  => this [EVE.Data.Configuration.Constants.locationRecycler];
    public Constant LocationSystem    => this [EVE.Data.Configuration.Constants.locationSystem];
    public Constant LocationUniverse  => this [EVE.Data.Configuration.Constants.locationUniverse];
    public Constant LocationMarket    => this [EVE.Data.Configuration.Constants.locationMarket];
    public Constant LocationTemp      => this [EVE.Data.Configuration.Constants.locationTemp];

    public Constant OwnerSecureCommerceCommission => this [EVE.Data.Configuration.Constants.ownerSecureCommerceCommission];
    public Constant OwnerBank                     => this [EVE.Data.Configuration.Constants.ownerBank];

    public Constant MarketTransactionTax       => this [EVE.Data.Configuration.Constants.mktTransactionTax];
    public Constant MarketCommissionPercentage => this [EVE.Data.Configuration.Constants.marketCommissionPercentage];
    public Constant MarketMinimumFee           => this [EVE.Data.Configuration.Constants.mktMinimumFee];
    public Constant MarketModificationDelay    => this [EVE.Data.Configuration.Constants.mktModificationDelay];

    public Constant SkillPointMultiplier => this [EVE.Data.Configuration.Constants.skillPointMultiplier];

    public Constant CorporationStartupCost            => this [EVE.Data.Configuration.Constants.corporationStartupCost];
    public Constant CorporationAdvertisementFlatFee   => this [EVE.Data.Configuration.Constants.corporationAdvertisementFlatFee];
    public Constant CorporationAdvertisementDailyRate => this [EVE.Data.Configuration.Constants.corporationAdvertisementDailyRate];
    public Constant MedalTaxCorporation               => this [EVE.Data.Configuration.Constants.medalTaxCorporation];
    public Constant MedalCost                         => this [EVE.Data.Configuration.Constants.medalCost];
    public Constant WarDeclarationCost                => this [EVE.Data.Configuration.Constants.warDeclarationCost];

    public Constant AllianceCreationCost   => this [EVE.Data.Configuration.Constants.allianceCreationCost];
    public Constant AllianceMembershipCost => this [EVE.Data.Configuration.Constants.allianceMembershipCost];

#endregion
}