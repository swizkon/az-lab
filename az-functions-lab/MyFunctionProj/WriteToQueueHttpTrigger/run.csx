#r "Microsoft.WindowsAzure.Storage"
#r "Newtonsoft.Json"
#r "Microsoft.Azure.WebJobs"
#r "System.Net"

using System;
using System.Net;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Primitives;
using Newtonsoft.Json;
using Microsoft.WindowsAzure.Storage.Queue;
using Microsoft.Azure.WebJobs;

public interface IResult
{
    HttpStatusCode Status { get; }
}

public class OkResult : IResult
{
    public HttpStatusCode Status => HttpStatusCode.OK;
    public string Payload { get; set;} 

    public OkResult(string data)
    {
        Payload = data;
    }
}

public static IResult Run(HttpRequest req, TraceWriter log)
{
    log.Info("HttpTrigger does WriteToQueue");

    string requestBody = new StreamReader(req.Body).ReadToEnd();
    dynamic data = JsonConvert.DeserializeObject(requestBody);
    var name = data?.name;

    try
    {
        return new OkResult(name);
    }
    catch (System.Exception ex)
    {
        log.Error(ex.Message);
        // return ex.Message;
        return new OkResult(ex.Message);
    }
}

/*

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