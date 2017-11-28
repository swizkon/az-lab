using System;
using System.Net;

static HttpClient client = new HttpClient();

public static void Run(string myQueueItem, TraceWriter log)
{
    log.Info($"C# Queue trigger function processed: {myQueueItem}");
    
    string data = myQueueItem;

    string url = $"http://localhost:12345/api/MyFirstHttpTrigger?Name={data}";

    log.Info($"GET: {url}");

    var result = client.GetStringAsync(url).Result;

    log.Info($"result: {result}");
}
