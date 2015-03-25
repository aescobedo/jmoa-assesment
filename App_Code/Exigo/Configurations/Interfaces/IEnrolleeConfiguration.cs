using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

public interface IEnrolleeConfiguration
{
    int DefaultWarehouseID { get; }
    string DefaultCountryCode { get; }
    string DefaultCurrencyCode { get; }
    int DefaultLanguagePreferenceID { get; }
    int CustomerType { get; }
    int CustomerStatusType { get; }
    string InitialNotes { get; }

    bool InsertIntoEnrollerTree { get; }
    bool InsertIntoUnilevelTree { get; }
    bool InsertIntoBinaryTree { get; }
    bool InsertIntoMatrixTree { get; }
}
