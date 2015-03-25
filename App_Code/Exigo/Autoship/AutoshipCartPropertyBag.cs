using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Text;
using System.IO;
using System.Xml.Serialization;
using Exigo.WebService;

public class AutoshipCartPropertyBag
{
    /// <summary>
    /// The version of the current Exigo PropertyBag object. This value is privately set.
    /// </summary>
    private string version = "1.0.7";

    #region Constructors
    // Creating a new cart from scratch
    public AutoshipCartPropertyBag()
    {
        Version = version;
        CreatedDate = DateTime.Now;
        CustomerID = Identity.Current.CustomerID;
        Domain = string.Empty;
    }

    // Load an existing cart based on the cookie name provided, and create one if none is found
    public AutoshipCartPropertyBag(string CookieName)
    {
        LoadPropertyBag(CookieName);
    }
    #endregion

    #region Object Properties
    /// <summary>
    /// The name of the current ShoppingCart object. This is also the name of the cookie used to store the unique SessionID used to get the cart XML from the GetSessionRequest() in Exigo.
    /// </summary>
    public string Description { get; set; }

    /// <summary>
    /// The version of the current ShoppingCart object. This can only be set internally, and should only change whenever a structural change to the ShoppingCart object is made. When changed, any ShoppingCart object loaded that does not match the current version will be emptied and re-created using the current version.
    /// </summary>
    public string Version { get; set; }

    /// <summary>
    /// The SessionID of the current ShoppingCart object. This is the unique key used to store and recall the data in the cart in Exigo's Session.
    /// </summary>
    public string SessionID { get; set; }

    /// <summary>
    /// The customer ID that is associated with the current ShoppingCart object. If the cart is being used in a public site, the customer ID will be 0.
    /// </summary>
    public int CustomerID { get; set; }

    /// <summary>
    /// The domain that is associated with the current ShoppingCart object. If the Domain is left empty, the cookie that stores the shopping cart will not be restricted by domain.
    /// </summary>
    public string Domain { get; set; }

    /// <summary>
    /// The date the current ShoppingCart object was created.
    /// </summary>
    public DateTime CreatedDate { get; set; }
    #endregion

    #region Serialize/Deserialize Methods
    /// <summary>
    /// Returns an XML string based on the ShoppingCart object.
    /// </summary>
    /// <returns>string</returns>
    protected string Serialize()
    {
        var sb = new StringBuilder();
        var sw = new StringWriter(sb);
        var ser = new XmlSerializer(this.GetType());
        ser.Serialize(sw, this);
        sw.Close();
        return sb.ToString();
    }

    /// <summary>
    /// Deserializes the provided string into the object cast as the provided type.
    /// </summary>
    /// <typeparam name="T">The type of item to base the XML schema on.</typeparam>
    /// <param name="s">The serialized XML based on the XML schema of the provided type.</param>
    /// <returns>Type</returns>
    protected static T Deserialize<T>(string s) where T : class
    {
        var ser = new XmlSerializer(typeof(T));
        var sr = new StringReader(s);
        return ser.Deserialize(sr) as T;
    }
    #endregion

    #region Public Methods
    /// <summary>
    /// Saves the cart to the Exigo Session.
    /// </summary>
    public void Save()
    {
        ExigoApiContext.CreateWebServiceContext().SetSession(new SetSessionRequest()
        {
            SessionID = SessionID,
            SessionData = this.Serialize()
        });
    }

    /// <summary>
    /// Clears the cart by creating a new SessionID and Session, and saving it to the existing cookie.
    /// </summary>
    public void Empty()
    {
        // Create a new SessionID
        string newGuid = HttpContext.Current.Server.UrlEncode(Guid.NewGuid().ToString());
        SessionID = newGuid;

        // Define a cart object
        CreateNewShoppingCartPropertyBag(Description, SessionID);
    }

    /// <summary>
    /// Returns the cart as an XML string
    /// </summary>
    /// <returns>string</returns>
    public override string ToString()
    {
        return HttpContext.Current.Server.HtmlEncode(this.Serialize());
    }
    #endregion

    #region Private Methods
    /// <summary>
    /// Creates a new ShoppingCart object, saves it in the Exigo Session, and returns the new ShoppingCart.
    /// </summary>
    /// <param name="description">The name of the new cart.</param>
    /// <param name="sessionID">The SessionID used to store the serialized cart in the Exigo Session.</param>
    /// <returns>ShoppingCart</returns>
    private AutoshipCartPropertyBag CreateNewShoppingCartPropertyBag(string description, string sessionID)
    {
        // Create a string that is the serialized XML of a new ShoppingCart.
        AutoshipCartPropertyBag newPropertyBag = new AutoshipCartPropertyBag();
        newPropertyBag.Description = Description;
        newPropertyBag.SessionID = sessionID;
        string serializedBasket = newPropertyBag.Serialize();

        // Save the SessionID and serialized cart in Exigo
        ExigoApiContext.CreateWebServiceContext().SetSession(new SetSessionRequest()
        {
            SessionID = sessionID,
            SessionData = serializedBasket
        });

        // Create and save our cookie. The value of the cookie should be the SessionID.
        HttpCookie cookie = new HttpCookie(Description, SessionID);
        cookie.Expires = DateTime.Now.AddDays(CookieAgeLengthInDays);
        if (!string.IsNullOrEmpty(Domain)) cookie.Domain = Domain.Trim().ToLower();
        HttpContext.Current.Response.Cookies.Add(cookie);

        // Populate the initial values into the cart from the new cart.
        PopulateInitialPropertyBagValues(newPropertyBag);

        return newPropertyBag;
    }

    /// <summary>
    /// Load an existing cart based on the cookie name provided, and create one if none is found
    /// </summary>
    /// <param name="CookieName">The name of the property bag</param>
    public void LoadPropertyBag(string CookieName)
    {
        // Save the cookie name as the description
        Description = CookieName;

        // Try to load the cookie
        HttpCookie cookie = HttpContext.Current.Request.Cookies[Description];

        // Define a cart object we will use for the rest of this constructor.
        AutoshipCartPropertyBag propertyBag = null;

        // If the cookie doesn't exist...
        if (cookie == null)
        {
            // Create a new SessionID
            string newGuid = HttpContext.Current.Server.UrlEncode(Guid.NewGuid().ToString());
            SessionID = newGuid;

            // Create the new cart
            propertyBag = CreateNewShoppingCartPropertyBag(Description, SessionID);
        }
        else
        {
            // Save the value of the cookie to the SessionID.
            SessionID = cookie.Value;

            // Get the value of the SessionID found in the cookie from Exigo
            string returnedData = ExigoApiContext.CreateWebServiceContext().GetSession(new GetSessionRequest()
            {
                SessionID = cookie.Value
            }).SessionData;

            // Check to see if there is any data there. If it is completely blank, we need to save a serialized blank cart to the Exigo Session.
            if (string.IsNullOrEmpty(returnedData))
            {
                propertyBag = CreateNewShoppingCartPropertyBag(Description, cookie.Value);
            }

            // If we got data back, let's deserialize the data and populate this object.
            else
            {
                // Deserialize the XML from the Exigo Session.
                propertyBag = Deserialize<AutoshipCartPropertyBag>(returnedData);

                // We need to check to ensure that the loaded cart is using the most current version. If not, we need to clear the loaded cart and create a new one.
                // This is to ensure that any changes made by the developer will take effect for all customers, even if they already have a cart in progress.
                // Not doing this could result in discrepencies between your latest changes and methods, and those that are already saved from a previous version.
                if (propertyBag.Version != version)
                {
                    propertyBag = CreateNewShoppingCartPropertyBag(Description, cookie.Value);
                }
                else
                {
                    // Now that we've done all our checkers for version integrity, nulled cookies and nulled data, let's go ahead and load in the variables.
                    PopulateInitialPropertyBagValues(propertyBag);
                }
            }
        }
    }

    public void LoadAutoshipIntoPropertyBag(int autoshipID)
    {
        ExistingAutoshipID = autoshipID;

        var autoship = ExigoApiContext.CreateWebServiceContext().GetAutoOrders(new GetAutoOrdersRequest
        {
            CustomerID = Identity.Current.CustomerID,
            AutoOrderStatus = AutoOrderStatusType.Active,
            AutoOrderID = ExistingAutoshipID
        }).AutoOrders[0];


        // Set the property bag values
        AutoshipDescription = autoship.Description;
        Frequency = autoship.Frequency;
        StartDate = autoship.StartDate;
        NextRunDate = autoship.NextRunDate;
        ShipMethodID = autoship.ShipMethodID;

        ShippingFirstName = autoship.FirstName;
        ShippingLastName = autoship.LastName;
        ShippingAddress1 = autoship.Address1;
        ShippingAddress2 = autoship.Address2;
        ShippingCity = autoship.City;
        ShippingState = autoship.State;
        ShippingZip = autoship.Zip;
        ShippingCountry = autoship.Country;

        switch (autoship.PaymentType)
        {
            case AutoOrderPaymentType.PrimaryCreditCard: this.PaymentType = PaymentMethodType.PrimaryCreditCard; break;
            case AutoOrderPaymentType.SecondaryCreditCard: this.PaymentType = PaymentMethodType.SecondaryCreditCard; break;
            case AutoOrderPaymentType.CheckingAccount: this.PaymentType = PaymentMethodType.BankAccountOnFile; break;
            default: throw new Exception(autoship.PaymentType + " is an unsupported autoship payment type at this time.");
        }


        var paymentTypesOnFile = ExigoApiContext.CreateWebServiceContext().GetCustomerBilling(new GetCustomerBillingRequest
        {
            CustomerID = Identity.Current.CustomerID
        });


        if (autoship.PaymentType == AutoOrderPaymentType.PrimaryCreditCard || autoship.PaymentType == AutoOrderPaymentType.SecondaryCreditCard)
        {
            CreditCardAccountResponse card = null;
            if (autoship.PaymentType == AutoOrderPaymentType.PrimaryCreditCard) card = paymentTypesOnFile.PrimaryCreditCard;
            if (autoship.PaymentType == AutoOrderPaymentType.SecondaryCreditCard) card = paymentTypesOnFile.SecondaryCreditCard;

            CreditCardNameOnCard = card.BillingName;
            CreditCardNumber = card.CreditCardNumberDisplay;
            CreditCardExpirationDate = new DateTime(card.ExpirationYear, card.ExpirationMonth, 1);
            CreditCardBillingAddress = card.BillingAddress;
            CreditCardBillingCity = card.BillingCity;
            CreditCardBillingState = card.BillingState;
            CreditCardBillingZip = card.BillingZip;
            CreditCardBillingCountry = card.BillingCountry;
        }

        if (autoship.PaymentType == AutoOrderPaymentType.CheckingAccount)
        {
            var account = paymentTypesOnFile.BankAccount;
            BankAccountNameOnAccount = account.NameOnAccount;
            BankAccountBankName = account.BankName;
            BankAccountAccountType = account.BankAccountType;
            BankAccountAccountNumber = account.BankAccountNumberDisplay;
            BankAccountRoutingNumber = account.BankRoutingNumber;
            BankAccountBankAddress = account.BillingAddress;
            BankAccountBankCity = account.BillingCity;
            BankAccountBankState = account.BillingState;
            BankAccountBankZip = account.BillingZip;
            BankAccountBankCountry = account.BillingCountry;
        }
    }

    /// <summary>
    /// Populates the initial variables and values when the cart is loaded or created.
    /// </summary>
    /// <param name="propertyBag">The AutoshipCartPropertyBag object that contains the variables you want populated.</param>
    private void PopulateInitialPropertyBagValues(AutoshipCartPropertyBag propertyBag)
    {
        // Use reflection to populate our object with the supplied version's properties
        Type type = propertyBag.GetType();
        foreach (var property in type.GetProperties())
        {
            property.SetValue(this, property.GetValue(propertyBag, null), null);
        }
    }
    #endregion

    #region Private Variables
    private int CookieAgeLengthInDays = 31;
    #endregion





    #region Properties
    public int ExistingAutoshipID { get; set; }

    public string AutoshipDescription { get; set; }
    public FrequencyType Frequency { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime NextRunDate { get; set; }

    public string ShippingFirstName { get; set; }
    public string ShippingLastName { get; set; }
    public string Email { get; set; }
    public string Phone { get; set; }

    public AddressType ShippingAddressType { get; set; }
    public string ShippingAddress1 { get; set; }
    public string ShippingAddress2 { get; set; }
    public string ShippingCity { get; set; }
    public string ShippingState { get; set; }
    public string ShippingZip { get; set; }
    public string ShippingCountry { get; set; }

    public int DropShipCustomerID { get; set; }

    public PaymentMethodType PaymentType { get; set; }

    public AccountCreditCardType CreditCardType { get; set; }
    public string CreditCardNameOnCard { get; set; }
    public string CreditCardNumber { get; set; }
    public DateTime CreditCardExpirationDate { get; set; }
    public string CreditCardCvc { get; set; }
    public string CreditCardBillingAddress { get; set; }
    public string CreditCardBillingCity { get; set; }
    public string CreditCardBillingState { get; set; }
    public string CreditCardBillingZip { get; set; }
    public string CreditCardBillingCountry { get; set; }

    public string BankAccountNameOnAccount { get; set; }
    public string BankAccountAccountNumber { get; set; }
    public string BankAccountRoutingNumber { get; set; }
    public string BankAccountBankName { get; set; }
    public BankAccountType BankAccountAccountType { get; set; }
    public string BankAccountBankAddress { get; set; }
    public string BankAccountBankCity { get; set; }
    public string BankAccountBankState { get; set; }
    public string BankAccountBankZip { get; set; }
    public string BankAccountBankCountry { get; set; }

    public int ShipMethodID { get; set; }

    public bool ReferredByEndOfCheckout { get; set; }
    public Market Market { get; set; }

    public enum AddressType
    {
        Main,
        Mailing,
        Other,
        New,
        DropShip
    }

    public enum PaymentMethodType
    {
        PrimaryCreditCard,
        SecondaryCreditCard,
        NewCreditCard,
        BankAccountOnFile,
        NewBankAccount
    }

    public enum NewCreditCardTypes
    {
        Primary,
        Secondary
    }
    #endregion
}