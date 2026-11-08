using System;
using AuthGatun.Domains.IdentityAccess.Adapters.Outbound.QrCode;
using AuthGatun.Domains.IdentityAccess.Application;
using SkiaSharp;
using ZXing;
using ZXing.QrCode;
using Xunit;

namespace AuthGatun.Tests.Domains.IdentityAccess.Adapters.Outbound;

public class ZXingQrCodeReaderTest
{
    private const string Link = "otpauth://totp/GitHub:gato?secret=JBSWY3DPEHPK3PXP&issuer=GitHub";

    private static byte[] QrPng(string contents, int size, SKEncodedImageFormat format = SKEncodedImageFormat.Png, int margin = 4)
    {
        var matrix = new QRCodeWriter().encode(
            contents,
            BarcodeFormat.QR_CODE,
            size,
            size,
            new System.Collections.Generic.Dictionary<EncodeHintType, object> { [EncodeHintType.MARGIN] = margin });

        using var bitmap = new SKBitmap(matrix.Width, matrix.Height);
        for (var y = 0; y < matrix.Height; y++)
            for (var x = 0; x < matrix.Width; x++)
                bitmap.SetPixel(x, y, matrix[x, y] ? SKColors.Black : SKColors.White);

        using var image = SKImage.FromBitmap(bitmap);
        return image.Encode(format, 100).ToArray();
    }

    [Theory]
    [InlineData(SKEncodedImageFormat.Png)]
    [InlineData(SKEncodedImageFormat.Jpeg)]
    public void ReadsTheTextOfAQrImage(SKEncodedImageFormat format)
    {
        Assert.Equal(Link, new ZXingQrCodeReader().Read(QrPng(Link, 300, format)));
    }

    [Fact]
    public void ReadsALargeScreenshotLikeImage()
    {
        Assert.Equal(Link, new ZXingQrCodeReader().Read(QrPng(Link, 2400)));
    }

    [Fact]
    public void ReturnsNullWhenTheImageHasNoQr()
    {
        using var bitmap = new SKBitmap(200, 200);
        bitmap.Erase(SKColors.White);
        using var image = SKImage.FromBitmap(bitmap);

        Assert.Null(new ZXingQrCodeReader().Read(image.Encode(SKEncodedImageFormat.Png, 100).ToArray()));
    }

    [Fact]
    public void ReportsBytesThatAreNotAnImage()
    {
        var error = Assert.Throws<IdentityAccessException>(() => new ZXingQrCodeReader().Read([1, 2, 3, 4]));

        Assert.Equal(IdentityAccessError.ImageUnreadable, error.Error);
    }
}
