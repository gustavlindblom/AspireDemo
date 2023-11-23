namespace AspireDemo.Domain
{
  public static class Constants
  {
    public static class ServiceNames
    {
      public const string ApiService = "apiservice";
      public const string FrontendService = "frontendservice";
      public const string IndexService = "indexservice";
    }

    public static class ContainerNames
    {
      public const string RedisContainer = "redis";
      public const string PostgresContainer = "postgres";
      public const string PostgresDatabase = "postgresdb";
      public const string RabbitMqContainer = "rabbitmq";
    }

    public static class RoutingKeys
    {
      public const string AddOrUpdateProduct = "hello";
    }
  }
}
