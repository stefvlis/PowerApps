using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Azure.Data.Tables;
using Azure;
using Azure.Storage.Blobs;
using System.Text;

namespace OrderFunction
{
    public record Order : ITableEntity
    {
        public string RowKey { get; set; } = default!;
        public string PartitionKey { get; set; } = default!;
        public string Name { get; init; } = default!;
        public string Amount  { get; init; }
        public ETag ETag { get; set; } = default!;
        public DateTimeOffset? Timestamp { get; set; } = default!;
    }

    public static class OrderFunction
    {
        [FunctionName("OrderFunction")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = null)] HttpRequest req,
            ILogger log)
        {

            var str = await req.ReadAsStringAsync();
            log.LogInformation($"Received new postMessage:{str}");

            //Read input (amount and order)
            string responseMessage = "ok";
            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            dynamic input = JsonConvert.DeserializeObject(requestBody);

            //Write Json file to BLOB
            byte[] byteArray = Encoding.ASCII.GetBytes(requestBody);
            MemoryStream stream = new MemoryStream(byteArray);
            var CONNECTION_STRING = Environment.GetEnvironmentVariable("CONNECTION_STRING");
            const string BLOB_CONTAINER = "orders";
            string GUID = Guid.NewGuid().ToString();
            var blobClient = new BlobClient(CONNECTION_STRING, BLOB_CONTAINER, $"{GUID}.json");
            blobClient.Upload(stream);

            //Create table client
            TableServiceClient tableServiceClient = new TableServiceClient(CONNECTION_STRING);
            TableClient tableClient = tableServiceClient.GetTableClient(tableName: "Orders");
            await tableClient.CreateIfNotExistsAsync();

            // Read amount
            string amount = input?.Bedrag;
            //string amount = requestBody;

            // Create new item using constructor
            var order1 = new Order()
            {
                RowKey = GUID,
                PartitionKey = "orders",
                Name = "processed",
                Amount = amount
            };
            // Add new item to server-side table
            await tableClient.AddEntityAsync<Order>(order1);
            return new OkObjectResult(amount);
        }
    }
}