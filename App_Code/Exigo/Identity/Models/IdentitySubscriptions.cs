using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Web;

public class IdentitySubscriptions
{
	public IdentitySubscriptions()
	{
		var command = new SqlHelper();
        var table = command.GetTable(@"
                SELECT 
                    SubscriptionID = cs.SubscriptionID,
                    IsActive = cs.IsActive,
                    StartDate = cs.StartDate,
                    ExpirationDate = cs.ExpireDate
                FROM
	                CustomerSubscriptions cs
                WHERE
	                cs.CustomerID = {0}
            ", Identity.Current.CustomerID);


        AnnualSubscription = new IdentitySubscription();
        if(table != null && table.Rows.Count > 0)
        {
            foreach(DataRow row in table.Rows)
            {
                var subscriptionID = Convert.ToInt32(row["SubscriptionID"]);

                switch(subscriptionID)
                {
                    case 10:
                        AnnualSubscription = new IdentitySubscription();
                        AnnualSubscription.SubscriptionID = 10;
                        AnnualSubscription.IsActive = Convert.ToBoolean(row["IsActive"]);
                        AnnualSubscription.StartDate = Convert.ToDateTime(row["StartDate"]);
                        AnnualSubscription.ExpirationDate = Convert.ToDateTime(row["ExpirationDate"]);
                        break;
                }
            }
        }
	}

    public IdentitySubscription AnnualSubscription { get; set; }
}

public class IdentitySubscription
{
    public int SubscriptionID { get; set; }
    public bool IsActive { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime ExpirationDate { get; set; }
}