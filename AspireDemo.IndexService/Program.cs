using AspireDemo.Domain.Models;
using AspireDemo.IndexService;
using RabbitMQ.Client;
using System.Text;
using System.Text.Json;
using Constants = AspireDemo.Domain.Constants;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();
builder.AddRabbitMQ(Constants.ContainerNames.RabbitMqContainer);

List<Product> productsIndex = [];

var app = builder.Build();

app.MapGet("/search", (string query) =>
{
  List<SearchResult> result = [];

  query = query.ToLower();

  foreach (var product in productsIndex)
  {
    string productName = product.ProductName;
    string productDescription = product.ProductDescription;
    if (productName.Contains(query, StringComparison.CurrentCultureIgnoreCase) || productDescription.Contains(query, StringComparison.CurrentCultureIgnoreCase))
    {
      var productResult = new SearchResult(productName, productDescription, product.Slug);
      result.Add(productResult);
    }
  }

  return
    new SearchResults(result.Count, result);
});

app.MapDefaultEndpoints();

var connection = app.Services.GetRequiredService<IConnection>();
using var channel = connection.CreateModel();
channel.QueueDeclare(
  queue: Constants.RoutingKeys.AddOrUpdateProduct,
  durable: false,
  exclusive: false,
  autoDelete: false,
  arguments: null
);
var consumer = new LoggedEventingBasicConsumer(channel, app.Services.GetRequiredService<ILogger<IndexQueue>>());
consumer.Received += (model, ea) =>
{
  var body = ea.Body.ToArray();
  var message = Encoding.UTF8.GetString(body);
  consumer.Logger.LogInformation(
    "Received message: {DeliveryTag}\n" +
    "Message body: {MessageBody}",
    ea.DeliveryTag,
    message
  );
  channel.BasicAck(ea.DeliveryTag, false);

  var product = JsonSerializer.Deserialize<Product>(message);
  if (product == null)
  {
    consumer.Logger.LogError(
      "Received invalid json for product indexing"
    );
    return;
  }
  var existing = productsIndex.FirstOrDefault(i => i.ProductId == product.ProductId);
  if (existing != null)
  {
    existing.ProductName = product.ProductName;
    existing.ProductDescription = product.ProductDescription;
    existing.Slug = product.Slug;
    return;
  }

  productsIndex.Add(product);
};


channel.BasicConsume(
  queue: Constants.RoutingKeys.AddOrUpdateProduct,
  consumer: consumer
);


app.Run();

record SearchResults(int TotalResults, IList<SearchResult> Entries);
record SearchResult(string Title, string Description, string Slug);