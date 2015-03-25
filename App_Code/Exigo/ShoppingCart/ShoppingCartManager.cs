using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

public class ShoppingCartManager
{
    public ShoppingCartManager()
    {
        if(Identity.Current.Market.Name != PropertyBag.Market.Name)
        {
            Reset();
            HttpContext.Current.Response.Redirect(GetStepUrl(ShoppingStep.ProductList));
        }
    }

    // Misc. Cart Settings
    public string ShoppingCartName                      = "BackofficeOrder";

    // Page URL's
    public string UrlProductList                        = "ShoppingProductList.aspx";
    public string UrlProductDetail                      = "ShoppingProductDetail.aspx";
    public string UrlCart                               = "ShoppingCart.aspx";
    public string UrlCheckoutShippingAddress            = "ShoppingCheckoutShippingAddress.aspx";
    public string UrlCheckoutShippingMethod             = "ShoppingCheckoutShippingMethod.aspx";
    public string UrlCheckoutPayment                    = "ShoppingCheckoutPayment.aspx";
    public string UrlCheckoutReview                     = "ShoppingCheckoutReview.aspx";
    public string UrlCheckoutComplete                   = "ShoppingCheckoutComplete.aspx";

    // Navigation Methods
    public string GetStepUrl(ShoppingStep step)
    {
        var url = string.Empty;

        switch(step)
        {
            case ShoppingStep.ProductList:                      url = UrlProductList; break;
            case ShoppingStep.ProductDetail:                    url = UrlProductDetail; break;
            case ShoppingStep.Cart:                             url = UrlCart; break;
            case ShoppingStep.ShippingAddress:                  url = UrlCheckoutShippingAddress; break;
            case ShoppingStep.ShippingMethod:                   url = UrlCheckoutShippingMethod; break;
            case ShoppingStep.Payment:                          url = UrlCheckoutPayment; break;
            case ShoppingStep.Review:                           url = UrlCheckoutReview; break;
            case ShoppingStep.Complete:                         url = UrlCheckoutComplete; break;
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
                _cart = new ShoppingCart(ShoppingCartName + "ShoppingCart", Identity.Current.CustomerID)
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

    public ShoppingCartPropertyBag PropertyBag
    {
        get
        {
            if (_propertyBag == null)
            {
                _propertyBag = new ShoppingCartPropertyBag(ShoppingCartName + "ShoppingCartPropertyBag");
                if(_propertyBag.Market == null) { 
                    _propertyBag.Market = Identity.Current.Market;
                    _propertyBag.Save();
                }
            }
            return _propertyBag;
        }
    }
    private ShoppingCartPropertyBag _propertyBag;

    public IOrderConfiguration Configuration
    {
        get
        {
            if (_configuration == null)
            {
                switch(Identity.Current.Market.Name)
                {
                    case MarketName.UnitedStates:
                    default:                            _configuration = new USInitialOrderConfiguration(); break;
                }
            }
            return _configuration;
        }
    }
    private IOrderConfiguration _configuration;

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

    public IAutoOrderSettingsConfiguration Autoship
    {
        get
        {
            if (_autoship == null)
            {
                switch(PropertyBag.Market.Name)
                {
                    case MarketName.UnitedStates:
                    default:                            _autoship = new USAutoshipSettingsConfiguration(); break;
                }
            }
            return _autoship;
        }
    }
    private IAutoOrderSettingsConfiguration _autoship;

    // Public Methods
    public void Reset()
    {
        Cart.Empty();
        PropertyBag.Empty();
    }
}

public enum ShoppingStep
{
    ProductList,
    ProductDetail,
    Cart,
    ShippingAddress,
    ShippingMethod,
    Payment,
    Review,
    Complete
}