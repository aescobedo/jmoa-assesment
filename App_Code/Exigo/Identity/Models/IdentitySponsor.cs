using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Web;

public class IdentitySponsor
{
	public IdentitySponsor()
	{
		var command = new SqlHelper();
        var row = command.GetRow(@"
                SELECT 
                    SponsorID = p.CustomerID,
                    FirstName = p.FirstName,
                    LastName = p.LastName,
                    Company = p.Company
                FROM
	                Customers c
                    INNER JOIN Customers p
                        ON p.CustomerID = c.SponsorID
                WHERE
	                c.CustomerID = {0}
            ", Identity.Current.CustomerID);

        if(row != null)
        {
            this.SponsorID              = Convert.ToInt32(row["SponsorID"]);
            this.FirstName              = row["FirstName"].ToString();
            this.LastName               = row["LastName"].ToString();
            this.Company                = row["Company"].ToString();
        }
	}

    public int SponsorID { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string Company { get; set; }

    public string DisplayName
    {
        get { return GlobalUtilities.Coalesce(this.Company, this.FirstName + " " + this.LastName); }
    }
}