using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Web;

public class IdentityAddress
{
	public IdentityAddress()
	{
		var command = new SqlHelper();
        var row = command.GetRow(@"
                SELECT 
                    Address1 = c.MainAddress1,
                    Address2 = c.MainAddress2,
                    City = c.MainCity,
                    State = c.MainState,
                    Zip = c.MainZip,
                    Country = c.MainCountry
                FROM
	                Customers c
                WHERE
	                c.CustomerID = {0}
            ", Identity.Current.CustomerID);

        if(row != null)
        {
            this.Address1           = row["Address1"].ToString();
            this.Address2           = row["Address2"].ToString();
            this.City               = row["City"].ToString();
            this.State              = row["State"].ToString();
            this.Zip                = row["Zip"].ToString();
            this.Country            = row["Country"].ToString();
        }
	}

    public string Address1 { get; set; }
    public string Address2 { get; set; }
    public string City { get; set; }
    public string State { get; set; }
    public string Zip { get; set; }
    public string Country { get; set; }
}