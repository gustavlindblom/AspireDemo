using AspireDemo.Domain.Models;
using Dapper;
using Microsoft.AspNetCore.Mvc;
using Npgsql;
using RabbitMQ.Client;
using System.Text;
using System.Text.Json;
using Constants = AspireDemo.Domain.Constants;

var builder = WebApplication.CreateBuilder(args);

// Add service defaults & Aspire components.
builder.AddServiceDefaults();
builder.AddNpgsqlDataSource(Constants.ContainerNames.PostgresDatabase);
builder.AddRabbitMQ(Constants.ContainerNames.RabbitMqContainer);


// Add services to the container.
builder.Services.AddProblemDetails();

var app = builder.Build();

// Configure the HTTP request pipeline.
app.UseExceptionHandler();

app.MapGet("/product", (NpgsqlDataSource dataSource) =>
{
    string command = "SELECT * FROM products";
    using var connection = dataSource.OpenConnection();
    IEnumerable<Product> queryResult = connection.Query<Product>(command);

    return queryResult.ToArray();
});

app.MapPost("/product", (NpgsqlDataSource dataSource, IConnection connection, ProductRequest request) =>
{
    string command =    "INSERT INTO products (productname, productdescription, slug) " +
                        "VALUES (@productName, @productDescription, @slug) " + 
                        "RETURNING productid";
    string slug = request.ProductName.Replace(" ", "-").ToLower();
    var queryArguments = new
    {
        productName         = request.ProductName,
        productDescription  = request.ProductDescription,
        slug
    };

    using var sqlConnection = dataSource.OpenConnection();
    var id = sqlConnection.QuerySingle<int>(command, queryArguments);
    
    using var channel = connection.CreateModel();

    Product product = new Product(id, request.ProductName, request.ProductDescription, slug);

    channel.QueueDeclare(
      queue: Constants.RoutingKeys.AddOrUpdateProduct,
      durable: false,
      exclusive: false,
      autoDelete: false,
      arguments: null
    );

    string message = JsonSerializer.Serialize(product);
    var body = Encoding.UTF8.GetBytes(message);

    channel.BasicPublish(
      exchange: string.Empty,
      routingKey: Constants.RoutingKeys.AddOrUpdateProduct,
      basicProperties: null,
      body: body
    );

    return product;
});

app.MapGet("/product/{slug}", (NpgsqlDataSource dataSource, string slug) =>
{
    string command = "SELECT * FROM products WHERE slug = @slug";
    var queryArguments = new
    {
        slug
    };

    using var connection = dataSource.OpenConnection();
    Product? product = connection.QuerySingleOrDefault<Product>(command, queryArguments);

    return product;
});

app.MapDefaultEndpoints();

app.Run();

record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
  public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}

record ProductRequest(string ProductName, string ProductDescription);