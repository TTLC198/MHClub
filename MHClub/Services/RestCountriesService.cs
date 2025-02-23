using System.Text.Json;
using MHClub.Models.RestCountries;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace MHClub.Services;

public class RestCountriesService
{
    private readonly string _baseApiUrl;
    
    public RestCountriesService(IConfiguration config)
    {
        _baseApiUrl = config.GetValue<string>("RestCountriesApiUrl") ?? "https://restcountries.com/v3.1";
    }

    public async Task<List<CountryInfo>> GetAll()
    {
        using var httpClient = new HttpClient();
        var responseStream = await httpClient.GetStreamAsync($"{_baseApiUrl}/all");
        var result = await JsonSerializer.DeserializeAsync<List<CountryInfo>>(responseStream);
        return result ?? [];
    }
    
    public async Task<List<SelectListItem>> GetAllForSelect()
    {
        using var httpClient = new HttpClient();
        var responseStream = await httpClient.GetStreamAsync($"{_baseApiUrl}/all");
        var result = await JsonSerializer.DeserializeAsync<List<CountryInfo>>(responseStream);
        return result?.Select(x => new SelectListItem
        {
            Text = x.Translations.FirstOrDefault(tr => tr.Key == "rus").Value.Common,
            Value = x.Cca2
        }).ToList() ?? [];
    }
}