using InventoryService.Messages;
using Microsoft.Extensions.Caching.Memory;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace InventoryService
{
    //this should be moved to a persistent store doing idempotence by transactionid
    public class InventoryRepository
    {
        IMemoryCache cache;

        private string STOCKKEY = "STOCKKEY";

        public InventoryRepository(IMemoryCache memoryCache)
        {
            cache = memoryCache;
        }

        public void SetStockInPersistence(ProductStockInfo stockInfo)
        {
            var stockDictionary = cache.GetOrCreate<Dictionary<string, int>>(STOCKKEY, (d) =>
            {
                return new Dictionary<string, int>();
            });

            stockDictionary[stockInfo.ProductName] = stockInfo.Stock;
        }

        public int AddStockToPersistence(string productName, int quantity)
        {
            var stockDictionary = cache.GetOrCreate<Dictionary<string, int>>(STOCKKEY, (d) =>
            {
                return new Dictionary<string, int>();
            });

            if(!stockDictionary.ContainsKey(productName))
            {
                stockDictionary.Add(productName, 0);
            }

            var currentStock = stockDictionary[productName];

            var finalStock = currentStock + quantity; 

            stockDictionary[productName] = finalStock;

            return finalStock;
        }

        public int GetStockFromPersistence(string productName)
        {
            var stockDictionary = cache.GetOrCreate<Dictionary<string, int>>(STOCKKEY, (d) =>
            {
                return new Dictionary<string, int>();
            });

            if (!stockDictionary.ContainsKey(productName))
            {
                stockDictionary.Add(productName, 0);
            }

            var currentStock = stockDictionary[productName];            

            return currentStock;
        }
    }
}
