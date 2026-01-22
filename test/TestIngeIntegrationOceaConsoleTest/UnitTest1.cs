using Moq;
using TestIngeIntegrationOceaConsole.Services;

namespace TestIngeIntegrationOceaConsoleTest;

public class CsvServiceTests
{
    [Fact]
    public void ReadAndValidate_Success()
    {
        // Arrange
        var sequence = new Queue<string?>([
            "WorkOrderId;MeterSerial;Street;PostalCode;City;PlannedDate",
            "WO-2026-0101;MTR-69002-0101;12 Rue de la République;69002;Lyon;2026-01-22",
            "WO-2026-0102;MTR-69003-0102;85 Cours Lafayette;69003;Lyon;2026-01-23",
            "WO-2026-0103;MTR-69006-0103;14 Boulevard des Belges;69006;Lyon;2026-01-24",
            null,
        ]);

        var readerMock = new Mock<ILineReader>();
        readerMock.Setup(r => r.ReadLine()).Returns(() => sequence.Dequeue());

        var csvService = new CsvService(_ => readerMock.Object);

        // Act
        var result = csvService.ReadAndValidate("input.csv");

        // Assert
        Assert.Equal(3, result.ValidOrders.Count);
        Assert.Empty(result.Rejected);
    }

    [Fact]
    public void ReadAndValidate_Failed()
    {
        // Arrange
        var sequence = new Queue<string?>([
            "WorkOrderId;MeterSerial;Street;PostalCode;City;PlannedDate",
            "WO-2026-0101;;12 Rue de la République;69002;Lyon;2026-01-22",
            "WO-2026-0102;MTR-69003-0102;85 Cours Lafayette;6903;Lyon;202-01-23",
            "WO-2026-0103;MTR-69006-0103;14 Boulevard des Belges;69006;Lyon;2026-01-24",
            null,
        ]);

        var readerMock = new Mock<ILineReader>();
        readerMock.Setup(r => r.ReadLine()).Returns(() => sequence.Dequeue());

        var csvService = new CsvService(_ => readerMock.Object);

        // Act
        var (validOrders, rejectedLines) = csvService.ReadAndValidate("input.csv");

        // Assert
        Assert.Single(validOrders);
        Assert.Equal(2, rejectedLines.Count);
    }
}
