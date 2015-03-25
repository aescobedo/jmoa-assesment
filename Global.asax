<%@ Application Language="C#" %>

<script runat="server">
    void Application_Start(object sender, EventArgs e) 
    {
    }

    void Application_BeginRequest(object sender, EventArgs e)
    {
        //If the cookie that represents the current language exists, we set the culture code appropriately
        var languageCookie = Request.Cookies[GlobalSettings.Markets.LanguageCookieName];

        if (languageCookie != null && languageCookie.Value != null)
        {
            System.Threading.Thread.CurrentThread.CurrentCulture = System.Globalization.CultureInfo.GetCultureInfo(languageCookie.Value);
            System.Threading.Thread.CurrentThread.CurrentUICulture = System.Globalization.CultureInfo.CreateSpecificCulture(languageCookie.Value);
        }   
    }
    
    void Application_End(object sender, EventArgs e) 
    {
        //  Code that runs on application shutdown
    }
        
    void Application_Error(object sender, EventArgs e) 
    { 
        // Code that runs when an unhandled error occurs
    }

    void Session_Start(object sender, EventArgs e) 
    {
        // Code that runs when a new session is started
       
    }

    void Session_End(object sender, EventArgs e) 
    {
        // Code that runs when a session ends. 
        // Note: The Session_End event is raised only when the sessionstate mode
        // is set to InProc in the Web.config file. If session mode is set to StateServer 
        // or SQLServer, the event is not raised.

    }

    // Custom forms authentication designed to sign the user out automatically if they have been inactive for the pre-determined amount of time.
    public void FormsAuthentication_OnAuthenticate(object sender, FormsAuthenticationEventArgs args)
    {
        var authCookie = Request.Cookies[FormsAuthentication.FormsCookieName];
        if(authCookie != null)
        {
            string authCookieValue = authCookie.Value;
            if(!string.IsNullOrEmpty(authCookieValue))
            {
                var identity = Identity.Deserialize(authCookieValue);
                if(identity == null || identity.Expires < DateTime.UtcNow)
                {
                    FormsAuthentication.SignOut();
                }
                else
                {
                    args.User = new WebPrincipal(identity);
                    
                    // encrypt the ticket
                    var encTicket = FormsAuthentication.Encrypt(new FormsAuthenticationTicket(1,
                        identity.CustomerID.ToString(),
                        DateTime.UtcNow,
                        DateTime.UtcNow.AddMinutes(30),
                        false,
                        FormsAuthentication.Decrypt(authCookieValue).UserData));

                    // create the cookie.
                    var cookie = HttpContext.Current.Request.Cookies[FormsAuthentication.FormsCookieName]; //saved user
                    cookie.Value = encTicket;
                    Response.Cookies.Set(cookie);
                }
            }
        }
    }
       
</script>
