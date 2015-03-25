using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

public interface IAddressConfiguration
{
    bool AllowZipCode { get; }
    string DefaultZipCode { get; }
}
