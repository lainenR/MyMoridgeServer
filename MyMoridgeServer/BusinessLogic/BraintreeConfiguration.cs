using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Braintree;

namespace MyMoridgeServer.BusinessLogic
{
    public class BraintreeConfiguration : IBraintreeConfiguration
    {
        public string Environment { get; set; }
        public string MerchantId { get; set; }
        public string PublicKey { get; set; }
        public string PrivateKey { get; set; }
        private IBraintreeGateway BraintreeGateway { get; set; }

        public IBraintreeGateway CreateGateway()
        {
            if (MerchantId == null || PublicKey == null || PrivateKey == null)
            {
                Environment = Common.GetAppConfigValue("BraintreeEnvironment");
                MerchantId = Common.GetAppConfigValue("BraintreeMerchantId");
                PublicKey = Common.GetAppConfigValue("BraintreePublicKey");
                PrivateKey = Common.GetAppConfigValue("BraintreePrivateKey");
            }

            return new BraintreeGateway(Environment, MerchantId, PublicKey, PrivateKey);
        }

        public IBraintreeGateway GetGateway()
        {
            if (BraintreeGateway == null)
            {
                BraintreeGateway = CreateGateway();
            }

            return BraintreeGateway;
        }
    }
}