using System.Text.Json;

namespace AspireDemo.Web
{
    public class ProductApiClient(HttpClient httpClient)
    {
        public async Task<ProductDto[]> GetProductsAsync()
        {
            return await httpClient.GetFromJsonAsync<ProductDto[]>("/product") ?? [];
        }

        public async Task<ProductDto?> CreateProductAsync(ProductDto product)
        {
            var result = await httpClient.PostAsJsonAsync("/product", product);
            if (result == null || !result.IsSuccessStatusCode)
                return null;

            string body = await result.Content.ReadAsStringAsync() ?? string.Empty;
            return JsonSerializer.Deserialize<ProductDto>(body);
        }

        public async Task<ProductDto?> GetDetailsAsync(string slug)
        {
            return await httpClient.GetFromJsonAsync<ProductDto>($"/product/{slug}") ?? null;
        }
    }
}

public record ProductDto(string ProductName, string ProductDescription, string Slug);
