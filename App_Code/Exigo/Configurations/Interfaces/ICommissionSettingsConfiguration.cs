using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Exigo.WebService;

public interface ICommissionSettingsConfiguration
{
    bool DisplaySSNField { get; }

	List<PayableType> AvailablePayableTypes { get; }
    PayableType DefaultPayableType { get; }

    List<TaxIDType> AvailableTaxIDTypes { get; }
    TaxIDType DefaultTaxIDType { get; }
}