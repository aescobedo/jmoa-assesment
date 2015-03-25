using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

public static class CacheHelper
{
    public static T GetFromCache<T>(string key, int expiration = 15)
    {
        key += Identity.Current.CustomerID.ToString();
        var type = typeof(T);
        var model = HttpContext.Current.Cache[key];
        if(model == null)
        {
            model = (T)Activator.CreateInstance(typeof(T));
            HttpContext.Current.Cache.Insert(key,
                    model,
                    null,
                    DateTime.Now.AddMinutes(expiration),
                    System.Web.Caching.Cache.NoSlidingExpiration,
                    System.Web.Caching.CacheItemPriority.Normal,
                    null);
        }
        dynamic typedModel = Convert.ChangeType(model, type);
        return typedModel;
    }
}