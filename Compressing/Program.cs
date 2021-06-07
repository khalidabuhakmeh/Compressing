using System.IO;
using System.Threading.Tasks;
using Compressing;
using Spectre.Console;

public class Program
{
    public static async Task Main(string[] args = null)
    {
        var comedyOfErrors = await File.ReadAllTextAsync("the-comedy-of-errors.txt");

        var compressions = new[]
        {
            await comedyOfErrors.ToGzipAsync(),
            await comedyOfErrors.ToBrotliAsync(),
            await comedyOfErrors.ToDeflateAsync(),
        };

        var table = new Table()
            .Title("compression in bytes")
            .ShowHeaders()
            .AddColumns("kind", "level", "before", "after", "difference", "% reduction");
    
        foreach (var result in compressions)
        {
            table
                .AddRow(
                    result.Kind, 
                    result.Level.ToString(),
                    result.Original.Size.ToString("N0"),
                    result.Result.Size.ToString("N0"), 
                    result.Difference.ToString("N0"), 
                    result.Percent.ToString("P")
                );
        }

        AnsiConsole.Render(table);
    }
}
