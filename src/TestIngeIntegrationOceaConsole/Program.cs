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

// Crée un fichier CSV pour les WorkOrders rejetés. The
// output path is relative to the working directory.
string rejectedPath = $"{DefaultOutputFolder}/rejectedLines.csv";
CsvService.WriteRejectedCsv(rejectedPath, rejectedLines);

Console.WriteLine($"Lignes lues\t: {validOrders.Count + rejectedLines.Count}");
Console.WriteLine($"Lignes valides\t: {validOrders.Count}");
Console.WriteLine($"Lignes rejetées\t: {rejectedLines.Count}");
