using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Web;

public class IdentityWebsite
{
	public IdentityWebsite()
	{
		var command = new SqlHelper();
        var row = command.GetRow(@"
                SELECT 
                    WebAlias = cs.WebAlias,
                    LoginName = c.LoginName
                FROM 
                    Customers c
                    INNER JOIN CustomerSites cs
                            on cs.CustomerID = c.CustomerID
                WHERE
	                c.CustomerID = {0}
            ", Identity.Current.CustomerID);

        if(row != null)
        {
            this.WebAlias           = row["WebAlias"].ToString();
            this.LoginName          = row["LoginName"].ToString();
        }
	}

    public string WebAlias { get; set; }
    public string LoginName { get; set; }
}