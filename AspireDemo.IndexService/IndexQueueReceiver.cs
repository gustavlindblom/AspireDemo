using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace AspireDemo.IndexService
{
  internal class IndexQueue { }

  internal class LoggedEventingBasicConsumer(IModel model, ILogger<IndexQueue> logger) : EventingBasicConsumer(model)
  {

    public readonly ILogger<IndexQueue> Logger = logger;
  }
}
