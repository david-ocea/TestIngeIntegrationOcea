using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Unicode;
using OceaSmartBuildingApp.Models;
using OceaSmartBuildingApp.Services;
using TestIngeIntegrationOceaConsole.Services;

/// <summary>
/// Chemin du fichier d'entrée. Peut être modifié en rajoutant
/// un argument en ligne de commande (cf README).
/// </summary>
var DefaultInputFileName = "Data/input.csv";

/// <summary>
/// Dossier dans lequel est mis le fichier de sortie
/// </summary>
var DefaultOutputFolder = "Output";

string inputPath = args.Length > 0 ? args[0] : DefaultInputFileName;

if (!File.Exists(inputPath))
{
    Console.Error.WriteLine($"Fichier d'entré CSV non trouvé : {inputPath}");
    return;
}

var csvService = new CsvService(path => new FileLineReader(path));

var (validOrders, rejectedLines) = csvService.ReadAndValidate(inputPath);

try
{
    Directory.CreateDirectory(DefaultOutputFolder);
}
catch (Exception ex)
{
    Console.Error.WriteLine($"Erreur lors de la création du dossier de sortie: {ex.Message}");
    return;
}

// Crée un fichier CSV pour les WorkOrders rejetés. The
// output path is relative to the working directory.
string rejectedPath = $"{DefaultOutputFolder}/rejectedLines.csv";
CsvService.WriteRejectedCsv(rejectedPath, rejectedLines);

Console.WriteLine($"Lignes lues\t: {validOrders.Count + rejectedLines.Count}");
Console.WriteLine($"Lignes valides\t: {validOrders.Count}");
Console.WriteLine($"Lignes rejetées\t: {rejectedLines.Count}");

// Instancie le service de geocodage.
// Le HttpClient est partagé à travers toute l'application
using HttpClient httpClient = new HttpClient();
var geocodingService = new GeocodingService(httpClient);

// Enrichit les Work Orders valides
List<WorkOrderResult> enrichedOrders = new List<WorkOrderResult>();
foreach (var wo in validOrders)
{
    var result = WorkOrderResult.FromWorkOrder(wo);
    try
    {
        var geo = await geocodingService.GeocodeAsync(result.AddressLabel);
        result.SetGPS(geo);
    }
    catch (Exception ex)
    {
        // En cas d'échec ou de TO sur l'appel géocodage, le traitement doit continuer
        // Les erreurs sont affchées dans la console pour information.
        Console.Error.WriteLine(
            $"Erreur lors de l'appel géocodage pour '{result.AddressLabel}': {ex.Message}"
        );
    }
    enrichedOrders.Add(result);
}

// Serialize le résultat en JSON et l'écrit dans le fichier output.json
var options = new JsonSerializerOptions
{
    Encoder = JavaScriptEncoder.Create(UnicodeRanges.All),
    WriteIndented = true,
};

byte[] utf8Bytes = JsonSerializer.SerializeToUtf8Bytes(enrichedOrders, options);
string outputJson = Encoding.UTF8.GetString(utf8Bytes);

string outputPath = $"{DefaultOutputFolder}/output.json";
await File.WriteAllTextAsync(outputPath, outputJson, Encoding.UTF8);

// Simule le HTTP POST.
var payloadBytes = Encoding.UTF8.GetBytes(outputJson);
var content = new StringContent(outputJson, Encoding.UTF8, "application/json");
HttpResponseMessage response;
try
{
    Console.WriteLine($"Envoi HTTP en cours...");
    response = await httpClient.PostAsync("https://httpbin.org/post", content);
    Console.WriteLine(
        $"Envoi HTTP terminé. Code: {(int)response.StatusCode}. Taille du payload: {payloadBytes.Length} octets."
    );
}
catch (Exception ex)
{
    Console.Error.WriteLine($"Erreur lors de l'envoi HTTP: {ex.Message}");
}
