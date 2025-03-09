using System.Text.Json;
using MHClub.Models.RestCountries;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace MHClub.Services;

public class RestCountriesService
{
    private readonly string _baseApiUrl;
    private readonly List<SelectListItem> _defaultItems = [new("Не выбрано", null)];
    
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
        try
        {
            using var httpClient = new HttpClient() { };
            var responseMessage = await httpClient.GetAsync($"{_baseApiUrl}/all");
            if (!responseMessage.IsSuccessStatusCode) return _defaultItems;
            var result =
                await JsonSerializer.DeserializeAsync<List<CountryInfo>>(
                    await responseMessage.Content.ReadAsStreamAsync());
            return result?.Select(x =>
            {
                var friendlyName = x.Translations.FirstOrDefault(tr => tr.Key == "rus").Value.Common;
                return new SelectListItem
                {
                    Text = friendlyName,
                    Value = friendlyName
                };
            }).ToList().Prepend(_defaultItems.First()).ToList() ?? _defaultItems;
        }
        catch
        {
            return _defaultItems;
        }
    }
    
    public async Task<List<SelectListItem>> GetRusForSelect()
    {
        try
        {
            using var httpClient = new HttpClient() { };
            var responseMessage = await httpClient.GetAsync($"{_baseApiUrl}/all");
            if (!responseMessage.IsSuccessStatusCode) return _defaultItems;
            var result =
                await JsonSerializer.DeserializeAsync<List<CountryInfo>>(
                    await responseMessage.Content.ReadAsStreamAsync());
            return result?.Select(x =>
            {
                var friendlyName = x.Translations.FirstOrDefault(tr => tr.Key == "rus").Value.Common;
                return new SelectListItem
                {
                    Text = friendlyName,
                    Value = friendlyName
                };
            }).Where(x => x.Value == "Россия").ToList().Prepend(_defaultItems.First()).ToList() ?? _defaultItems;
        }
        catch
        {
            return _defaultItems;
        }
    }
    
    public async Task<List<CountryInfo>> GetByCode(string code)
    {
        try
        {
            using var httpClient = new HttpClient() { };
            var responseMessage = await httpClient.GetAsync($"{_baseApiUrl}/alpha/{code}");
            if (!responseMessage.IsSuccessStatusCode) return [];
            var result =
                await JsonSerializer.DeserializeAsync<List<CountryInfo>>(
                    await responseMessage.Content.ReadAsStreamAsync());
            return result ?? [];
        }
        catch
        {
            return [];
        }
    }
}