using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Text;
using System.IO;
using System.Xml.Serialization;
using Exigo.WebService;

/// <summary>
/// An object used in Exigo custom shopping carts to hold items selected by the customer for purchase.
/// </summary>
[Serializable]
public class ShoppingCart
{
    /// <summary>
    /// The version of the current Exigo ShoppingCart object. This value is privately set.
    /// </summary>
    private string version = "2.2.1";

    #region Constructors
    // Creating a new cart from scratch
    public ShoppingCart()
    {
        Version = version;
        CreatedDate = DateTime.Now;
        Items = new ShoppingCartItemsCollection();

        CustomerID = 0;
        Country = string.Empty;
        Region = string.Empty;
        Domain = string.Empty;
    }

    // Load an existing cart based on the cookie name provided, and create one if none is found
    public ShoppingCart(string CookieName)
    {
        LoadShoppingCart(CookieName, 0);
    }

    // Load an existing cart based on the cookie name and customer ID provided, and create one if none is found
    public ShoppingCart(string CookieName, int customerID)
    {
        LoadShoppingCart(CookieName, customerID);
    }
    #endregion

    #region Properties
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
    /// The country that is associated with the current ShoppingCart object.
    /// </summary>
    public string Country { get; set; }

    /// <summary>
    /// The state/region that is associated with the current ShoppingCart object.
    /// </summary>
    public string Region { get; set; }

    /// <summary>
    /// The domain that is associated with the current ShoppingCart object. If the Domain is left empty, the cookie that stores the shopping cart will not be restricted by domain.
    /// </summary>
    public string Domain { get; set; }

    /// <summary>
    /// The date the current ShoppingCart object was created.
    /// </summary>
    public DateTime CreatedDate { get; set; }

    /// <summary>
    /// A list of all of the items found in the current ShoppingCart object.
    /// </summary>
    public ShoppingCartItemsCollection Items { get; set; }

    /// <summary>
    /// The ID of the current autoship being edited or changed. If the AutoshipID is 0, then the current ShoppingCart object is being used to create a new autoship.
    /// </summary>
    public int AutoshipID { get; set; }
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
        CreateNewShoppingCart(Description, SessionID);
    }

    /// <summary>
    /// Returns the cart as an XML string
    /// </summary>
    /// <returns>string</returns>
    public override string ToString()
    {
        return HttpContext.Current.Server.HtmlEncode(this.Serialize());
    }

    /// <summary>
    /// Parse the Request.Form object to find all items in the current page, find all items requested to be added by the user, add/update the items in the cart and save the cart.
    /// </summary>
    public void AddItemsToBasket(bool replaceExistingItems)
    {
        int i = 0;
        string itemCode = GetRequestFormVariable("ItemCode", i);

        while (!string.IsNullOrEmpty(itemCode))
        {
            if (!string.IsNullOrEmpty(GetRequestFormVariable("Quantity", i)))
            {
                int quantity;
                if (int.TryParse(GetRequestFormVariable("Quantity", i), out quantity))
                {
                    string parentItemCode = GetRequestFormVariable("ParentItemCode", i);
                    string dynamicKitCategory = GetRequestFormVariable("DynamicKitCategory", i);
                    ShoppingCartItemType type = (ShoppingCartItemType)Enum.Parse(typeof(ShoppingCartItemType), GetRequestFormVariable("Type", i));

                    // If we have a valid quantity, let's start handling this item
                    if (quantity > 0)
                    {
                        // If we have quantity bigger than 0, and we are replacing items, we need to delete the item first.
                        // We find all items that match the item exactly, and delete it if found.
                        if (replaceExistingItems)
                        {
                            this.Items.ForEach(itemToBeDeleted =>
                            {
                                if (itemToBeDeleted.ItemCode == itemCode
                                    && itemToBeDeleted.Type == type
                                    && itemToBeDeleted.ParentItemCode == parentItemCode
                                    && itemToBeDeleted.DynamicKitCategory == dynamicKitCategory)
                                {
                                    this.Items.Remove(itemToBeDeleted);
                                }
                            });
                        }

                        // Now, we add the item.
                        this.Items.Add(new ShoppingCartItem()
                        {
                            ItemCode = itemCode,
                            Quantity = quantity,
                            ParentItemCode = parentItemCode,
                            DynamicKitCategory = dynamicKitCategory,
                            Type = type
                        });
                    }

                    // If we have a quantity of 0, let's go ahead and remove the item from the cart.
                    else
                    {
                        if (replaceExistingItems)
                        {
                            this.Items.ForEach(itemToBeDeleted =>
                            {
                                if (itemToBeDeleted.ItemCode == itemCode
                                    && itemToBeDeleted.Type == type
                                    && itemToBeDeleted.ParentItemCode == parentItemCode
                                    && itemToBeDeleted.DynamicKitCategory == dynamicKitCategory)
                                {
                                    this.Items.Remove(itemToBeDeleted);
                                }
                            });
                        }
                    }
                }
            }

            i++;
            itemCode = GetRequestFormVariable("ItemCode", i);
        }
        this.Save();
    }

    /// <summary>
    /// Returns a form field ID designed to be comsumed by this cart.
    /// </summary>
    /// <param name="formField">The core name of the Request.Form field name.</param>
    /// <returns></returns>
    public string GetFormFieldID(string formField)
    {
        return GetFormFieldID(formField, 0);
    }

    /// <summary>
    /// Returns a form field ID designed to be comsumed by this cart.
    /// </summary>
    /// <param name="formField">The core name of the Request.Form field name.</param>
    /// <param name="fieldCounter">he counter used when iterating through the Request.Form object.</param>
    /// <returns></returns>
    public string GetFormFieldID(string formField, int fieldCounter)
    {
        return Description + "_" + formField + "_" + fieldCounter;
    }

    /// <summary>
    /// Loads an existing autoship into the current cart object.
    /// </summary>
    /// <param name="autoshipID">The ID of the existing autoship.</param>
    public void LoadAutoship(int autoshipID, int customerID)
    {
        // Clear the current items
        this.Items.Clear();

        // Set the autoship ID for the cart
        this.AutoshipID = autoshipID;

        // Get the autoship from the API
        var autoship = ExigoApiContext.CreateWebServiceContext().GetAutoOrders(new GetAutoOrdersRequest
        {
            AutoOrderID = autoshipID,
            CustomerID = customerID
        }).AutoOrders[0];

        // Fill in the cart with the items from the autoship
        foreach (var detail in autoship.Details)
        {
            if (!string.IsNullOrEmpty(detail.ParentItemCode))
            {
                this.Items.Add(new ShoppingCartItem
                {
                    ItemCode = detail.ItemCode,
                    Quantity = detail.Quantity,
                    ParentItemCode = detail.ParentItemCode,
                    Type = ShoppingCartItemType.Default,
                    DynamicKitCategory = "NONE"
                });
            }
            else
            {
                this.Items.Add(new ShoppingCartItem
                {
                    ItemCode = detail.ItemCode,
                    Quantity = detail.Quantity,
                    ParentItemCode = string.Empty,
                    Type = ShoppingCartItemType.Default
                });
            }
        }

        // Save the cart
        this.Save();
    }
    #endregion

    #region Private Methods
    /// <summary>
    /// Creates a new ShoppingCart object, saves it in the Exigo Session, and returns the new ShoppingCart.
    /// </summary>
    /// <param name="description">The name of the new cart.</param>
    /// <param name="sessionID">The SessionID used to store the serialized cart in the Exigo Session.</param>
    /// <returns>ShoppingCart</returns>
    private ShoppingCart CreateNewShoppingCart(string description, string sessionID)
    {
        // Create a string that is the serialized XML of a new ShoppingCart.
        ShoppingCart newCart = new ShoppingCart();
        newCart.Description = Description;
        newCart.SessionID = sessionID;
        newCart.Items = new ShoppingCartItemsCollection();
        string serializedBasket = newCart.Serialize();

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
        PopulateInitialCartValues(newCart);

        return newCart;
    }

    /// <summary>
    /// Load an existing ShoppingCart given the ShoppingCart's name and an optional customer ID
    /// </summary>
    /// <param name="CookieName">The name of the shopping cart</param>
    /// <param name="customerID">(Optional) The customer ID the shopping cart belongs to. Pass 0 if the cart does not belong to an existing customer.</param>
    private void LoadShoppingCart(string CookieName, int customerID)
    {
        // Save the cookie name as the description
        Description = CookieName;

        // Try to load the cookie
        HttpCookie cookie = HttpContext.Current.Request.Cookies[Description];

        // Define a cart object we will use for the rest of this constructor.
        ShoppingCart cart = null;

        // If the cookie doesn't exist...
        if (cookie == null)
        {
            // Create a new SessionID
            string newGuid = HttpContext.Current.Server.UrlEncode(Guid.NewGuid().ToString());
            SessionID = newGuid;

            // Create the new cart
            cart = CreateNewShoppingCart(Description, SessionID);
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
                cart = CreateNewShoppingCart(Description, cookie.Value);
            }

            // If we got data back, let's deserialize the data and populate this object.
            else
            {
                // Deserialize the XML from the Exigo Session.
                cart = Deserialize<ShoppingCart>(returnedData);

                // We need to check to ensure that the loaded cart is using the most current version. If not, we need to clear the loaded cart and create a new one.
                // This is to ensure that any changes made by the developer will take effect for all customers, even if they already have a cart in progress.
                // Not doing this could result in discrepencies between your latest changes and methods, and those that are already saved from a previous version.
                if (cart.Version != version || cart.CustomerID != customerID)
                {
                    cart = CreateNewShoppingCart(Description, cookie.Value);
                }
                else
                {
                    // Now that we've done all our checkers for version integrity, nulled cookies and nulled data, let's go ahead and load in the variables.
                    PopulateInitialCartValues(cart);
                }
            }
        }
    }

    /// <summary>
    /// Pulls from the Request.Form by formatting the request in a consistent way.
    /// </summary>
    /// <param name="formField">The core name of the Request.Form field name.</param>
    /// <param name="fieldCounter">The counter used when iterating through the Request.Form object.</param>
    /// <returns></returns>
    private string GetRequestFormVariable(string formField, int fieldCounter)
    {
        if (!string.IsNullOrEmpty(HttpContext.Current.Request.Form[GetFormFieldID(formField, fieldCounter)]))
        {
            return HttpContext.Current.Request.Form[GetFormFieldID(formField, fieldCounter)];
        }
        else
        {
            return string.Empty;
        }
    }

    /// <summary>
    /// Populates the initial variables and values when the cart is loaded or created.
    /// </summary>
    /// <param name="cart">The ShoppingCart object that contains the variables you want populated.</param>
    private void PopulateInitialCartValues(ShoppingCart cart)
    {
        this.Version = cart.Version;
        this.Description = cart.Description;
        this.CreatedDate = cart.CreatedDate;
        this.SessionID = cart.SessionID;
        this.AutoshipID = cart.AutoshipID;
        this.CustomerID = cart.CustomerID;
        this.Domain = cart.Domain;
        this.Country = cart.Country;
        this.Region = cart.Region;

        // Finally, load the items
        this.Items = cart.Items;
    }
    #endregion

    #region Private Variables
    private int CookieAgeLengthInDays = 31;
    #endregion
}

/// <summary>
/// An item found in an Exigo ShoppingCart object. Items are designed to be stored in an ShoppingBasketItemsCollection. 
/// </summary>
[Serializable]
public class ShoppingCartItem
{
    public ShoppingCartItem()
    {
        ItemCode = string.Empty;
        Quantity = 0;
        ParentItemCode = string.Empty;
        DynamicKitCategory = string.Empty;
        Type = ShoppingCartItemType.Default;
    }

    /// <summary>
    /// Creates a ShoppingCartItem given parameters used in a product page or product list that contains dynamically rendered items on the page using a unique item counter integer.
    /// </summary>
    /// <param name="cart">The shopping cart context.</param>
    /// <param name="itemCounter">The unique item counter to use when grabbing the item from the Request.Form object.</param>
    public ShoppingCartItem(ShoppingCart cart, int itemCounter)
    {
        ItemCode = HttpContext.Current.Request.Form[cart.GetFormFieldID("ItemCode", itemCounter)];
        Quantity = Convert.ToDecimal(HttpContext.Current.Request.Form[cart.GetFormFieldID("Quantity", itemCounter)]);
        ParentItemCode = HttpContext.Current.Request.Form[cart.GetFormFieldID("ParentItemCode", itemCounter)];
        Type = (ShoppingCartItemType)Enum.Parse(typeof(ShoppingCartItemType), HttpContext.Current.Request.Form[cart.GetFormFieldID("Type", itemCounter)]);
        DynamicKitCategory = HttpContext.Current.Request.Form[cart.GetFormFieldID("DynamicKitCategory", itemCounter)];
    }

    /// <summary>
    /// The item code of the new ShoppingCartItem.
    /// </summary>
    public string ItemCode { get; set; }

    /// <summary>
    /// The quantity of the new ShoppingCartItem requested by the customer.
    /// </summary>
    public decimal Quantity { get; set; }

    /// <summary>
    /// This new ShoppingCartItem's master kit item code. When defined, this ShoppingCartItem will act as a kit member of the supplied parent item code, and the item will be added to the order without cost. Note that the parent item code does not have to be setup in Exigo as a kit.
    /// </summary>
    public string ParentItemCode { get; set; }

    /// <summary>
    /// The category of the dynamic kit it is found in. This is used to ensure that if two Items with the same item code added from two different dynamic kits will not conflict.
    /// </summary>
    public string DynamicKitCategory { get; set; }

    /// <summary>
    /// The type of the new ShoppingCartItem with respect to your cart feature. This should be used to distinguish an item by purpose or other distinction.
    /// </summary>
    public ShoppingCartItemType Type { get; set; }

    /// <summary>
    /// A custom value stored for this item.
    /// </summary>
    public override string ToString()
    {
        return string.Format("{0}|{1}", ItemCode, Quantity);
    }

    /// <summary>
    /// Clone an ShoppingCartItem and get an exact independent copy.
    /// </summary>
    /// <returns>ShoppingCartItem</returns>
    public ShoppingCartItem Clone()
    {
        return Deserialize<ShoppingCartItem>(this.Serialize());
    }

    /// <summary>
    /// Determines if the item is part of a dynamic kit by checking the value of the Parent ShoppingCartItem Code. If the parent item code is not empty, it is a dynamic kit.
    /// </summary>
    public bool IsDynamicKitMember
    {
        get
        {
            return (!string.IsNullOrEmpty(this.ParentItemCode));
        }
    }

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
}

/// <summary>
/// A collection of items found in an Exigo ShoppingCart. This collection is inherited from List&lt;ShoppingCartItem&gt;.
/// </summary>
[Serializable]
public class ShoppingCartItemsCollection : List<ShoppingCartItem>
{
    /// <summary>
    /// Adds the provided item to the ShoppingBasketItemsCollection, or adds the quantity of the provided item to any pre-existing item found in the ShoppingBasketItemsCollection that matches the provided item code and type.
    /// </summary>
    /// <param name="item">The item you want to add to the ShoppingBasketItemsCollection.</param>
    public void Add(ShoppingCartItem newItem)
    {
        // Get a list of all items that have the same item code and type.
        List<ShoppingCartItem> preExistingItems = this.FindAll(item => item.ItemCode.Equals(newItem.ItemCode, StringComparison.OrdinalIgnoreCase)
            && item.Type.Equals(newItem.Type)
            && item.DynamicKitCategory.Equals(newItem.DynamicKitCategory, StringComparison.OrdinalIgnoreCase)
            && item.ParentItemCode.Equals(newItem.ParentItemCode, StringComparison.OrdinalIgnoreCase));

        // If we returned any existing items that match the item code and type, we need to add to those existing items.
        if (preExistingItems.Count() > 0)
        {
            // Loop through each item found.
            preExistingItems.ForEach(item =>
            {
                // Add the new quantity to the existing item code.
                // Note that the only thing we are adding to the existing item code is the new quantity.
                item.Quantity = item.Quantity + newItem.Quantity;
            });
        }

        // If we didn't find any existing items matching the item code or type, let's add it to the ShoppingBasketItemsCollection
        else
        {
            base.Add(newItem);
        }
    }

    /// <summary>
    /// Removes any item from the ShoppingBasketItemsCollection that matches the provided item code.
    /// </summary>
    /// <param name="itemCode">The item code you want removed from the ShoppingBasketItemsCollection.</param>
    public void Remove(string itemCode)
    {
        this.FindAll(item => item.ItemCode.Equals(itemCode, StringComparison.OrdinalIgnoreCase)).ForEach(item =>
        {
            base.Remove(item);
        });

        DeleteOrphanDynamicKitMembers();
    }

    /// <summary>
    /// Removes any item from the ShoppingBasketItemsCollection that matches the provided item code and dynamic kit category.
    /// </summary>
    /// <param name="itemCode">The item code you want removed from the ShoppingBasketItemsCollection.</param>
    /// <param name="dynamicKitCategory">The dynamic kit category of items you want removed from the ShoppingBasketItemsCollection.</param>
    public void Remove(string itemCode, string dynamicKitCategory)
    {
        this.FindAll(item => item.ItemCode.Equals(itemCode, StringComparison.OrdinalIgnoreCase)
            && item.DynamicKitCategory.Equals(dynamicKitCategory, StringComparison.OrdinalIgnoreCase)).ForEach(item =>
            {
                base.Remove(item);
            });

        DeleteOrphanDynamicKitMembers();
    }

    /// <summary>
    /// Removes any item from the ShoppingBasketItemsCollection that matches the provided item code and type.
    /// </summary>
    /// <param name="itemCode">The item code you want removed from the ShoppingBasketItemsCollection.</param>
    /// <param name="type">The type of item you want removed from the ShoppingBasketItemsCollection.</param>
    public void Remove(string itemCode, ShoppingCartItemType type)
    {
        this.FindAll(item => item.ItemCode.Equals(itemCode, StringComparison.OrdinalIgnoreCase) && item.Type.Equals(type)).ForEach(item =>
        {
            base.Remove(item);
        });

        DeleteOrphanDynamicKitMembers();
    }

    /// <summary>
    /// Removes any item from the ShoppingBasketItemsCollection that matches the provided item code, type and dynamic kit category.
    /// </summary>
    /// <param name="itemCode">The item code you want removed from the ShoppingBasketItemsCollection.</param>
    /// <param name="type">The type of item you want removed from the ShoppingBasketItemsCollection.</param>
    /// <param name="dynamicKitCategory">The dynamic kit category of items you want removed from the ShoppingBasketItemsCollection.</param>
    public void Remove(string itemCode, ShoppingCartItemType type, string dynamicKitCategory)
    {
        this.FindAll(item =>
            item.ItemCode.Equals(itemCode, StringComparison.OrdinalIgnoreCase)
            && item.Type.Equals(type)
            && item.DynamicKitCategory.Equals(dynamicKitCategory, StringComparison.OrdinalIgnoreCase))
            .ForEach(item =>
            {
                base.Remove(item);
            });

        DeleteOrphanDynamicKitMembers();
    }

    /// <summary>
    /// Removes any item from the ShoppingBasketItemsCollection that matches the provided type.
    /// </summary>
    /// <param name="type">The type of items you want removed from the ShoppingBasketItemsCollection.</param>
    public void Remove(ShoppingCartItemType type)
    {
        this.FindAll(item => item.Type.Equals(type)).ForEach(item =>
        {
            base.Remove(item);
        });

        DeleteOrphanDynamicKitMembers();
    }

    /// <summary>
    /// Removes any item from the ShoppingBasketItemsCollection that matches the provided type and dynamic kit category.
    /// </summary>
    /// <param name="type">The type of items you want removed from the ShoppingBasketItemsCollection.</param>
    /// <param name="dynamicKitCategory">The dynamic kit category of items you want removed from the ShoppingBasketItemsCollection.</param>
    public void Remove(ShoppingCartItemType type, string dynamicKitCategory)
    {
        this.FindAll(item => item.Type.Equals(type) && item.DynamicKitCategory.Equals(dynamicKitCategory, StringComparison.OrdinalIgnoreCase)).ForEach(item =>
        {
            base.Remove(item);
        });

        DeleteOrphanDynamicKitMembers();
    }

    /// <summary>
    /// A method used to ensure that there are no dynamic kit members that don't have a dynamic kit master in the cart.
    /// </summary>
    public void DeleteOrphanDynamicKitMembers()
    {
        List<ShoppingCartItem> itemsToDelete = new List<ShoppingCartItem>();
        foreach (var item in this)
        {
            if (item.IsDynamicKitMember)
            {
                var existingParents = this.FindAll(i => i.ItemCode == item.ParentItemCode && !i.IsDynamicKitMember && i.Type == item.Type);
                if (existingParents.Count == 0)
                {
                    itemsToDelete.Add(item);
                }
            }
        }

        itemsToDelete.ForEach(x => base.Remove(x));
    }
}

/// <summary>
/// The type of the item with respect to your cart feature. This should be used to distinguish an item by purpose or other distinction.
/// </summary>
[Serializable]
public enum ShoppingCartItemType
{
    Default,
    Autoship
}
