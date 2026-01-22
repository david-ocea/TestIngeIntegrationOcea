using System.Text;

namespace TestIngeIntegrationOceaConsole.Services;

public class FileLineReader : ILineReader
{
    private readonly StreamReader _reader;

    public FileLineReader(string filePath)
    {
        _reader = new StreamReader(filePath, Encoding.UTF8, detectEncodingFromByteOrderMarks: true);
    }

    public string? ReadLine() => _reader.ReadLine();

    public void Dispose() => _reader.Dispose();
}
