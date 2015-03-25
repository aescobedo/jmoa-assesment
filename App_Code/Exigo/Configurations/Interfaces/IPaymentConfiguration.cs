using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Exigo.WebService;

public interface IPaymentConfiguration
{
	List<PaymentType> AvailablePaymentMethodTypes { get; }
    PaymentType DefaultPaymentMethodType { get; }
}