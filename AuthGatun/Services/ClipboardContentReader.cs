using System;
using System.Buffers.Binary;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Input;
using Avalonia.Input.Platform;

namespace AuthGatun.Services;

public record ClipboardContent(byte[]? Image, string? Text);

public static class ClipboardContentReader
{
    private static readonly string[] ImageFormats =
    [
        "image/png", "PNG", "image/jpeg", "JFIF", "image/bmp", "image/x-bmp", "image/x-MS-bmp", "image/webp"
    ];

    private const string DibFormat = "DeviceIndependentBitmap";

    public static async Task<ClipboardContent> ReadAsync(IClipboard clipboard)
    {
        var image = await ReadImageAsync(clipboard);
        return new ClipboardContent(image, image is null ? await ReadTextAsync(clipboard) : null);
    }

    private static async Task<byte[]?> ReadImageAsync(IClipboard clipboard)
    {
        string[] available;
        try
        {
            available = await clipboard.GetFormatsAsync() ?? [];
        }
        catch (Exception)
        {
            return null;
        }

        if (available.Length == 0)
            return null;

        foreach (var format in ImageFormats.Where(candidate =>
                     available.Contains(candidate, StringComparer.OrdinalIgnoreCase)))
        {
            var bytes = await ReadBytesAsync(clipboard, format);
            if (bytes is { Length: > 0 })
                return bytes;
        }

        if (available.Contains(DibFormat, StringComparer.OrdinalIgnoreCase))
        {
            var dib = await ReadBytesAsync(clipboard, DibFormat);
            return dib is null ? null : DibToBmp(dib);
        }

        return null;
    }

    private static async Task<string?> ReadTextAsync(IClipboard clipboard)
    {
        try
        {
            return await clipboard.GetTextAsync();
        }
        catch (Exception)
        {
            return null;
        }
    }

    private static async Task<byte[]?> ReadBytesAsync(IClipboard clipboard, string format)
    {
        try
        {
            return await clipboard.GetDataAsync(format) switch
            {
                byte[] bytes => bytes,
                MemoryStream memory => memory.ToArray(),
                Stream stream => ReadAll(stream),
                _ => null
            };
        }
        catch (Exception)
        {
            return null;
        }
    }

    private static byte[] ReadAll(Stream stream)
    {
        using var memory = new MemoryStream();
        stream.CopyTo(memory);
        return memory.ToArray();
    }

    // A DIB is a BMP without the 14 byte file header; the header is rebuilt so it can be decoded as a normal image.
    private static byte[]? DibToBmp(byte[] dib)
    {
        if (dib.Length < 40)
            return null;

        var headerSize = BinaryPrimitives.ReadInt32LittleEndian(dib);
        var bitCount = BinaryPrimitives.ReadInt16LittleEndian(dib.AsSpan(14));
        var compression = BinaryPrimitives.ReadInt32LittleEndian(dib.AsSpan(16));
        var colorsUsed = BinaryPrimitives.ReadInt32LittleEndian(dib.AsSpan(32));

        var paletteSize = bitCount <= 8 ? (colorsUsed == 0 ? 1 << bitCount : colorsUsed) * 4 : colorsUsed * 4;
        var maskSize = compression == 3 && headerSize == 40 ? 12 : 0;
        var pixelOffset = 14 + headerSize + maskSize + paletteSize;

        var bmp = new byte[14 + dib.Length];
        bmp[0] = (byte)'B';
        bmp[1] = (byte)'M';
        BinaryPrimitives.WriteInt32LittleEndian(bmp.AsSpan(2), bmp.Length);
        BinaryPrimitives.WriteInt32LittleEndian(bmp.AsSpan(10), pixelOffset);
        dib.CopyTo(bmp, 14);
        return bmp;
    }
}
