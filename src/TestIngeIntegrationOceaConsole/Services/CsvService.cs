using System.Globalization;
using System.Text;

namespace OceaSmartBuildingApp
{
    /// <summary>
    /// Lit les fichiers CSV qui contient des WorkOrders. Le service
    /// valide chaque lignes selon les règles métier et extrait les
    /// lignes rejetées.
    /// </summary>
    public static class CsvService
    {
        /// <summary>
        /// Lit le fichier CSV et renvoie la liste des WorkOrder valides et
        /// la liste des WorkOrder rejetées et la raison du rejet.
        /// </summary>
        /// <param name="path">Le chemin du fichier CSV</param>
        /// <returns>Tuple de WorkOrders valides/rejetés</returns>
        public static (
            List<WorkOrder> ValidOrders,
            List<(string Line, string Reason)> Rejected
        ) ReadAndValidate(string path)
        {
            var valid = new List<WorkOrder>();
            var rejected = new List<(string line, string Reason)>();
            using var reader = new StreamReader(path, Encoding.UTF8);
            string? header = reader.ReadLine();
            if (header == null)
            {
                throw new InvalidDataException("Le fichier est vide ou sans en-tête.");
            }
            // Le header doit contenir 6 colonnes
            string[] headerParts = header.Split(';');
            if (headerParts.Length != 6)
            {
                throw new InvalidDataException($"En-tête invalide: {header}");
            }

            string? line;
            while ((line = reader.ReadLine()) != null)
            {
                // On ignore les lignes vides
                if (string.IsNullOrWhiteSpace(line))
                {
                    continue;
                }

                var rejectedReasons = new List<String>();

                // Sépare les lignes avec des ;
                // TODO: meilleure implémentation du parsing
                // s'il y a des ; dans les adresses...
                string[] parts = line.Split(';');

                if (parts.Length != 6)
                {
                    rejected.Add((line, "Nombre de champs invalide."));
                    continue;
                }

                string[] fields = new string[6];
                for (int i = 0; i < fields.Length; i++)
                {
                    fields[i] = i < parts.Length ? parts[i] : string.Empty;
                }
                string workOrderId = fields[0];
                string meterSerial = fields[1];
                string street = fields[2];
                string postalCode = fields[3].Trim();
                string city = fields[4];
                string plannedDateStr = fields[5];

                // Vérification de tous les critères de validation
                if (
                    string.IsNullOrWhiteSpace(workOrderId)
                    || string.IsNullOrWhiteSpace(meterSerial)
                    || string.IsNullOrWhiteSpace(street)
                    || string.IsNullOrWhiteSpace(postalCode)
                    || string.IsNullOrWhiteSpace(city)
                    || string.IsNullOrWhiteSpace(plannedDateStr)
                )
                {
                    rejectedReasons.Add("Champ(s) manquant(s)");
                }
                if (postalCode.Length != 5 || !postalCode.All(char.IsDigit))
                {
                    rejectedReasons.Add("Code postal invalide");
                }
                if (
                    !DateTime.TryParseExact(
                        plannedDateStr,
                        "yyyy-MM-dd",
                        CultureInfo.InvariantCulture,
                        DateTimeStyles.None,
                        out var plannedDate
                    )
                )
                {
                    rejectedReasons.Add("Date planifiée invalide");
                }

                if (rejectedReasons.Count > 0)
                {
                    rejected.Add((line, string.Join(", ", rejectedReasons)));
                }
                else
                {
                    // Si toutes les validations sont passées, crée un WorkOrder.
                    var wo = new WorkOrder(
                        workOrderId,
                        meterSerial,
                        street,
                        postalCode,
                        city,
                        plannedDate
                    );
                    valid.Add(wo);
                }
            }
            return (valid, rejected);
        }

        /// <summary>
        /// Ecris les lignes rejetées dans le fichier CSV et
        /// la/les raison(s) du rejet.
        /// </summary>
        /// <param name="path">Le chemin de sortie du fichier CSV de rejet</param>
        /// <param name="rejected">La collection des lignes rejetées ainsi que la/les raisons du rejet</param>
        public static void WriteRejectedCsv(
            string path,
            IEnumerable<(string line, string Reason)> rejected
        )
        {
            using var writer = new StreamWriter(path, false, Encoding.UTF8);

            writer.WriteLine(
                "WorkOrderId;MeterSerial;Street;PostalCode;City;PlannedDate;RejectReason"
            );
            foreach (var (line, reason) in rejected)
            {
                writer.WriteLine(line + ";" + reason);
            }
        }
    }
}
