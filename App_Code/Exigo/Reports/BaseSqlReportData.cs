using Exigo.OData;
using Exigo.Reports;
using Exigo.WebService;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Script.Serialization;

public class SqlReportDataHelper
{
    #region Constructors
    public SqlReportDataHelper()
    {
        Page                = GlobalUtilities.TryParse<int>(HttpContext.Current.Request.QueryString["page"], 1);
        RowsPerPage         = GlobalUtilities.TryParse<int>(HttpContext.Current.Request.QueryString["recordcount"], 50);
        SortField           = HttpContext.Current.Request.QueryString["sortfield"].ToString();
        SortOrder           = HttpContext.Current.Request.QueryString["sortorder"].ToString();
        SearchField         = (HttpContext.Current.Request.QueryString["searchfield"] != null) ? HttpContext.Current.Request.QueryString["searchfield"].ToString() : string.Empty;
        SearchOperator      = (HttpContext.Current.Request.QueryString["searchoperator"] != null) ? HttpContext.Current.Request.QueryString["searchoperator"].ToString() : string.Empty;
        SearchFilter        = (HttpContext.Current.Request.QueryString["searchfilter"] != null) ? HttpContext.Current.Request.QueryString["searchfilter"].ToString() : string.Empty;
    }
    #endregion

    #region Properties
    public int Page { get; set; }
    public int RowsPerPage { get; set; }
    public string SortField { get; set; }
    public string SortOrder { get; set; }
    public string SearchField { get; set; }
    public string SearchOperator { get; set; }
    public string SearchFilter { get; set; }

    public string OrderByClause
    {
        get
        {
            var order = (SortOrder == "desc") ? "desc" : "";
            return SortField + " " + order;
        }
    }
    public string WhereClause
    {
        get
        {
            if(string.IsNullOrEmpty(SearchField) || string.IsNullOrEmpty(SearchOperator) || string.IsNullOrEmpty(SearchFilter)) return string.Empty;

            var clause = "";
            switch(SearchOperator)
            {
                case "eq":      clause = string.Format("{0} = '{1}'", SearchField, SearchFilter); break;
                case "neq":     clause = string.Format("{0} <> '{1}'", SearchField, SearchFilter); break;
                case "gt":      clause = string.Format("{0} > '{1}'", SearchField, SearchFilter); break;
                case "gte":     clause = string.Format("{0} >= '{1}'", SearchField, SearchFilter); break;
                case "lt":      clause = string.Format("{0} < '{1}'", SearchField, SearchFilter); break;
                case "lte":     clause = string.Format("{0} <= '{1}'", SearchField, SearchFilter); break;
                case "btw":     clause = string.Format("{0} BETWEEN '{1}' and '{2}'", SearchField, SearchFilter.Split(',')[0], SearchFilter.Split(',')[1]); break;
                case "il":      clause = string.Format("{0} IN ({1})", SearchField, SearchFilter); break;
                case "nil":     clause = string.Format("{0} NOT IN ({1})", SearchField, SearchFilter); break;
            }

            return clause + " AND";
        }
    }
    #endregion
}

public static class SqlReportSearchListJsonDataSource
{
    public static string Countries() 
    {
        // Fetch the data
        var countries = ExigoApiContext.CreateWebServiceContext().GetCountryRegions(new GetCountryRegionsRequest
        {
            CountryCode = Identity.Current.Address.Country
        }).Countries;

        // Assemble the list
        var list = new List<SearchListItem>();
        list.Add(new SearchListItem { text = "-- Any --", value = "*" });
        foreach(var response in countries)
        {
            list.Add(new SearchListItem { text = response.CountryName, value = response.CountryCode });
        }

        // Serialize the results
        var serializer = new JavaScriptSerializer();
        var json = serializer.Serialize(list);

        // Return the Json
        return json;
    }
    public static string States() 
    {
        // Fetch the data
        var regions = ExigoApiContext.CreateWebServiceContext().GetCountryRegions(new GetCountryRegionsRequest
        {
            CountryCode = Identity.Current.Address.Country
        }).Regions;

        // Assemble the list
        var list = new List<SearchListItem>();
        list.Add(new SearchListItem { text = "-- Any --", value = "*" });
        foreach(var response in regions)
        {
            list.Add(new SearchListItem { text = response.RegionName, value = response.RegionCode });
        }

        // Serialize the results
        var serializer = new JavaScriptSerializer();
        var json = serializer.Serialize(list);

        // Return the Json
        return json;
    }
    public static string Ranks() 
    {
        // Fetch the data
        var countries = ExigoApiContext.CreateODataContext().Ranks
            .Where(c => c.RankDescription != null && c.RankDescription != "")
            .Where(c => c.RankID <= 15);

        // Assemble the list
        var list = new List<SearchListItem>();
        list.Add(new SearchListItem { text = "-- Any --", value = "*" });
        foreach(var response in countries)
        {
            list.Add(new SearchListItem { text = response.RankDescription, value = response.RankID.ToString() });
        }

        // Serialize the results
        var serializer = new JavaScriptSerializer();
        var json = serializer.Serialize(list);

        // Return the Json
        return json;
    }
    public static string CustomerTypes() 
    {
        // Fetch the data
        var data = ExigoApiContext.CreateODataContext().CustomerTypes;

        // Assemble the list
        var list = new List<SearchListItem>();
        list.Add(new SearchListItem { text = "-- Any --", value = "*" });
        foreach(var response in data)
        {
            list.Add(new SearchListItem { text = response.CustomerTypeDescription, value = response.CustomerTypeID.ToString() });
        }

        // Serialize the results
        var serializer = new JavaScriptSerializer();
        var json = serializer.Serialize(list);

        // Return the Json
        return json;
    }
    public static string CustomerStatuses() 
    {
        // Fetch the data
        var data = ExigoApiContext.CreateODataContext().CustomerStatuses;

        // Assemble the list
        var list = new List<SearchListItem>();
        list.Add(new SearchListItem { text = "-- Any --", value = "*" });
        foreach(var response in data)
        {
            list.Add(new SearchListItem { text = response.CustomerStatusDescription, value = response.CustomerStatusID.ToString() });
        }

        // Serialize the results
        var serializer = new JavaScriptSerializer();
        var json = serializer.Serialize(list);

        // Return the Json
        return json;
    }
    public static string OrderStatuses() 
    {
        // Fetch the data
        var data = ExigoApiContext.CreateODataContext().OrderStatuses;

        // Assemble the list
        var list = new List<SearchListItem>();
        list.Add(new SearchListItem { text = "-- Any --", value = "*" });
        foreach(var response in data)
        {
            list.Add(new SearchListItem { text = response.OrderStatusDescription, value = response.OrderStatusID.ToString() });
        }

        // Serialize the results
        var serializer = new JavaScriptSerializer();
        var json = serializer.Serialize(list);

        // Return the Json
        return json;
    }
    public static string OrderTypes() 
    {
        // Fetch the data
        var data = ExigoApiContext.CreateODataContext().OrderTypes;

        // Assemble the list
        var list = new List<SearchListItem>();
        list.Add(new SearchListItem { text = "-- Any --", value = "*" });
        foreach(var response in data)
        {
            list.Add(new SearchListItem { text = response.OrderTypeDescription, value = response.OrderTypeID.ToString() });
        }

        // Serialize the results
        var serializer = new JavaScriptSerializer();
        var json = serializer.Serialize(list);

        // Return the Json
        return json;
    }
    public static string Periods()
    {
        // Fetch the data
        var data = new List<Period>();
	    int lastResultCount = 50;
	    int callsMade = 0;
        int periodTypeID = GlobalUtilities.GetDefaultPeriodTypeID();
        int currentPeriodID = GlobalUtilities.GetCurrentPeriodID();
	    while(lastResultCount == 50)
	    {
		    var results = ExigoApiContext.CreateODataContext().Periods
                .Where(c => c.PeriodTypeID <= periodTypeID)
                .Where(c => c.PeriodID <= currentPeriodID)
			    .OrderByDescending(c => c.PeriodID)
			    .Skip(callsMade * 50)
			    .Take(50)
			    .Select(c => c)			
			    .ToList();

		    results.ForEach(c => data.Add(c));

		    callsMade++;
		    lastResultCount = results.Count;
	    }






        // Assemble the list
        var list = new List<SearchListItem>();
        list.Add(new SearchListItem { text = "-- Any --", value = "*" });
        foreach(var response in data)
        {
            list.Add(new SearchListItem { text = response.PeriodDescription, value = response.PeriodID.ToString() });
        }

        // Serialize the results
        var serializer = new JavaScriptSerializer();
        var json = serializer.Serialize(list);

        // Return the Json
        return json;
    }
    public static string PeriodTypes() 
    {
        // Fetch the data
        var data = ExigoApiContext.CreateODataContext().PeriodTypes.OrderBy(c => c.PeriodTypeDescription);

        // Assemble the list
        var list = new List<SearchListItem>();
        list.Add(new SearchListItem { text = "-- Any --", value = "*" });
        foreach(var response in data)
        {
            list.Add(new SearchListItem { text = response.PeriodTypeDescription, value = response.PeriodTypeID.ToString() });
        }

        // Serialize the results
        var serializer = new JavaScriptSerializer();
        var json = serializer.Serialize(list);

        // Return the Json
        return json;
    }
}