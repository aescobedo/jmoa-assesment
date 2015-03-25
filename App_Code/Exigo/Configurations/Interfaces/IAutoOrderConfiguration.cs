using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Exigo.WebService;

public interface IAutoOrderConfiguration : IOrderConfiguration
{
    string Description { get; }
    int WarehouseID { get; }
    string CurrencyCode { get; }
    int PriceTypeID { get; }
    int LanguageID { get; }
    int DefaultShipMethodID { get; }
    List<int> AvailableShipMethods { get; }
    string DefaultCountryCode { get; }
    FrequencyType Frequency { get; }
    DateTime StartDate { get; }
    DateTime? StopDate { get; }

    int WebID { get; }
    int WebCategoryID { get; }
    string[] ItemCodes { get; }
}
