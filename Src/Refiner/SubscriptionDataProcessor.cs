using Newtonsoft.Json;
using System;
using Newtonsoft.Json.Linq;
using Shared;

namespace Refiner
{
    public class SubscriptionDataProcessor : IDataProcessor
    {
        private readonly Envelope<string> _data;

        public SubscriptionDataProcessor()
        {}

        public JArray CleanData ()
        {
            dynamic cleanData = new JArray();
            dynamic envelopeData = JToken.Parse(_data.Payload);

            foreach (var obj in envelopeData)
            {
                dynamic entry = new JObject();
                
                entry.Reseller = new JArray();
                entry.User = new JArray();
                entry.Subscription = new JArray();
                entry.Bill = new JArray();
                
                entry.Reseller.Id = obj.resellerId;
                entry.Reseller.Name = obj.resellerName;
                entry.Reseller.Discount = obj.discount;
                
                entry.User.AccountId = obj.accountId;
                
                entry.Subscription.Name = obj.subscriptionName;
                entry.Subscription.Type = obj.type;
                entry.Subscription.State = obj.state;
                entry.Subscription.Price = obj.price;
                entry.Subscription.OnExpiry = obj.onExpiry;
                entry.Subscription.StartDate = obj.startDate;
                entry.Subscription.ExpiryDate = obj.expiryDate;

                entry.Bill.BillingDate = obj.billingDate;
                entry.DiscountedPrice = obj.discountedPrice;
                entry.CurrencyName = obj.currencyName;
                entry.CurrencyISO = obj.code;

                cleanData.Add(entry);
            }

            return cleanData;
        }

        public string Process(string data)
        {
            throw new NotImplementedException();
        }

        public void StoreProcessedData(JArray data, IDBFacade database)
        {
            string[] dataAsString = data.ToObject<string[]>();
            database.SaveCollection(data);
        }
        
    }
}