using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Security.Principal;
using System.Web.Security;

//ToDo: Fix the Identity implementation
[Serializable]
public class Identity
{
    public static Identity Current
    {
        get
        {
            return (HttpContext.Current.User.Identity as Identity);
        }
    }

    public Identity(System.Web.Security.FormsAuthenticationTicket ticket)
    {
        string[] a = ticket.UserData.Split('|');
        Name = ticket.Name;

        // WebIdentity Variables
        CustomerID                  = int.Parse(GlobalUtilities.Coalesce(a[0], "0"));
        FirstName                   = GlobalUtilities.Coalesce(a[1], "");
        LastName                    = GlobalUtilities.Coalesce(a[2], "");
        Company                     = GlobalUtilities.Coalesce(a[3], "");

        LanguageID                  = int.Parse(GlobalUtilities.Coalesce(a[4], Languages.English.ToString()));
        CustomerTypeID              = int.Parse(GlobalUtilities.Coalesce(a[5], CustomerTypes.Distributor.ToString()));
        CustomerStatusID            = int.Parse(GlobalUtilities.Coalesce(a[6], CustomerStatusTypes.Active.ToString()));
        DefaultWarehouseID          = int.Parse(GlobalUtilities.Coalesce(a[7], Warehouses.Default.ToString()));
        PriceTypeID                 = int.Parse(GlobalUtilities.Coalesce(a[8], PriceTypes.Distributor.ToString()));
        CurrencyCode                = GlobalUtilities.Coalesce(a[9], "usd");

        if(a.Length < 11) 
        {
            var service = new IdentityAuthenticationService();
            service.SignOut();
            return;
        }
        JoinedDate = DateTime.Parse(GlobalUtilities.Coalesce(a[10].ToString(), DateTime.Now.ToString()));

        Expires = ticket.Expiration.ToUniversalTime();
    }

    // Determine the culture codes
    public string CultureCode
    {
        get
        {
            return GetBrowsersDefaultCultureCode();
        }
    }
    public string UICultureCode
    {
        get
        {
            return GetBrowsersDefaultCultureCode();
        }
    }

    public string Name { get; set; }

    public int CustomerID { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string Company { get; set; }
    
    public int LanguageID { get; set; }
    public int CustomerTypeID { get; set; }
    public int CustomerStatusID { get; set; }
    public int DefaultWarehouseID { get; set; }
    public int PriceTypeID { get; set; }
    public string CurrencyCode { get; set; }
    public DateTime JoinedDate { get; set; }
    
    public IdentityAddress Address                      { get { return CacheHelper.GetFromCache<IdentityAddress>("Address"); } }
    public IdentityContactInformation ContactInfo       { get { return CacheHelper.GetFromCache<IdentityContactInformation>("ContactInformation"); } }
    public IdentityRanks Ranks                          { get { return CacheHelper.GetFromCache<IdentityRanks>("Ranks"); } }
    public IdentityVolumes Volumes                      { get { return CacheHelper.GetFromCache<IdentityVolumes>("Volumes"); } }
    public IdentitySubscriptions Subscriptions          { get { return CacheHelper.GetFromCache<IdentitySubscriptions>("Subscriptions"); } }
    public IdentityWebsite Website                      { get { return CacheHelper.GetFromCache<IdentityWebsite>("Website"); } }
    public IdentityEnroller Enroller                    { get { return CacheHelper.GetFromCache<IdentityEnroller>("Enroller"); } }
    public IdentitySponsor Sponsor                      { get { return CacheHelper.GetFromCache<IdentitySponsor>("Sponsor"); } }

    public string DisplayName
    {
        get { return GlobalUtilities.Coalesce(this.Company, this.FirstName + " " + this.LastName); }
    }
    public Market Market 
    {
        get
        {
            return GlobalSettings.Markets.AvailableMarkets.Where(c => c.Countries.Contains(this.Address.Country)).FirstOrDefault();
        }
    }
    public DateTime Expires { get; set; }
     
    private string GetBrowsersDefaultCultureCode()
    {
        string[] languages = HttpContext.Current.Request.UserLanguages;

        if (languages == null || languages.Length == 0)
            return "en-US";
        try
        {
            string language = languages[0].Trim();
            return language;
        }

        catch (ArgumentException)
        {
            return "en-US";
        }
    }

    public static Identity Deserialize(string data)
    {
        try
        {
            var ticket = FormsAuthentication.Decrypt(data);
            return new Identity(ticket);
        }
        catch(Exception ex)
        {
            var service = new IdentityAuthenticationService();
            service.SignOut();
            return null;
        }
    }
}
