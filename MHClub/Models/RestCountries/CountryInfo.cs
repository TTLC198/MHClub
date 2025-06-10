using System.Text.Json.Serialization;

namespace MHClub.Models.RestCountries;

public class CountryInfo
{
    [JsonPropertyName("name")]
    public Name Name { get; set; }
    [JsonPropertyName("translations")]
    public Dictionary<string, Translation> Translations { get; set; }
}

public class Name
{
    [JsonPropertyName("common")]
    public string Common { get; set; }

    [JsonPropertyName("official")]
    public string Official { get; set; }

    [JsonPropertyName("nativeName")]
    public Dictionary<string, NativeName> NativeName { get; set; }
}

public class NativeName
{
    [JsonPropertyName("official")]
    public string Official { get; set; }

    [JsonPropertyName("common")]
    public string Common { get; set; }
}

public class Currency
{
    [JsonPropertyName("name")]
    public string Name { get; set; }

    [JsonPropertyName("symbol")]
    public string Symbol { get; set; }
}

public class Idd
{
    [JsonPropertyName("root")]
    public string Root { get; set; }

    [JsonPropertyName("suffixes")]
    public List<string> Suffixes { get; set; }
}

public class Translation
{
    [JsonPropertyName("official")]
    public string Official { get; set; }

    [JsonPropertyName("common")]
    public string Common { get; set; }
}