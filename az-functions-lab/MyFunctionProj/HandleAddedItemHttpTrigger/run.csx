#r "Newtonsoft.Json"

using System;
using System.Net;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Primitives;
using Newtonsoft.Json;


public class AutoscoutItem
{
    public string PartitionKey { get; set; }
    public string RowKey { get; set; }
    
    public string Title { get; set; }
}

public static IActionResult Run(HttpRequest req, TraceWriter log, out string newItem, out AutoscoutItem autoscoutItem)
{
    // log.Info("C# HTTP trigger function processed a request.");

    string name = req.Query["name"];

    string requestBody = new StreamReader(req.Body).ReadToEnd();
    dynamic data = JsonConvert.DeserializeObject(requestBody);
    name = name ?? data?.name;

    newItem = name;

    autoscoutItem = new AutoscoutItem();
    autoscoutItem.PartitionKey = "";
    autoscoutItem.RowKey = Guid.NewGuid().ToString();
    autoscoutItem.Title = name;

    return name != null 
        ? (ActionResult)new OkObjectResult($"Hello, {name}")
        : new BadRequestObjectResult("Please pass a name on the query string or in the request body");
}
