using Azure.Storage.Queues;

namespace DurableFunctionApp1;

public class QueueService : IQueueService
{
    private readonly QueueClient queueClient;

    public QueueService(QueueClient queueClient)
    {
        this.queueClient = queueClient;
    }
}

public interface IQueueService
{

}