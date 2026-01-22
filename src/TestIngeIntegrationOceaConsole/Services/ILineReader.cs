namespace TestIngeIntegrationOceaConsole.Services;

public interface ILineReader : IDisposable
{
    string? ReadLine();
}
