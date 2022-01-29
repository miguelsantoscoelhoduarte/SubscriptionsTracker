using SubscriptionsManager.AWSTools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SubscriptionsManager.Domain.Models
{
    public class Subscription : IItem
    {
        public string ID { get; set; }

        public string Title { get; set; }

        public decimal Price { get; set; }

        public string UserID { get; set; }
    }
}
