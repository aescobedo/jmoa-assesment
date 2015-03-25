using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

public partial class Login : Page
{
    #region Page Load
    protected void Page_Load(object sender, EventArgs e)
    {
        chkRememberMe.Text = Resources.Login.RememberMeOnThisComputer;
        rtxtPassword.ErrorMessage = Resources.Login.Required;
        txtPassword.Attributes.Add("placeholder", Resources.Login.Password);

        rtxtLoginName.ErrorMessage = Resources.Login.Required;
        txtLoginName.Attributes.Add("placeholder", Resources.Login.Username);

        if (!IsPostBack)
        {
            PopulateLanguageDropDown_OnPageLoad();

            // Auto-populate the username from the cookie
            if (Request.Cookies[UsernameCookieName] != null)
            {
                chkRememberMe.Checked = true;
                LoginName = Request.Cookies[UsernameCookieName].Value;
                txtPassword.Focus();
            }
            else
            {
                txtLoginName.Focus();
            }


            // Auto-fill some test credentials if we are running this website on our local machines. This is just for convenience.
            if (Request.IsLocal || Request.Url.AbsoluteUri.Contains("sample.exigo.com"))
            {
                txtLoginName.Text = GlobalSettings.LocalSettings.TestLoginName;
                txtPassword.Attributes.Add("value", GlobalSettings.LocalSettings.TestPassword);
            }
        }
    }
    #endregion

    #region Properties
    public string ErrorString { get; set; }

    public string LoginName
    {
        get { return txtLoginName.Text; }
        set { txtLoginName.Text = value; }
    }
    public string Password
    {
        get { return txtPassword.Text; }
        set { txtPassword.Text = value; }
    }
    public bool RememberMe
    {
        get { return chkRememberMe.Checked; }
        set { chkRememberMe.Checked = value; }
    }

    public string UsernameCookieName = "Username";

    public string languagePreferenceCookieName = GlobalSettings.Markets.LanguageCookieName;
    #endregion

    #region Helper Methods
    public void SaveUsernameCookie()
    {
        var cookie = Request.Cookies[UsernameCookieName] ?? new HttpCookie(UsernameCookieName);
        cookie.Value = LoginName;
        cookie.Expires = DateTime.Now.AddDays(30);
        Response.Cookies.Add(cookie);
    }
    public void DeleteUsernameCookie()
    {
        HttpCookie cookie = Request.Cookies[UsernameCookieName];
        if (cookie != null)
        {
            cookie = new HttpCookie(UsernameCookieName);
            cookie.Value = string.Empty;
            cookie.Expires = DateTime.Now.AddDays(-1);
            Response.Cookies.Add(cookie);
        }
    }
    #endregion

    #region Event Handlers
    public void SignIn_Click(object sender, EventArgs e)
    {
        // If they want to be remembered, let's save their username to a cookie. If not, let's kill any cookies that might already exist.
        if (RememberMe) SaveUsernameCookie();
        else DeleteUsernameCookie();


        var svc = new IdentityAuthenticationService();
        if (svc.SignIn(LoginName, Password))
        {
            if (Request.QueryString["ReturnUrl"] != null)
            {
                Response.Redirect(Request.QueryString["ReturnUrl"], false);
            }
            else
            {
                Response.Redirect("~/Home.aspx", false);
            }
        }
        else
        {
            ErrorString = Resources.Login.Invalid;
        }

    }
    #endregion

    #region Localization
    protected void SetLanguage(object sender, EventArgs e)
    {
        var selectedLanguage = lstLanguage.SelectedValue;

        SetCookie(languagePreferenceCookieName, selectedLanguage);

        // Change the culture code to allow us to refresh the identity appropriately.
        System.Threading.Thread.CurrentThread.CurrentCulture = System.Globalization.CultureInfo.GetCultureInfo(selectedLanguage);
        System.Threading.Thread.CurrentThread.CurrentUICulture = System.Globalization.CultureInfo.CreateSpecificCulture(selectedLanguage);

        Response.Redirect(Request.Url.AbsolutePath);
    }

    public void PopulateLanguageDropDown_OnPageLoad()
    {
        lstLanguage.Items.Clear();
        var culturecode = GetCookie(languagePreferenceCookieName, "en-US");

        switch (culturecode)
        {
            case "es-US":
                lstLanguage.Items.Add(new ListItem { Text = Resources.Navigation.Lang_Spanish, Value = Resources.Navigation.Lang_Spanish_prefix });
                lstLanguage.Items.Add(new ListItem { Text = Resources.Navigation.Lang_English, Value = Resources.Navigation.Lang_English_prefix });
                lstLanguage.Items.Add(new ListItem { Text = Resources.Navigation.Lang_Malay, Value = Resources.Navigation.Lang_Malay_prefix });
                break;
            case "ms-MY":
                lstLanguage.Items.Add(new ListItem { Text = Resources.Navigation.Lang_Malay, Value = Resources.Navigation.Lang_Malay_prefix });
                lstLanguage.Items.Add(new ListItem { Text = Resources.Navigation.Lang_English, Value = Resources.Navigation.Lang_English_prefix });
                lstLanguage.Items.Add(new ListItem { Text = Resources.Navigation.Lang_Spanish, Value = Resources.Navigation.Lang_Spanish_prefix });
                break;
            case "en-US":
            default:
                lstLanguage.Items.Add(new ListItem { Text = Resources.Navigation.Lang_English, Value = Resources.Navigation.Lang_English_prefix });
                lstLanguage.Items.Add(new ListItem { Text = Resources.Navigation.Lang_Spanish, Value = Resources.Navigation.Lang_Spanish_prefix });
                lstLanguage.Items.Add(new ListItem { Text = Resources.Navigation.Lang_Malay, Value = Resources.Navigation.Lang_Malay_prefix });
                break;
        }
    }
    #endregion

    #region Cookies
    private string GetCookie(string cookieName, string defaultValue)
    {
        var cookie = Request.Cookies[cookieName];

        if (cookie == null)
        {
            cookie = CreateCookie(languagePreferenceCookieName, defaultValue);
        }

        return cookie.Value;
    }
    private void SetCookie(string cookieName, string value)
    {
        var cookie = Request.Cookies[cookieName];

        if (cookie == null)
        {
            cookie = CreateCookie(languagePreferenceCookieName, value);
        }
        else
        {
            cookie.Value = value;
        }

        SaveCookie(cookie);
    }
    private HttpCookie CreateCookie(string cookieName, string value)
    {
        var cookie = new HttpCookie(cookieName);
        cookie.Expires = DateTime.Now.AddYears(1);
        cookie.Value = value;

        SaveCookie(cookie);

        return cookie;
    }
    private void SaveCookie(HttpCookie cookie)
    {
        Response.Cookies.Add(cookie);
    }
    #endregion
}
