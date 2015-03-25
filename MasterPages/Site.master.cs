using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

public partial class MasterPages_Site : System.Web.UI.MasterPage
{
    #region Page Load
    protected void Page_Load(object sender, EventArgs e)
    {
        if (Identity.Current == null)
        {
            Response.Redirect("Login.aspx");
        }

        var service = new MessagesService();
        var unreadMailCount = service.GetUnreadMailCount();

        if (unreadMailCount > 0)
        {
            var unreadMailCountAsText = (unreadMailCount > 99) ? "99+" : unreadMailCount.ToString();
            UnreadMailCountDisplay = string.Format("<span class='label label-success'>{0}</span>", unreadMailCountAsText);
        }

        if (!Page.IsPostBack)
        {
            PopulateLanguageDropDown_OnPageLoad();
        }
    }
    #endregion

    #region Properties
    public string UnreadMailCountDisplay { get; set; }

    public string languagePreferenceCookieName = GlobalSettings.Markets.LanguageCookieName;
    #endregion

    #region Localization
    protected void SetLanguage(object sender, EventArgs e)
    {
        var selectedLanguage = lstLanguage.SelectedValue;

        SetCookie(languagePreferenceCookieName, selectedLanguage);

        // Change the culture code to allow us to refresh the identity appropriately.
        System.Threading.Thread.CurrentThread.CurrentCulture = System.Globalization.CultureInfo.GetCultureInfo(selectedLanguage);
        System.Threading.Thread.CurrentThread.CurrentUICulture = System.Globalization.CultureInfo.CreateSpecificCulture(selectedLanguage);

        // Refresh the identity to account for cultural differences.
        var identityService = new IdentityAuthenticationService();
        identityService.RefreshIdentity();

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
