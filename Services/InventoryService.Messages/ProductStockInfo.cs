using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace InventoryService.Messages
{
    public class ProductStockInfo
    {
        public string ProductName { get; set; }

        public int Stock { get; set; }
    }
}
