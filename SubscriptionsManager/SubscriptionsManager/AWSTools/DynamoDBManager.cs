using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DocumentModel;
using Amazon.DynamoDBv2.Model;
using SubscriptionsManager.Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Threading.Tasks;

namespace SubscriptionsManager.AWSTools
{
    public class DynamoDBManager : IDynamoDBManager
    {
        private AmazonDynamoDBClient Client;

        public virtual bool CreateClient()
        {
            try
            {
                Client = new AmazonDynamoDBClient();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public virtual async Task<bool> CheckingTableExistenceAsync(string tblNm)
        {
            var response = await Client.ListTablesAsync();
            return response.TableNames.Contains(tblNm);
        }

        public virtual async Task<bool> AddItem(string tableName, Dictionary<string, AttributeValue> item)
        {
            var request = new PutItemRequest
            {
                TableName = tableName,
                Item = item
            };

            try
            {
                await Client.PutItemAsync(request);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public virtual async Task<List<Subscription>> GetItemsFromSubscriptionByUserID(string tableName, string userID)
        {
            List<Subscription> subscriptions = new List<Subscription>();

            QueryRequest queryRequest = new QueryRequest
            {
                TableName = tableName,
                IndexName = "UserID-index",
                KeyConditionExpression = "UserID = :userID",
                ExpressionAttributeValues = new Dictionary<string, AttributeValue> {
                    {":userID", new AttributeValue { S =  userID }}
                },
                ScanIndexForward = true
            };

            var request = new QueryRequest
            {
                TableName = tableName,
                KeyConditionExpression = "UserID = :v_Id",
                ExpressionAttributeValues = new Dictionary<string, AttributeValue> {
                {":v_Id", new AttributeValue { S =  userID }}}
            };

            var response = await Client.QueryAsync(queryRequest);

            foreach (Dictionary<string, AttributeValue> item in response.Items)
            {
                Subscription subscription = BuildSubscriptionFromItem(item);
                subscriptions.Add(subscription);
            }

            return subscriptions;
        }

        private Subscription BuildSubscriptionFromItem(Dictionary<string, AttributeValue> attributeList)
        {
            string userid = "";
            string title = "";
            string id = "";
            decimal price = 0m;

            foreach (KeyValuePair<string, AttributeValue> kvp in attributeList)
            {
                string attributeName = kvp.Key;
                AttributeValue value = kvp.Value;

                if (attributeName.Equals(nameof(Subscription.Title)))
                {
                    title = value.S;
                }
                else if (attributeName.Equals(nameof(Subscription.ID)))
                {
                    id = value.S;
                }
                else if (attributeName.Equals(nameof(Subscription.Price)))
                {
                    price = Convert.ToDecimal(value.S);
                }
                else if (attributeName.Equals(nameof(Subscription.UserID)))
                {
                    userid = value.S;
                }
            }

            return new Subscription
            {
                ID = id,
                Price = price,
                Title = title,
                UserID = userid
            };
        }
    }
}
