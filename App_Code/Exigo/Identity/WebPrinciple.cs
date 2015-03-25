using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Security.Principal;

public class WebPrincipal : IPrincipal
{
    Identity _identity;
    public WebPrincipal(Identity identity)
    {
        _identity = identity;
    }

    public IIdentity Identity
    {
        get { return _identity; }
    }

    public bool  IsInRole(string role)
    {
        return true;
    }
}