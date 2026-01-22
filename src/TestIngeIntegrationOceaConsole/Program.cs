using System.Text;
using System.Text.Json;
using OceaSmartBuildingApp;

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

var (validOrders, rejectedLines) = CsvService.ReadAndValidate(inputPath);

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

// Enrichit les Work Orders valides
List<WorkOrderResult> enrichedOrders = new List<WorkOrderResult>();
foreach (var wo in validOrders)
{
    var result = WorkOrderResult.FromWorkOrder(wo);
    enrichedOrders.Add(result);
}

// Serialize le résultat en JSON et l'écrit dans le fichier output.json
byte[] utf8Bytes = JsonSerializer.SerializeToUtf8Bytes(
    enrichedOrders,
    new JsonSerializerOptions { WriteIndented = true }
);
string outputJson = System.Text.Encoding.UTF8.GetString(utf8Bytes);

string outputPath = $"{DefaultOutputFolder}/output.json";
await File.WriteAllTextAsync(outputPath, outputJson, Encoding.UTF8);
