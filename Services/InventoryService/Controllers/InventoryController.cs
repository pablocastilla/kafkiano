﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Confluent.Kafka;
using Constants;
using InventoryService.Messages;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace InventoryService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class InventoryController : ControllerBase
    {
        // GET api/values
        [HttpGet]
        public ActionResult<IEnumerable<string>> Get()
        {
            return new string[] { "value1", "value2" };
        }

        // GET api/values/5
        [HttpGet("{id}")]
        public ActionResult<string> Get(int id)
        {
            return "value";
        }

        // POST api/values
        [HttpPost]
        public async Task Post([FromBody] ProductStockEvent value)
        {
            var config = new ProducerConfig { BootstrapServers = "127.0.0.1:9092" };

            using (var producer = new Producer<string, string>(config))
            {
                var deliveryReport = await producer.ProduceAsync(TOPICS.INVENTORYEVENTS, new Message<string, string>
                {
                    Key = value.ProductName,
                    Value = JsonConvert.SerializeObject(value)
                });

                
            }
        }

        // PUT api/values/5
        [HttpPut("{id}")]
        public void Put(int id, [FromBody] string value)
        {
        }

        // DELETE api/values/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
        }
    }
}
