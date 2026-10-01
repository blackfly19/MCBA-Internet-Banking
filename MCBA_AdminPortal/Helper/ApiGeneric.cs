using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Caching.Memory;

namespace MCBA_AdminPortal.Helper;

public class ApiGeneric : IApiGeneric
{
    private readonly HttpClient _httpClient;
    private readonly string _apiBaseUrl;
    private readonly IMemoryCache _cache;
    private const string TokenCacheKey = "AdminToken";

    public ApiGeneric(HttpClient httpClient, IConfiguration configuration, IMemoryCache cache)
    {
        _httpClient = httpClient;
        _apiBaseUrl = configuration["ConnectionStrings:AdminApiUrl"];
        _cache = cache;
    }

    private void AddAuthHeader()
    {
        // Get token from cache
        if (_cache.TryGetValue(TokenCacheKey, out string token) && !string.IsNullOrEmpty(token))
        {
            _httpClient.DefaultRequestHeaders.Clear();
            _httpClient.DefaultRequestHeaders.Add("X-API-Key", token);
        }
    }

    public T Get<T>(string endpoint)
    {
        AddAuthHeader();
        
        var response = _httpClient.GetAsync($"{_apiBaseUrl}/{endpoint}").Result;
        if (!response.IsSuccessStatusCode)
            return default;

        var json = response.Content.ReadAsStringAsync().Result;
        return JsonSerializer.Deserialize<T>(json, new JsonSerializerOptions 
        { 
            PropertyNameCaseInsensitive = true 
        });
    }

    public bool Put<T>(string endpoint, T data)
    {
        AddAuthHeader();
        
        var json = JsonSerializer.Serialize(data);
        var content = new StringContent(json, Encoding.UTF8, "application/json");
        var response = _httpClient.PutAsync($"{_apiBaseUrl}/{endpoint}", content).Result;
        return response.IsSuccessStatusCode;
    }

    public bool Post(string endpoint)
    {
        AddAuthHeader();
        
        var response = _httpClient.PostAsync($"{_apiBaseUrl}/{endpoint}", null).Result;
        return response.IsSuccessStatusCode;
    }
}