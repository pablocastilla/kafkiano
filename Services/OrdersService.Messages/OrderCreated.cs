using System;

namespace OrdersService.Messages
{
    public class OrderCreated
    {
        public string ProductName { get; set; }

        public int Quantity { get; set; }

        public string OrderId { get; set; }
        
        public string User { get; set; }

        
    }
}
