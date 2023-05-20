using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Specialized;
using Azure.Storage.Queues;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;

namespace DurableFunctionApp1;

public static class Orchestrator
{
    [FunctionName("Orchestrator")]
    public static async Task<List<string>> RunOrchestrator(
        [OrchestrationTrigger] IDurableOrchestrationContext context)
    {
        var outputs = new List<string>();

        // Replace "hello" with the name of your Durable Activity Function.
        outputs.Add(await context.CallActivityAsync<string>(nameof(SayHello), "Tokyo"));
        outputs.Add(await context.CallActivityAsync<string>(nameof(SayHello), "Seattle"));
        outputs.Add(await context.CallActivityAsync<string>(nameof(SayHello), "London"));

        // returns ["Hello Tokyo!", "Hello Seattle!", "Hello London!"]
        return outputs;
    }

#if false
    [FunctionName(nameof(SayHello))]
    public static string SayHello([ActivityTrigger] string name, ILogger log)
    {
        log.LogInformation("Saying hello to {name}.", name);
        return $"Hello {name}!";
    }
#endif

    [FunctionName(nameof(SayHello))]
    public static string SayHello([ActivityTrigger] IDurableActivityContext context, ILogger log)
    {
        var name = context.GetInput<string>();
        log.LogInformation("Saying hello to {name}.", name);
        return $"Hello {name}!";
    }

    [FunctionName("Orchestrator_HttpStart")]
    public static async Task<HttpResponseMessage> HttpStart(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post")] HttpRequestMessage req,
        [DurableClient] IDurableOrchestrationClient starter,
        ILogger log)
    {
        // Function input comes from the request content.
        string instanceId = await starter.StartNewAsync("Orchestrator", null);

        log.LogInformation("Started orchestration with ID = '{instanceId}'.", instanceId);

        return starter.CreateCheckStatusResponse(req, instanceId);
    }

    #region Storage triggers

    [FunctionName(nameof(BlobTriggerFunction))]
    public static async Task BlobTriggerFunction(
        //[BlobTrigger("myblob-items/{name}")] string myBlob,
        [BlobTrigger("myblob-items/{name}")] BlobClient myBlob,
        [DurableClient] IDurableOrchestrationClient starter,
        ILogger log)
    {
        string instanceId = await starter.StartNewAsync(nameof(OrchestratorFunction), null, myBlob);
        var containerClient = myBlob.GetParentBlobContainerClient();
        var blobServiceClient = containerClient.GetParentBlobServiceClient();

        log.LogInformation($"Started orchestration with ID = '{instanceId}' by blob trigger.");
    }

    [FunctionName(nameof(QueueTriggerFunction))]
    public static async Task QueueTriggerFunction(
        //[QueueTrigger("myqueue-items")] string myQueueItem,
        [QueueTrigger("myqueue-items")] QueueClient myQueueItem,
        [DurableClient] IDurableOrchestrationClient starter,
        ILogger log)
    {
        string instanceId = await starter.StartNewAsync(nameof(OrchestratorFunction), null, myQueueItem);

        log.LogInformation($"Started orchestration with ID = '{instanceId}' by queue trigger.");
    }

    [FunctionName(nameof(OrchestratorFunction))]
    public static async Task<List<string>> OrchestratorFunction(
        [OrchestrationTrigger] IDurableOrchestrationContext context)
    {
        string input = context.GetInput<string>();
        var outputs = new List<string>();

        // Replace "hello" with the name of your Durable Activity Function.
        outputs.Add(await context.CallActivityAsync<string>(nameof(SayHello), "Tokyo"));
        outputs.Add(await context.CallActivityAsync<string>(nameof(SayHello), "Seattle"));
        outputs.Add(await context.CallActivityAsync<string>(nameof(SayHello), "London"));
        outputs.Add(await context.CallActivityAsync<string>(nameof(SayHello), input));

        return outputs;
    }

    #endregion

}
