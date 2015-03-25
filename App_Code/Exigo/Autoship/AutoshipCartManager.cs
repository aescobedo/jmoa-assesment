using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Exigo.WebService;

public class AutoshipCartManager
{
    public AutoshipCartManager()
    {
    }

    // Misc. Cart Settings
    public string ShoppingCartName                      = "BackofficeAutoship";

    // Page URL's
    public string UrlAutoshipList                       = "AutoshipList.aspx";
    public string UrlProductList                        = "AutoshipProductList.aspx";
    public string UrlProductDetail                      = "AutoshipProductDetail.aspx";
    public string UrlCart                               = "AutoshipCart.aspx";
    public string UrlCheckoutDetails                    = "AutoshipCheckoutDetails.aspx";
    public string UrlCheckoutShippingAddress            = "AutoshipCheckoutShippingAddress.aspx";
    public string UrlCheckoutShippingMethod             = "AutoshipCheckoutShippingMethod.aspx";
    public string UrlCheckoutPayment                    = "AutoshipCheckoutPayment.aspx";
    public string UrlCheckoutReview                     = "AutoshipCheckoutReview.aspx";
    public string UrlCheckoutComplete                   = "AutoshipCheckoutComplete.aspx";

    // Navigation Methods
    public string GetStepUrl(AutoshipManagerStep step)
    {
        var url = string.Empty;

        switch(step)
        {
            case AutoshipManagerStep.List:                          url = UrlAutoshipList; break;
            case AutoshipManagerStep.ProductList:                   url = UrlProductList; break;
            case AutoshipManagerStep.ProductDetail:                 url = UrlProductDetail; break;
            case AutoshipManagerStep.Cart:                          url = UrlCart; break;
            case AutoshipManagerStep.Details:                       url = UrlCheckoutDetails; break;
            case AutoshipManagerStep.ShippingAddress:               url = UrlCheckoutShippingAddress; break;
            case AutoshipManagerStep.ShippingMethod:                url = UrlCheckoutShippingMethod; break;
            case AutoshipManagerStep.Payment:                       url = UrlCheckoutPayment; break;
            case AutoshipManagerStep.Review:                        url = UrlCheckoutReview; break;
            case AutoshipManagerStep.Complete:                      url = UrlCheckoutComplete; break;
        }

        return url;
    }

    // Configuration Objects
    #region Payment Method Settings
    public class InternationalPaymentSettings : IPaymentConfiguration
    {
        public List<Exigo.WebService.PaymentType> AvailablePaymentMethodTypes
        {
            get
            {
                return new List<Exigo.WebService.PaymentType>()
                {
                    Exigo.WebService.PaymentType.CreditCard
                };
            }
        }
        public Exigo.WebService.PaymentType DefaultPaymentMethodType
        {
            get { return Exigo.WebService.PaymentType.CreditCard; }
        }
    }
    public class LocalPaymentSettings : IPaymentConfiguration
    {
        public List<Exigo.WebService.PaymentType> AvailablePaymentMethodTypes
        {
            get
            {
                return new List<Exigo.WebService.PaymentType>()
                {
                    Exigo.WebService.PaymentType.CreditCard,
                    Exigo.WebService.PaymentType.ACHDebit
                };
            }
        }
        public Exigo.WebService.PaymentType DefaultPaymentMethodType
        {
            get { return Exigo.WebService.PaymentType.CreditCard; }
        }
    }
    #endregion

    // Configuration Objects
    public ShoppingCart Cart
    {
        get
        {
            if (_cart == null)
            {
                _cart = new ShoppingCart(ShoppingCartName + "AutoshipManager", Identity.Current.CustomerID)
                {
                    CustomerID = Identity.Current.CustomerID,
                    Country = Identity.Current.Address.Country,
                    Region = Identity.Current.Address.State
                };
            }
            return _cart;
        }
    }
    private ShoppingCart _cart;

    public AutoshipCartPropertyBag PropertyBag
    {
        get
            {
                if (_propertyBag == null)
                {
                    _propertyBag = new AutoshipCartPropertyBag(ShoppingCartName + "AutoshipManagerPropertyBag");
                    if(_propertyBag.Market == null) { 
                        _propertyBag.Market = Identity.Current.Market;
                        _propertyBag.Save();
                    }
                }
                return _propertyBag;
            }
        }
    private AutoshipCartPropertyBag _propertyBag;

    public IAutoOrderConfiguration Configuration
    {
        get
        {
            if (_configuration == null)
            {
                switch(Identity.Current.Market.Name)
                {
                    case MarketName.UnitedStates:
                    default:                            _configuration = new USAutoshipConfiguration(); break;
                }
            }
            return _configuration;
        }
    }
    private IAutoOrderConfiguration _configuration;

    public IAddressConfiguration AddressSettings
    {
        get
        {
            if (_addressSettings == null)
            {
                switch(Identity.Current.Market.Name)
                {
                    case MarketName.UnitedStates:
                    default:                            _addressSettings = new USAddressCongifuration(); break;
                }
            }
            return _addressSettings;
        }
    }
    private IAddressConfiguration _addressSettings;

    public IAutoOrderSettingsConfiguration AutoshipSettings
    {
        get
        {
            if (_autoshipSettings == null)
            {
                switch(PropertyBag.Market.Name)
                {
                    case MarketName.UnitedStates:
                    default:                            _autoshipSettings = new USAutoshipSettingsConfiguration(); break;
                }
            }
            return _autoshipSettings;
        }
    }
    private IAutoOrderSettingsConfiguration _autoshipSettings;

    public IPaymentConfiguration Payments
    {
        get
        {
            if(_payments == null)
            {
                if(PropertyBag.Market.Name == MarketName.UnitedStates) _payments = new LocalPaymentSettings();
                else _payments = new InternationalPaymentSettings();
            }
            return _payments;
        }
    }
    private IPaymentConfiguration _payments;

    // Public Methods
    public void Reset()
    {
        Cart.Empty();
        PropertyBag.Empty();
    }
}

public enum AutoshipManagerStep
{
    List,
    ProductList,
    ProductDetail,
    Cart,
    Details,
    ShippingAddress,
    ShippingMethod,
    Payment,
    Review,
    Complete
}