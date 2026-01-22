using TestIngeIntegrationOceaConsole.Utils;

namespace OceaSmartBuildingApp.Models;

/// <summary>
/// Représente un Work Order lu depuis le fichier CSV.
/// Vérficiation de la validité du WorkOrder via les
/// règles métiers fournies.
/// </summary>
public class WorkOrder
{
    public string WorkOrderId { get; }
    public string MeterSerial { get; }
    public string Street { get; }
    public string PostalCode { get; }
    public string City { get; }
    public DateTime PlannedDate { get; }

    public WorkOrder(
        string workOrderId,
        string meterSerial,
        string street,
        string postalCode,
        string city,
        DateTime plannedDate
    )
    {
        WorkOrderId = workOrderId.Trim();
        MeterSerial = meterSerial.Trim();
        Street = StringHelper.NormalizeWhitespace(street.Trim());
        PostalCode = postalCode.Trim();
        City = StringHelper.NormalizeWhitespace(city.Trim());
        PlannedDate = plannedDate;
    }
}
