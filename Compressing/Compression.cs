using System;
using System.IO;
using System.IO.Compression;
using System.Text;
using System.Threading.Tasks;

namespace Compressing
{
    public static class Compression
    {
        private static async Task<CompressionResult> ToCompressedStringAsync(string value,
                                                                             CompressionLevel level,
                                                                             string algorithm,
                                                                             Func<Stream, Stream> createCompressionStream)
        {
            var bytes = Encoding.Unicode.GetBytes(value);
            await using var input = new MemoryStream(bytes);
            await using var output = new MemoryStream();
            await using var stream = createCompressionStream(output);

            await input.CopyToAsync(stream);
            await stream.FlushAsync();

            var result = output.ToArray();

            return new CompressionResult(
                new(value, bytes.Length),
                new(Convert.ToBase64String(result), result.Length),
                level,
                algorithm);
        }

        public static async Task<CompressionResult> ToGzipAsync(this string value, CompressionLevel level = CompressionLevel.Fastest)
            => await ToCompressedStringAsync(value, level, "GZip", s => new GZipStream(s, level));
        
        public static async Task<CompressionResult> ToBrotliAsync(this string value, CompressionLevel level = CompressionLevel.Fastest)
            => await ToCompressedStringAsync(value, level, "Brotli", s => new BrotliStream(s, level));

        private static async Task<string> FromCompressedStringAsync(string value, Func<Stream, Stream> createDecompressionStream)
        {
            var bytes = Convert.FromBase64String(value);
            await using var input = new MemoryStream(bytes);
            await using var output = new MemoryStream();
            await using var stream = createDecompressionStream(input);

            await stream.CopyToAsync(output);
            await output.FlushAsync();

            return Encoding.Unicode.GetString(output.ToArray());
        }

        public static async Task<string> FromGzipAsync(this string value)
            => await FromCompressedStringAsync(value, s => new GZipStream(s, CompressionMode.Decompress));
 
        public static async Task<string> FromBrotliAsync(this string value)
            => await FromCompressedStringAsync(value, s => new BrotliStream(s, CompressionMode.Decompress));
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