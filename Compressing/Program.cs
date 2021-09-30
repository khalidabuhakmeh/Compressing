using System;
using System.IO;
using System.IO.Compression;
using System.Text;
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
            await comedyOfErrors.ToGzipAsync(CompressionLevel.Fastest),
            await comedyOfErrors.ToGzipAsync(CompressionLevel.Optimal),
            await comedyOfErrors.ToBrotliAsync(CompressionLevel.Fastest),
            await comedyOfErrors.ToBrotliAsync(CompressionLevel.Optimal),
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

        // Use BrotliEncoder directly
        BrotliEncoderDecoderUsage(comedyOfErrors);
    }

    public static void BrotliEncoderDecoderUsage(string comedyOfErrors)
    {
        var source = Encoding.Unicode.GetBytes(comedyOfErrors);
        var memory = new byte[source.Length];
        var encoded = BrotliEncoder.TryCompress(
            source,
            memory,
            out var encodedBytes
        );

        Console.WriteLine($"compress bytes: {encodedBytes}");
        
        
        var target = new byte[memory.Length];
        BrotliDecoder.TryDecompress(memory, target, out var decodedBytes);
        Console.WriteLine($"decompress bytes: {decodedBytes}");

        var value = Encoding.Unicode.GetString(target);
    }
}