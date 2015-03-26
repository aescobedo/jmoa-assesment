using Exigo.WebService;
using Exigo.RankQualificationGoals;
using Exigo.OData;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

public partial class Home : System.Web.UI.Page
{
    private int customerID = Identity.Current.CustomerID;
    public int rankID = Identity.Current.Ranks.CurrentPeriodRankID;
    private int periodTypeID = PeriodTypes.Default;

     
    protected void Page_Load(object sender, EventArgs e)
    {

    }  

    public int ViewingRankID
    {
        get
        {
            if (Request.QueryString["rankid"] != null)
            {
                return Convert.ToInt32(Request.QueryString["rankid"]);
            }
            else
            {
                return (Identity.Current.Ranks.CurrentPeriodRankID > 0) ? Identity.Current.Ranks.CurrentPeriodRankID : 1;
            }
        }
    }
    public string ViewingRankDescription { get; set; }

    public int CurrentRankID { get; set; }

    //ToDo: Render a message in the Home.aspx after successful login
    protected override void Render(HtmlTextWriter writer)
    {
        if (Request.QueryString["datakey"] != null)
        {
            Response.Clear();

            switch (Request.QueryString["datakey"])
            { 
                default:
                    return;
            } 
        }
        if (Identity.Current.IsAuthenticated)
        {
            Response.Write("Login Successful!");
        }
        base.Render(writer);
    }
}