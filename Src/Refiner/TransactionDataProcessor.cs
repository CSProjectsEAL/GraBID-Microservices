using Newtonsoft.Json;
using System;
using Newtonsoft.Json.Linq;
using Shared;

namespace Refiner
{
    public class TransactionDataProcessor : DataProcessor
    {

        public dynamic ExtractDataArray(dynamic dataJObject)
        {
            return dataJObject.data;
        }
        
        public override dynamic Process(dynamic data)
        {
            JArray processedData = new JArray();

            foreach (dynamic obj in data)
            {
                dynamic entry = new JObject();
                // Clean data by means of extracting only what is needed.
                // Restructure data for ease of access and understability when creating queries
                entry.Reseller = new JObject();
                entry.User = new JObject();
                entry.Subscription = new JObject();
                entry.Bill = new JObject();

                // Correct structure of usergroups, to an enum array with user group classifiers
                if (obj.userGroup != null)
                {
                    string userGroupStr = obj.userGroup;
                    string[] userGroups = userGroupStr.Split(", ");
                    entry.UserGroups = new JArray(userGroups);
                }
                // Extracting values and placing them in the correct objects.
                entry.Reseller.Id = obj.resellerId;
                entry.Reseller.Name = obj.resellerName;
                entry.Reseller.Discount = obj.discount;

                entry.User.AccountId = obj.accountId;

                entry.Subscription.Name = obj.subscriptionName;
                entry.Subscription.Type = obj.type;
                entry.Subscription.State = obj.state;
                entry.Subscription.Price = obj.price;
                entry.Subscription.StartDate = obj.startDate;
                entry.Subscription.ExpiryDate = obj.expiryDate;

                entry.Bill.BillingDate = obj.billingDate;
                entry.Bill.DiscountedPrice = obj.discountedPrice;
                entry.Bill.CurrencyName = obj.currencyName;
                entry.Bill.CurrencyISO = obj.code;


                processedData.Add(entry);
            }
            return processedData;
        }
    }
}