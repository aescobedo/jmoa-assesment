using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Web;

public class IdentityRanks
{
	public IdentityRanks()
	{
		var command = new SqlHelper();
        var row = command.GetRow(@"
                DECLARE @defaultRankID INT
                DECLARE @defaultRankDescription NVARCHAR(50)

                SELECT TOP 1 @defaultRankID = r.RankID
		                   , @defaultRankDescription = r.RankDescription
                FROM
	                Ranks r
                ORDER BY
	                r.RankID ASC

                SELECT CurrentPeriodRankDescription = isnull(r.RankDescription, @defaultRankDescription)
	                 , CurrentPeriodRankID = CASE
		                   WHEN pv.RankID IS NULL THEN
			                   @defaultRankID
		                   WHEN pv.RankID = 0 THEN
			                   @defaultRankID
		                   ELSE
			                   pv.RankID
	                   END
	                 , HighestCurrentPeriodRankDescription = isnull(pr.RankDescription, @defaultRankDescription)
	                 , HighestCurrentPeriodRankID = CASE
		                   WHEN pv.PaidRankID IS NULL THEN
			                   @defaultRankID
		                   WHEN pv.PaidRankID = 0 THEN
			                   @defaultRankID
		                   ELSE
			                   pv.PaidRankID
	                   END
	                 , HighestAchievedRankDescription = isnull(cr.RankDescription, @defaultRankDescription)
	                 , HighestAchievedRankID = CASE
		                   WHEN c.RankID IS NULL THEN
			                   @defaultRankID
		                   WHEN c.RankID = 0 THEN
			                   @defaultRankID
		                   ELSE
			                   c.RankID
	                   END

                FROM
	                PeriodVolumes pv
	                LEFT JOIN Ranks r
		                ON r.RankID = pv.RankID
	                LEFT JOIN Ranks pr
		                ON pr.RankID = pv.RankID
	                INNER JOIN Customers c
		                ON c.CustomerID = pv.CustomerID
	                LEFT JOIN Ranks cr
		                ON cr.RankID = pv.RankID
                WHERE
	                pv.CustomerID = {0}
	                AND pv.PeriodTypeID = {1}
	                AND pv.PeriodID = {2}
            ", Identity.Current.CustomerID,
             (int)PeriodTypes.Default,
             GlobalUtilities.GetCurrentPeriodID());

        if(row != null)
        {
            this.CurrentPeriodRankID                        = Convert.ToInt32(row["CurrentPeriodRankID"]);
            this.CurrentPeriodRankDescription               = row["CurrentPeriodRankDescription"].ToString();
            this.HighestCurrentPeriodRankID                 = Convert.ToInt32(row["HighestCurrentPeriodRankID"]);
            this.HighestCurrentPeriodRankDescription        = row["HighestCurrentPeriodRankDescription"].ToString();
            this.HighestAchievedRankID                      = Convert.ToInt32(row["HighestAchievedRankID"]);
            this.HighestAchievedRankDescription             = row["HighestAchievedRankDescription"].ToString();
        }
	}

    public int CurrentPeriodRankID { get; set; }
    public string CurrentPeriodRankDescription { get; set; }

    public int HighestCurrentPeriodRankID { get; set; }
    public string HighestCurrentPeriodRankDescription { get; set; }

    public int HighestAchievedRankID { get; set; }
    public string HighestAchievedRankDescription { get; set; }
}