using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Security;
using Exigo.OData;
using Exigo.WebService;
using System.Data.SqlClient;
using System.Data;

public interface IAuthenticationService
{
    bool SignIn(string loginName, string password);
    void SignOut();
}
public class IdentityAuthenticationService : IAuthenticationService
{
    /// <summary>
    /// Signs the customer into the backoffice.
    /// </summary>
    /// <param name="loginName">The customer's login name</param>
    /// <param name="password">The customer's password</param>
    /// <returns>Whether or not the customer was successfully signed in.</returns>
    public bool SignIn(string loginName, string password)
    {
        object oCustomerID  = null;

        var command = new SqlHelper();
        oCustomerID = command.GetField("AuthenticateCustomer {0}, {1}", loginName, password);

        if (oCustomerID == null) return false;
        return CreateFormsAuthenticationTicket((int)oCustomerID);
    }

    /// <summary>
    /// Signs the customer into the backoffice.
    /// </summary>
    /// <param name="customerID">The customer's ID.</param>
    /// <param name="loginName">The customer's login name.</param>
    /// <returns>Whether or not the customer was successfully signed in.</returns>
    public bool SilentLogin(int customerID, string loginName)
    {
        var cust = (from c in ExigoApiContext.CreateODataContext().Customers
                    where c.CustomerID == customerID
                    where c.LoginName == loginName
                    select new Customer { CustomerID = c.CustomerID }).FirstOrDefault();

        if (cust != null) return CreateFormsAuthenticationTicket(cust.CustomerID);
        else return false;
    }

    /// <summary>
    /// Signs the customer into the backoffice.
    /// </summary>
    /// <param name="sessionID">A SessionID created by the Exigo web service's LoginCustomer method.</param>
    /// <returns>Whether or not the customer was successfully signed in.</returns>
    public bool SilentLogin(string sessionID)
    {
        var response = ExigoApiContext.CreateWebServiceContext().GetLoginSession(new GetLoginSessionRequest
        {
            SessionID = sessionID
        });

        if (response.Result.Status == ResultStatus.Success && response.CustomerID > 0) return CreateFormsAuthenticationTicket(response.CustomerID);
        else return false;
    }

    /// <summary>
    /// Refreshes the current identity.
    /// </summary>
    /// <returns>Whether or not the customer was successfully refreshed.</returns>
    public bool RefreshIdentity()
    {
        return CreateFormsAuthenticationTicket(Identity.Current.CustomerID);
    }

    /// <summary>
    /// Signs the user out of the backoffice
    /// </summary>
    public void SignOut()
    {
        FormsAuthentication.SignOut();
    }
    
    /// <summary>
    /// Creates the forms authentication ticket
    /// </summary>
    /// <param name="customerID">The customer ID</param>
    /// <returns>Whether or not the ticket was created successfully.</returns>
    public bool CreateFormsAuthenticationTicket(int customerID)
    {
        var command = new SqlHelper();
        //ToDo: Create a SQL Query to retrieve the customer information.
        DataRow row = null;
        
        
        DateTime now = DateTime.UtcNow;
      
        FormsAuthenticationTicket ticket = new FormsAuthenticationTicket(1,
            customerID.ToString(),
            now,
            now.AddMinutes(30),
            false,
            string.Format("{0}|{1}|{2}|{3}|{4}|{5}|{6}|{7}|{8}|{9}|{10}",
                customerID,
                row["FirstName"].ToString(),
                row["LastName"].ToString(),
                row["Company"].ToString(),

                Convert.ToInt32(row["LanguageID"]),
                Convert.ToInt32(row["CustomerTypeID"]),
                Convert.ToInt32(row["CustomerStatusID"]),
                Convert.ToInt32(row["DefaultWarehouseID"]),
                "",
                row["CurrencyCode"].ToString(),
                Convert.ToDateTime(row["CreatedDate"].ToString(), System.Threading.Thread.CurrentThread.CurrentUICulture)
            ));

        // encrypt the ticket
        string encTicket = FormsAuthentication.Encrypt(ticket);

        // create the cookie.
        HttpCookie cookie = HttpContext.Current.Request.Cookies[FormsAuthentication.FormsCookieName]; //saved user
        if (cookie == null)
        {
            HttpContext.Current.Response.Cookies.Add(new HttpCookie(FormsAuthentication.FormsCookieName, encTicket));
        }
        else
        {
            cookie.Value = encTicket;
            HttpContext.Current.Response.Cookies.Set(cookie);
        }
        return true;
    }
}