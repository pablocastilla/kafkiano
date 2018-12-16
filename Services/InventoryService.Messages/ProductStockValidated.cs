using System;
using System.Collections.Generic;
using System.Text;

namespace InventoryService.Messages
{
    public class ProductStockValidated
    {
        public string ProductName { get; set; }
              

        public string OrderId { get; set; }

        public bool Ok { get; set; }
    }
}
