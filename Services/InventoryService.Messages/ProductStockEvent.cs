using System;

namespace InventoryService.Messages
{
    public class ProductStockEvent
    {
        public string ProductName { get; set; }

        public StockAction Action { get; set; }

        public int Quantity { get; set; }
    }

    public enum StockAction { Add,Remove}
}
