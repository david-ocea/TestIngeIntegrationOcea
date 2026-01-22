using System.Text.Json;

namespace OceaSmartBuildingApp.Services;

/// <summary>
/// Service responsable de l'appel API géocodage.
/// Convertit les adresses en coordonnées géographiques.
/// </summary>
public class GeocodingService
{
    private readonly HttpClient _client;
    private const string BaseUrl = "https://data.geopf.fr/geocodage/search";

    public GeocodingService(HttpClient client)
    {
        _client = client ?? throw new ArgumentNullException(nameof(client));
    }

    /// <summary>
    /// Appel à l'API de façon asynchrone pour récupérer les coordonnées GPS.
    /// Si l'API ne renvoit pas de résultats ou s'il y a une erreur,
    /// la méthode renvoit null au lieu de renvoyer une erreur.
    /// </summary>
    /// <param name="address">L'adresse pour laquelle on souhaite les coordonnées.</param>
    /// <returns>Un GeoResult ou null si non trouvé.</returns>
    public async Task<GeoResult?> GeocodeAsync(string address)
    {
        if (string.IsNullOrWhiteSpace(address))
        {
            return null;
        }
        // TODO: gestion des quotes dans les adresses
        string url = BaseUrl + "?q=" + Uri.EscapeDataString(address) + "&limit=1";
        using var response = await _client.GetAsync(url);
        if (!response.IsSuccessStatusCode)
        {
            return null;
        }
        using var stream = await response.Content.ReadAsStreamAsync();
        using var doc = await JsonDocument.ParseAsync(stream);
        if (
            !doc.RootElement.TryGetProperty("features", out var features)
            || features.GetArrayLength() == 0
        )
        {
            return null;
        }

        var first = features[0];
        // Extrait les coordonnées [longitude, latitude] : geometry.coordinates
        if (
            !first.TryGetProperty("geometry", out var geometry)
            || !geometry.TryGetProperty("coordinates", out var coords)
        )
        {
            return null;
        }
        if (coords.GetArrayLength() < 2)
        {
            return null;
        }
        double longitude = coords[0].GetDouble();
        double latitude = coords[1].GetDouble();

        // Extrait le score : properties.score
        double score = 0;
        if (
            first.TryGetProperty("properties", out var props)
            && props.TryGetProperty("score", out var scoreProp)
        )
        {
            score = scoreProp.GetDouble();
        }

        return new GeoResult
        {
            Latitude = latitude,
            Longitude = longitude,
            Score = score,
        };
    }
}
