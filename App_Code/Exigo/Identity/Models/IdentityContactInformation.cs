using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Web;

public class IdentityContactInformation
{
	public IdentityContactInformation()
	{
		var command = new SqlHelper();
        var row = command.GetRow(@"
                SELECT 
                    Email = c.Email,
                    Phone = c.Phone,
                    Phone2 = c.Phone2,
                    MobilePhone = c.MobilePhone,
                    Fax = c.Fax
                FROM
	                Customers c
                WHERE
	                c.CustomerID = {0}
            ", Identity.Current.CustomerID);

        if(row != null)
        {
            this.Email                  = row["Email"].ToString();
            this.Phone                  = row["Phone"].ToString();
            this.Phone2                 = row["Phone2"].ToString();
            this.MobilePhone            = row["MobilePhone"].ToString();
            this.Fax                    = row["Fax"].ToString();
        }
	}

    public string Email { get; set; }
    public string Phone { get; set; }
    public string Phone2 { get; set; }
    public string MobilePhone { get; set; }
    public string Fax { get; set; }
}