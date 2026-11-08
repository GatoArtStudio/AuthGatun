using System;
using System.Collections.Generic;
using AuthGatun.Domains.IdentityAccess.Application;
using AuthGatun.Domains.IdentityAccess.Application.Ports.Outbound;
using SkiaSharp;
using ZXing;
using ZXing.Common;

namespace AuthGatun.Domains.IdentityAccess.Adapters.Outbound.QrCode;

public class ZXingQrCodeReader : IQrCodeReader
{
    private const int ReducedMaxSide = 1000;

    private static readonly BarcodeReaderGeneric Reader = new()
    {
        AutoRotate = true,
        Options = new DecodingOptions
        {
            TryHarder = true,
            TryInverted = true,
            PossibleFormats = new List<BarcodeFormat> { BarcodeFormat.QR_CODE }
        }
    };

    public string? Read(byte[] image)
    {
        using var codec = SKCodec.Create(new SKMemoryStream(image))
                          ?? throw new IdentityAccessException(IdentityAccessError.ImageUnreadable, "The image could not be decoded.");
        using var decoded = SKBitmap.Decode(codec)
                            ?? throw new IdentityAccessException(IdentityAccessError.ImageUnreadable, "The image could not be decoded.");

        var text = Decode(decoded);
        if (text is not null || Math.Max(decoded.Width, decoded.Height) <= ReducedMaxSide)
            return text;

        var scale = (double)ReducedMaxSide / Math.Max(decoded.Width, decoded.Height);
        var reducedInfo = new SKImageInfo(
            Math.Max(1, (int)(decoded.Width * scale)),
            Math.Max(1, (int)(decoded.Height * scale)),
            SKColorType.Bgra8888);

        using var reduced = decoded.Resize(reducedInfo, SKFilterQuality.High);
        return reduced is null ? null : Decode(reduced);
    }

    private static string? Decode(SKBitmap bitmap)
    {
        using var bgra = bitmap.ColorType == SKColorType.Bgra8888 ? null : bitmap.Copy(SKColorType.Bgra8888);
        var source = bgra ?? bitmap;

        var pixels = source.GetPixelSpan().ToArray();
        var luminance = new RGBLuminanceSource(pixels, source.Width, source.Height, RGBLuminanceSource.BitmapFormat.BGRA32);

        return Reader.Decode(luminance)?.Text;
    }
}
