using System;
using System.IO;
using System.IO.Compression;
using System.Text;
using System.Threading.Tasks;

namespace Compressing
{
    public static class Compression
    {
        public static async Task<CompressionResult> ToGzipAsync(this string value, CompressionLevel level = CompressionLevel.Fastest)
        {
            var bytes = Encoding.Unicode.GetBytes(value);
            await using var input = new MemoryStream(bytes);
            await using var output = new MemoryStream();
            await using var stream = new GZipStream(output, level);

            await input.CopyToAsync(stream);
            
            var result = output.ToArray();

            return new CompressionResult(
                new(value, bytes.Length),
                new(Convert.ToBase64String(result), result.Length),
                level,
                "Gzip");
        }
        
        public static async Task<CompressionResult> ToBrotliAsync(this string value, CompressionLevel level = CompressionLevel.Fastest)
        {
            var bytes = Encoding.Unicode.GetBytes(value);
            await using var input = new MemoryStream(bytes);
            await using var output = new MemoryStream();
            await using var stream = new BrotliStream(output, level);

            await input.CopyToAsync(stream);
            
            var result = output.ToArray();

            return new CompressionResult(
                new(value, bytes.Length),
                new(Convert.ToBase64String(result), result.Length),
                level,
                "Brotli"
            );
        }
        
        public static async Task<CompressionResult> ToDeflateAsync(this string value, CompressionLevel level = CompressionLevel.Fastest)
        {
            var bytes = Encoding.Unicode.GetBytes(value);
            await using var input = new MemoryStream(bytes);
            await using var output = new MemoryStream();
            await using var stream = new DeflateStream(output, level);

            await input.CopyToAsync(stream);
            
            var result = output.ToArray();

            return new CompressionResult(
                new(value, bytes.Length),
                new(Convert.ToBase64String(result), result.Length),
                level,
                "Deflate"
            );
        }

        public static async Task<string> FromGzipAsync(this string value)
        {
            var bytes = Convert.FromBase64String(value);
            await using var input = new MemoryStream(bytes);
            await using var output = new MemoryStream();
            await using var stream = new GZipStream(input, CompressionMode.Decompress);

            await stream.CopyToAsync(output);
            
            return Encoding.Unicode.GetString(output.ToArray());
        }

        public static async Task<string> FromBrotliAsync(this string value)
        {
            var bytes = Convert.FromBase64String(value);
            await using var input = new MemoryStream(bytes);
            await using var output = new MemoryStream();
            await using var stream = new BrotliStream(input, CompressionMode.Decompress);

            await stream.CopyToAsync(output);
            
            return Encoding.Unicode.GetString(output.ToArray());
        }
        
        public static async Task<string> FromDeflateAsync(this string value)
        {
            var bytes = Convert.FromBase64String(value);
            await using var input = new MemoryStream(bytes);
            await using var output = new MemoryStream();
            await using var stream = new DeflateStream(input, CompressionMode.Decompress);

            await stream.CopyToAsync(output);
            
            return Encoding.Unicode.GetString(output.ToArray());
        }
        
    }

    public record CompressionResult(
        CompressionValue Original,
        CompressionValue Result,
        CompressionLevel Level,
        string Kind
    )
    {
        public int Difference =>
            Original.Size - Result.Size;

        public decimal Percent =>
          Math.Abs(Difference / (decimal) Original.Size);
    }

    public record CompressionValue(
        string Value,
        int Size
    );
}