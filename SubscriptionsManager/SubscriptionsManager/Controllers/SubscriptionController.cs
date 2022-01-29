using Amazon.DynamoDBv2.Model;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using SubscriptionsManager.AWSTools;
using SubscriptionsManager.Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SubscriptionsManager.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class SubscriptionController : ControllerBase
    {
        private readonly ILogger<SubscriptionController> _logger;
        private readonly string _subscriptionTableName = "Subscription";
        private readonly DynamoDBManager _dynamoDBManager;

        public SubscriptionController(ILogger<SubscriptionController> logger, DynamoDBManager dynamoDBManager)
        {
            _logger = logger;
            _dynamoDBManager = dynamoDBManager;
        }

        /// <summary>
        /// Adds a subscription to AWS Dynamo DB Table named Subscription
        /// </summary>
        /// <param name="subscription"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<ActionResult> CreateSubscriptionAsync(Subscription subscription)
        {
            _logger.LogInformation($"{DateTime.Now.ToString("yyyyMMddHHmmss")} CreateSubscription called with subscription id {subscription.ID}");

            bool clientCreatedSuccessfully = _dynamoDBManager.CreateClient();

            if (clientCreatedSuccessfully)
            {
                bool subscriptionTableExists = await _dynamoDBManager.CheckingTableExistenceAsync(_subscriptionTableName);
                if (subscriptionTableExists)
                {
                    Dictionary<string, AttributeValue> subscriptionItem = BuildSubscriptionItem(subscription);
                    bool itemAddedSucessfully = await _dynamoDBManager.AddItem(_subscriptionTableName , subscriptionItem);
                    return itemAddedSucessfully ? Ok($"Added subscription with id {subscription.ID}") : StatusCode(400, "Item could not be added");
                }
                else
                {
                    return StatusCode(400, "Subscription table not found");
                }
            }
            else
            {
                return StatusCode(500, "There was a problem while creating a client!");
            }
        }

        /// <summary>
        /// Gets all subscription from DynamoDB Table Subscription for the given userID
        /// </summary>
        /// <param name="userID"></param>
        /// <returns></returns>
        [HttpGet("GetSubscriptionsFromUser")]
        public async Task<ActionResult> GetSubscriptionsFromUserAsync([FromQuery]string userID)
        {
            _logger.LogInformation($"{DateTime.Now.ToString("yyyyMMddHHmmss")} CreateSubscription called with userID ");

            bool clientCreatedSuccessfully = _dynamoDBManager.CreateClient();

            if (clientCreatedSuccessfully)
            {
                bool subscriptionTableExists = await _dynamoDBManager.CheckingTableExistenceAsync(_subscriptionTableName);
                if (subscriptionTableExists)
                {
                    List<Subscription> subscriptionsFromUser = await _dynamoDBManager.GetItemsFromSubscriptionByUserID(_subscriptionTableName, userID);
                    return Ok(subscriptionsFromUser);
                }
                else
                {
                    return StatusCode(400, "Subscription table not found");
                }
            }
            else
            {
                return StatusCode(500, "There was a problem while creating a client!");
            }
        }


        /// <summary>
        /// Builds a subscriptionItem from a subscription
        /// </summary>
        /// <param name="subscription"></param>
        /// <returns></returns>
        private Dictionary<string, AttributeValue> BuildSubscriptionItem(Subscription subscription)
        {
            return new Dictionary<string, AttributeValue>()
            {
                { "ID", new AttributeValue {S = subscription.ID}},
                { "Title", new AttributeValue { S = subscription.Title }},
                { "Price", new AttributeValue { S = subscription.Price.ToString() }},
                { "UserID", new AttributeValue { S = subscription.UserID }}
            };
        }
    }
}
