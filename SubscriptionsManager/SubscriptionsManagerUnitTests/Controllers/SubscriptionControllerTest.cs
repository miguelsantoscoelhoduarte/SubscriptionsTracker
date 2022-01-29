using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.ComponentModel.DataAnnotations;
using SubscriptionsManager.Controllers;
using Microsoft.Extensions.Logging;
using Moq;
using SubscriptionsManager.Domain.Models;
using SubscriptionsManagerUnitTests.Controllers;
using SubscriptionsManager.AWSTools;
using Amazon.DynamoDBv2.Model;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace DomainUnitTests
{
    [TestClass]
    public class SubscriptionControllerTest
    {
        private SubscriptionController _subscriptionController;
        private Mock<ILogger<SubscriptionController>> _loggerMock;
        private Mock<DynamoDBManager> _dynamoDBManagerMock;
        private UnitTestMock _unitTestMock;

        public SubscriptionControllerTest()
        {
            _unitTestMock = new UnitTestMock();
            _loggerMock = new Mock<ILogger<SubscriptionController>>();
            _dynamoDBManagerMock = new Mock<DynamoDBManager>();
            _subscriptionController = new SubscriptionController(_loggerMock.Object, _dynamoDBManagerMock.Object);
        }

        public void Setup(bool clientCreatedSuccessfully = true, bool tableExists = true, List<Subscription> subscriptionFromUserID = null)
        {
            _dynamoDBManagerMock.Setup(m => m.CreateClient()).Returns(clientCreatedSuccessfully);
            _dynamoDBManagerMock.Setup(m => m.CheckingTableExistenceAsync(It.IsAny<string>())).ReturnsAsync(tableExists);
            _dynamoDBManagerMock.Setup(m => m.AddItem(It.IsAny<string>(), It.IsAny<Dictionary<string, AttributeValue>>())).ReturnsAsync(true);
            _dynamoDBManagerMock.Setup(m => m.GetItemsFromSubscriptionByUserID(It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(subscriptionFromUserID);
        }

        [TestMethod]
        public async Task SubscriptionController_CreateSubscriptionAsync_Ok()
        {
            Setup();

            var subscription = new Subscription()
            {
                ID = "1234",
                Price = 12.0M,
                Title = "Amazon Prime",
                UserID = _unitTestMock._validUserID1
            };

            var controllerResult = await _subscriptionController.CreateSubscriptionAsync(subscription);

            var result = controllerResult as OkObjectResult;
           
            Assert.IsNotNull(result);
            Assert.IsTrue(result.StatusCode == 200);
            Assert.IsTrue(result.Value.Equals($"Added subscription with id {subscription.ID}"));
        }

        [TestMethod]
        public async Task SubscriptionController_CreateSubscriptionAsync_FailCreatingClient()
        {
            Setup(false);

            var subscription = new Subscription()
            {
                ID = "1234",
                Price = 12.0M,
                Title = "Amazon Prime",
                UserID = _unitTestMock._validUserID1
            };

            var controllerResult = await _subscriptionController.CreateSubscriptionAsync(subscription);

            var result = controllerResult as ObjectResult;

            Assert.IsNotNull(result);
            Assert.IsTrue(result.StatusCode == 500);
            Assert.IsTrue(result.Value.Equals("There was a problem while creating a client!"));
        }

        [TestMethod]
        public async Task SubscriptionController_CreateSubscriptionAsync_TableNotFound()
        {
            Setup(true, false);

            var subscription = new Subscription()
            {
                ID = "1234",
                Price = 12.0M,
                Title = "Amazon Prime",
                UserID = _unitTestMock._validUserID1
            };

            var controllerResult = await _subscriptionController.CreateSubscriptionAsync(subscription);

            var result = controllerResult as ObjectResult;

            Assert.IsNotNull(result);
            Assert.IsTrue(result.StatusCode == 400);
            Assert.IsTrue(result.Value.Equals("Subscription table not found"));
        }

        [TestMethod]
        public async Task SubscriptionController_GetSubscriptionsFromUserAsync_Ok()
        {
            var subscriptions = new List<Subscription>()
            {
                new Subscription
                {
                    ID = "1234",
                    Price = 12.0M,
                    Title = "Amazon Prime",
                    UserID = _unitTestMock._validUserID1
                }
            };

            Setup(true, true, subscriptions);

            var controllerResult = await _subscriptionController.GetSubscriptionsFromUserAsync(_unitTestMock._validUserID1);

            var result = controllerResult as OkObjectResult;

            Assert.IsNotNull(result);
            Assert.IsTrue(result.StatusCode == 200);
            Assert.AreEqual(subscriptions, result.Value);
        }

        [TestMethod]
        public async Task SubscriptionController_GetSubscriptionsFromUserAsync_FailCreatingClient()
        {
            Setup(false);

            var controllerResult = await _subscriptionController.GetSubscriptionsFromUserAsync(_unitTestMock._validUserID1);

            var result = controllerResult as ObjectResult;

            Assert.IsNotNull(result);
            Assert.IsTrue(result.StatusCode == 500);
            Assert.IsTrue(result.Value.Equals("There was a problem while creating a client!"));
        }

        [TestMethod]
        public async Task SubscriptionController_GetSubscriptionsFromUserAsync_TableNotFound()
        {
            Setup(true, false);

            var controllerResult = await _subscriptionController.GetSubscriptionsFromUserAsync(_unitTestMock._validUserID1);

            var result = controllerResult as ObjectResult;

            Assert.IsNotNull(result);
            Assert.IsTrue(result.StatusCode == 400);
            Assert.IsTrue(result.Value.Equals("Subscription table not found"));
        }
    }
}
