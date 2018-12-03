using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace WebUI
{
    public interface IGetProductStock { }

    public class GetProductStock: IGetProductStock
    {
        static HttpClient client = new HttpClient();

        Task executeCall;

        public GetProductStock()
        {
            executeCall = Task.Run(
                () => CallGetProductStock()
                );


        }

        public void CallGetProductStock()
        {
            var url = "http://localhost:8088/query";


            client.Timeout = TimeSpan.FromMilliseconds(Timeout.Infinite);

            var request = new HttpRequestMessage(HttpMethod.Post, url);
            request.Headers.Add("Accept", "application/vnd.ksql.v1+json");
            //request.Headers.Add("Content-Type", "application/vnd.ksql.v1+json");

            request.Content = new StringContent(
                        "{\"ksql\": \"SELECT * from InventoryEventsStream;\",\"streamsProperties\": {\"ksql.streams.auto.offset.reset\": \"earliest\"}}", 
                        Encoding.UTF8, "application/vnd.ksql.v1+json");

            using (var response = client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead))
            {
                using (var body = response.Result.Content.ReadAsStreamAsync().Result)
                using (var reader = new StreamReader(body))
                {
                    while (!reader.EndOfStream)
                    {
                        var message = reader.ReadToEndAsync().Result;

                    }
                }
            }
        }
    }
}
