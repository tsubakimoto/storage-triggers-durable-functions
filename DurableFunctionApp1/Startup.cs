using Azure.Storage.Queues;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.Azure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;

[assembly: FunctionsStartup(typeof(DurableFunctionApp1.Startup))]
namespace DurableFunctionApp1;

public class Startup : FunctionsStartup
{
    public override void Configure(IFunctionsHostBuilder builder)
    {
        builder.Services.AddHttpClient();

        // https://stackoverflow.com/a/74421494
        builder.Services.AddAzureClients(clientBuilder =>
        {
            clientBuilder.AddClient<QueueClient, QueueClientOptions>(options =>
            {
                IServiceProvider sp = builder.Services.BuildServiceProvider();
                IConfiguration configuration = sp.GetRequiredService<IConfiguration>();
                string connectionString = configuration["AzureWebJobsStorage"];
                return new(connectionString, "myqueue-items");
            });
        });

        builder.Services.AddSingleton<IQueueService, QueueService>();
    }
}
