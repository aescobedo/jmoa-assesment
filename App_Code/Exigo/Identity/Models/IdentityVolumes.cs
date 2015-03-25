using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Data.SqlTypes;
using System.Linq;
using System.Web;

public class IdentityVolumes
{
	public IdentityVolumes()
	{
		var command = new SqlHelper();
        var row = command.GetRow(@"
                DECLARE @currentPeriodID int

                SELECT TOP 1 
                    @currentPeriodID = p.PeriodID
                FROM
	                Periods p
                WHERE
	                p.PeriodTypeID = {1}
                    AND p.StartDate <= {2}
	                AND p.EndDate >= {2}

                SELECT 
                    Volume1,
                    Volume2,
                    Volume3
                FROM
	                PeriodVolumes pv
                WHERE
	                pv.CustomerID = {0}
	                AND pv.PeriodTypeID = {1}
	                AND pv.PeriodID = @currentPeriodID
            ", Identity.Current.CustomerID,
             (int)PeriodTypes.Default,
             new SqlDateTime(DateTime.Now));

        if(row != null)
        {
            this.Volume1                    = Convert.ToDecimal(row["Volume1"]);
            this.Volume2                    = Convert.ToDecimal(row["Volume2"]);
            this.Volume3                    = Convert.ToDecimal(row["Volume3"]);
        }
	}

    public decimal Volume1 { get; set; }
    public decimal Volume2 { get; set; }
    public decimal Volume3 { get; set; }
}