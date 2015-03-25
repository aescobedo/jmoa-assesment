using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

public interface IOrderConfiguration
{
    int WarehouseID { get; }
    string CurrencyCode { get; }
    int PriceTypeID { get; }
    int LanguageID { get; }
    int DefaultShipMethodID { get; }
    List<int> AvailableShipMethods { get; }
    string DefaultCountryCode { get; }

    int WebID { get; }
    int WebCategoryID { get; }
    string[] ItemCodes { get; }
}
