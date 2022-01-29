using Amazon.DynamoDBv2.Model;
using SubscriptionsManager.Domain.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SubscriptionsManager.AWSTools
{
    public interface IDynamoDBManager
    {
        Task<bool> CheckingTableExistenceAsync(string tblNm);

        Task<bool> AddItem(string tableName, Dictionary<string, AttributeValue> item);

        Task<List<Subscription>> GetItemsFromSubscriptionByUserID(string tableName, string userID);
    }
}