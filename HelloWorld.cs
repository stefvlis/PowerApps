using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Collections.Generic;
using Microsoft.WindowsAzure.Storage.Blob;
using Azure.Storage.Blobs;

namespace menufunction
{
    public static class HelloWorld
    {
        [FunctionName("HelloWorld")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = null)] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a request.");

            var CONNECTION_STRING = Environment.GetEnvironmentVariable("CONNECTION_STRING");
            const string BLOB_CONTAINER = "menu";
            string jsonFile = "menu.json";
            var blobClient = new BlobClient(CONNECTION_STRING, BLOB_CONTAINER, jsonFile);
            var content = await blobClient.OpenReadAsync();
            StreamReader reader = new StreamReader(content);
            string RawMenu = reader.ReadToEnd();

            return new OkObjectResult(RawMenu);
        }
    }
}
