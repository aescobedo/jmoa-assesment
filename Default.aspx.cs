using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

public partial class Default : Page
{
    #region Page Load
    protected void Page_Load(object sender, EventArgs e)
    {
        // Redirects the user to either the login page or the home page of the backoffice, depending on whether or not they are signed in.
        if (Identity.Current == null) Response.Redirect("Login.aspx" + Request.Url.Query);
        else Response.Redirect("Home.aspx" + Request.Url.Query);
    }
    #endregion
}