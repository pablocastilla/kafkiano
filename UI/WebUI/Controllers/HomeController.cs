using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using InventoryService.Messages;
using Microsoft.AspNetCore.Mvc;
using OrdersService.Messages;
using WebUI.Models;

namespace WebUI.Controllers
{
    public class HomeController : Controller
    {
        private static readonly HttpClient client = new HttpClient();

        public IActionResult Index()
        {
            return View(new IndexViewModel());
        }

        [HttpPost]
        public async Task<IActionResult> AddStock(IndexViewModel model)
        {          
            await AddStockProxy(model.ProductToAdd,model.QuantityToAdd);

            return Redirect("/");
        }

        [HttpPost]
        public async Task<IActionResult> MakeOrder(IndexViewModel model)
        {
            await MakeOrderProxy(model.ProductToOrder, model.QuantityToOrder);

            return Redirect("/");
        }

        private static async Task AddStockProxy(string productName, int quantity)
        {
            var message = new ProductStockEvent()
            {
                Action = StockAction.Add,
                ProductName = productName,
                Quantity = quantity
            };


            var stringTask = await client.PostAsJsonAsync("https://localhost:44371/api/Inventory", message);
                       
        }

        private static async Task MakeOrderProxy(string productName, int quantity)
        {
            var message = new OrderCreated()
            {
                OrderId = Guid.NewGuid().ToString(),
                User="pcv",
                ProductName = productName,
                Quantity = quantity
            };


            var stringTask = await client.PostAsJsonAsync("https://localhost:44358/api/Orders", message);

        }


        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
