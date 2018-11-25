using System;

namespace OrdersService.Messages
{
    public class OrderCreated
    {
        public string Product { get; set; }
        public string User { get; set; }

        public int Quantity { get; set; }
    }
}
