#r "Microsoft.WindowsAzure.Storage"
#r "Newtonsoft.Json"

using System;
using System.Net;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Primitives;
using Newtonsoft.Json;
using Microsoft.WindowsAzure.Storage.Queue;


public static void Run(HttpRequest req, CloudQueue outputQueue, TraceWriter log)
{
    log.Info("C# HTTP trigger function processed a request.");

    string name = req.Query["name"];

    string requestBody = new StreamReader(req.Body).ReadToEnd();
    dynamic data = JsonConvert.DeserializeObject(requestBody);
    name = name ?? data?.name;
    
    var message = new CloudQueueMessage(name);
    outputQueue.AddMessage(message, TimeSpan.FromMinutes(3));

    /*
    return name != null
        ? (ActionResult)new OkObjectResult($"Hello, {name}")
        : new BadRequestObjectResult("Please pass a name on the query string or in the request body");
        */
}


/*

[StorageAccount("AzureWebJobsStorage")]
public static class QueueFunctions
{
    // HTTP trigger with queue output binding
    [FunctionName("QueueOutput")]
    [return: Queue("myqueue-items")]
    public static string QueueOutput([HttpTrigger] dynamic input,  TraceWriter log)
    {
        log.Info($"C# function processed: {input.Text}");
        return input.Text;
    }
}


public static void Run(MyType myEventHubMessage, CloudQueue outputQueue, TraceWriter log)
{
    var deviceId = myEventHubMessage.DeviceId;
    var data = JsonConvert.SerializeObject(myEventHubMessage);
    var msg = new CloudQueueMessage(data);
    log.Info($"C# Event Hub trigger function processed a message: {deviceId}");
    outputQueue.AddMessage(msg, TimeSpan.FromMinutes(3), null, null, null);

}

public class MyType
{
  public string DeviceId { get; set; }
  public double Field1{ get; set; }
  public double Field2 { get; set; }
  public double Field3 { get; set; }
}
 */