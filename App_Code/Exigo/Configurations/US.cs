using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Exigo.WebService;

// Enrollee
public class USEnrolleeConfiguration : IEnrolleeConfiguration
{
    public int DefaultWarehouseID               { get { return (int)Warehouses.Default; } }
    public string DefaultCurrencyCode           { get { return Exigo.Global.Currencies.USDollars; } }
    public string DefaultCountryCode            { get { return "US"; } }
    public int DefaultLanguagePreferenceID      { get { return (int)Languages.English; } }
    public int CustomerType                     { get { return (int)CustomerTypes.Distributor; } }
    public int CustomerStatusType               { get { return (int)CustomerStatusTypes.Active; } }
    public string InitialNotes                  { get { return "Distributor was created by the US API Enrollment at " + HttpContext.Current.Request.Url.AbsoluteUri + " on " + DateTime.Now.ToString("dddd, MMMM d, yyyy h:mmtt") + " CST at IP " + GlobalUtilities.GetClientIP() + " using " + HttpContext.Current.Request.Browser.Browser + " " + HttpContext.Current.Request.Browser.Version + " (" + HttpContext.Current.Request.Browser.Platform + ")."; } }

    public bool InsertIntoEnrollerTree          { get { return true; } }
    public bool InsertIntoUnilevelTree          { get { return true; } }
    public bool InsertIntoBinaryTree            { get { return false; } }
    public bool InsertIntoMatrixTree            { get { return true; } }
}

// Address Settings
public class USAddressCongifuration: IAddressConfiguration
{
    public bool AllowZipCode                    { get { return true; } }
    public string DefaultZipCode                { get { return string.Empty; } }
}

// Orders
public class USInitialOrderConfiguration : IOrderConfiguration
{
    public int WarehouseID              { get { return (int)Warehouses.Default; } }
    public string CurrencyCode          { get { return Exigo.Global.Currencies.USDollars; } }
    public int PriceTypeID              { get { return (int)PriceTypes.Distributor; } }
    public int LanguageID               { get { return (int)Languages.English; } }
    public int DefaultShipMethodID      { get { return 1; } }
    public List<int> AvailableShipMethods { get { return new List<int> { 1, 2, 3 }; } }
    public string DefaultCountryCode    { get { return "US"; } }

    public int WebID                    { get { return 10; } }
    public int WebCategoryID            { get { return 25; } }
    public string[] ItemCodes           { get { return null; } }
}

// Autoships
public class USAutoshipConfiguration : IAutoOrderConfiguration
{
    public string Description           { get { return "My Monthly Autoship"; } }
    public int WarehouseID              { get { return (int)Warehouses.Default; } }
    public string CurrencyCode          { get { return Exigo.Global.Currencies.USDollars; } }
    public int PriceTypeID              { get { return (int)PriceTypes.Distributor; } }
    public int LanguageID               { get { return (int)Languages.English; } }
    public int DefaultShipMethodID      { get { return 1; } }
    public List<int> AvailableShipMethods { get { return new List<int> { 1, 2, 3 }; } }
    public string DefaultCountryCode    { get { return "US"; } }
    public FrequencyType Frequency      { get { return FrequencyType.Monthly; } }
    public DateTime StartDate           { get { return GlobalUtilities.GetNewAutoOrderStartDate(FrequencyType.Monthly); } }
    public DateTime? StopDate           { get { return null; } }

    public int WebID                    { get { return 10; } }
    public int WebCategoryID            { get { return 25; } }
    public string[] ItemCodes           { get { return null; } }
}

// Annual Subscription Autoships
public class USAnnualSubscriptionAutoshipConfiguration : IAutoOrderConfiguration
{
    public string Description           { get { return "My Annual Subscription"; } }
    public int WarehouseID              { get { return (int)Warehouses.Default; } }
    public string CurrencyCode          { get { return Exigo.Global.Currencies.USDollars; } }
    public int PriceTypeID              { get { return (int)PriceTypes.Distributor; } }
    public int LanguageID               { get { return (int)Languages.English; } }
    public int DefaultShipMethodID      { get { return 1; } }
    public List<int> AvailableShipMethods { get { return new List<int> { 1, 2, 3 }; } }
    public string DefaultCountryCode    { get { return "US"; } }
    public FrequencyType Frequency      { get { return FrequencyType.Yearly; } }
    public DateTime StartDate           { get { return GlobalUtilities.GetNewAutoOrderStartDate(FrequencyType.Yearly); } }
    public DateTime? StopDate           { get { return null; } }

    public int WebID                    { get { return 0; } }
    public int WebCategoryID            { get { return 0; } }
    public string[] ItemCodes           { get { return new string[] { "ANNUAL" }; } }
}

// Autoship Settings
public class USAutoshipSettingsConfiguration : IAutoOrderSettingsConfiguration
{
    public bool AllowAutoship                               { get { return true; } }
    public bool AllowStartDateChoice                        { get { return false; } }
    public List<FrequencyType> AvailableFrequencyTypes      { get { return new List<FrequencyType> { FrequencyType.Monthly };} }
    public FrequencyType DefaultFrequencyType               { get { return FrequencyType.Monthly; } }
}