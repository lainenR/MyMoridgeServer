using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Braintree;

namespace MyMoridgeServer.BusinessLogic
{
    public interface IBraintreeConfiguration
    {
        IBraintreeGateway CreateGateway();
        IBraintreeGateway GetGateway();
    }
}
