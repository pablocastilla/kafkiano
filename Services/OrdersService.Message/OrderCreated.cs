using System;

namespace OrdersService.Message
{
    public class OrderCreated
    {
        public string Product { get; set; }
        public string User { get; set; }

        public int Quantity { get; set; }
    }
}
