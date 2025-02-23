using System.Text.Json.Serialization;

namespace MHClub.Models.RestCountries;

public class CountryInfo
{
    [JsonPropertyName("name")]
    public Name Name { get; set; }

    [JsonPropertyName("tld")]
    public List<string> Tld { get; set; }

    [JsonPropertyName("cca2")]
    public string Cca2 { get; set; }

    [JsonPropertyName("ccn3")]
    public string Ccn3 { get; set; }

    [JsonPropertyName("cca3")]
    public string Cca3 { get; set; }

    [JsonPropertyName("independent")]
    public bool? Independent { get; set; }

    [JsonPropertyName("status")]
    public string Status { get; set; }

    [JsonPropertyName("unMember")]
    public bool? UnMember { get; set; }

    [JsonPropertyName("currencies")]
    public Dictionary<string, Currency> Currencies { get; set; }

    [JsonPropertyName("idd")]
    public Idd Idd { get; set; }

    [JsonPropertyName("capital")]
    public List<string> Capital { get; set; }

    [JsonPropertyName("altSpellings")]
    public List<string> AltSpellings { get; set; }

    [JsonPropertyName("region")]
    public string Region { get; set; }

    [JsonPropertyName("languages")]
    public Dictionary<string, string> Languages { get; set; }

    [JsonPropertyName("translations")]
    public Dictionary<string, Translation> Translations { get; set; }

    // Можно добавить еще свойства (всякие координаты, флаги, и т.д.) если нужно.
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