namespace OceaSmartBuildingApp
{
    /// <summary>
    /// Représente un WorkOrder enrichi avec les champs dérivés.
    /// </summary>
    public class WorkOrderResult : WorkOrder
    {
        // Champs dérivés
        public string AddressLabel { get; set; } = default!;
        public string Department { get; set; } = default!;
        public int DaysToPlan { get; set; }
        public string Priority { get; set; } = default!;

        public WorkOrderResult(
            string workOrderId,
            string meterSerial,
            string street,
            string postalCode,
            string city,
            DateTime plannedDate
        )
            : base(workOrderId, meterSerial, street, postalCode, city, plannedDate) { }

        /// <summary>
        /// Crée un WorkOrder enrichi à partir d'un WorkOrder.
        /// </summary>
        public static WorkOrderResult FromWorkOrder(WorkOrder wo)
        {
            var today = DateTime.Today;
            var res = new WorkOrderResult(
                wo.WorkOrderId,
                wo.MeterSerial,
                wo.Street,
                wo.PostalCode,
                wo.City,
                wo.PlannedDate
            );

            res.AddressLabel = $"{res.Street}, {res.PostalCode} {res.City}";

            // Le département est représenté par les deux premier chiffre du code postal
            res.Department = wo.PostalCode.Length >= 2 ? wo.PostalCode[..2] : wo.PostalCode;

            // DaysToPlan est le nombre de jour entre la date planifiée et la date actuelle
            res.DaysToPlan = Math.Abs((wo.PlannedDate.Date - today.Date).Days);

            if (res.DaysToPlan <= 1)
            {
                res.Priority = "P1";
            }
            else if (res.DaysToPlan is >= 2 and <= 3)
            {
                res.Priority = "P2";
            }
            else
            {
                res.Priority = "P3";
            }
            return res;
        }
    }
}
