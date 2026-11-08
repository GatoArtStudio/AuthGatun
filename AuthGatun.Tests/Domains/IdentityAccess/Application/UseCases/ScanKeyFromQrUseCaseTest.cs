using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AuthGatun.Domains.IdentityAccess.Application;
using AuthGatun.Domains.IdentityAccess.Application.Models;
using AuthGatun.Domains.IdentityAccess.Application.Ports.Outbound;
using AuthGatun.Domains.IdentityAccess.Application.UseCases;
using Xunit;

namespace AuthGatun.Tests.Domains.IdentityAccess.Application.UseCases;

public class ScanKeyFromQrUseCaseTest
{
    private sealed class FakeQrCodeReader(string? text) : IQrCodeReader
    {
        public string? Read(byte[] image) => text;
    }

    private static ScanKeyFromQrUseCase UseCase(string? qrText) => new(new FakeQrCodeReader(qrText));

    [Fact]
    public void ScanImageReturnsTheServiceAndTheSecretOfTheQr()
    {
        var key = UseCase("otpauth://totp/GitHub:gato?secret=JBSWY3DPEHPK3PXP&issuer=GitHub").ScanImage([1, 2, 3]);

        Assert.Equal(new ScannedKeyModel("GitHub", "JBSWY3DPEHPK3PXP", TotpParametersModel.Default), key);
    }

    [Fact]
    public void ScanImageReportsAnImageWithoutQr()
    {
        var error = Assert.Throws<IdentityAccessException>(() => UseCase(null).ScanImage([1]));

        Assert.Equal(IdentityAccessError.QrNotFound, error.Error);
    }

    [Fact]
    public void ParseLinkKeepsTheNonDefaultParameters()
    {
        var key = UseCase(null).ParseLink("otpauth://totp/A:b?secret=JBSWY3DPEHPK3PXP&algorithm=SHA256&digits=8&period=60");

        Assert.Equal(new TotpParametersModel("SHA256", 8, 60), key.Parameters);
    }

    [Fact]
    public void ParseLinkForKeysReadsEveryKeyOfAMigrationAndCountsUnsupportedOnes()
    {
        var link = MigrationLink(
            Entry([1, 2, 3, 4, 5], "Acme:ana", "Acme", algorithm: 2, digits: 2, type: 2),
            Entry([9, 9, 9], "Plain:bob", "", algorithm: 1, digits: 1, type: 2),
            Entry([7, 7], "Counter:eve", "Counter", algorithm: 1, digits: 1, type: 1),
            Entry([8, 8], "Old:max", "Old", algorithm: 4, digits: 1, type: 2));

        var result = UseCase(null).ParseLinkForKeys(link);

        Assert.Equal(2, result.Unsupported);
        Assert.Equal(2, result.Keys.Count);
        Assert.Equal(new ScannedKeyModel("Acme", "AEBAGBAF", new TotpParametersModel("SHA256", 8, 30)), result.Keys[0]);
        Assert.Equal("Plain", result.Keys[1].ServiceName);
        Assert.Equal("BEEQS", result.Keys[1].SecretKey);
        Assert.Equal(TotpParametersModel.Default, result.Keys[1].Parameters);
    }

    [Fact]
    public void ScanImageForKeysReadsAMigrationQr()
    {
        var link = MigrationLink(Entry([1, 2, 3, 4, 5], "Acme:ana", "Acme", 1, 1, 2));

        var result = UseCase(link).ScanImageForKeys([1]);

        Assert.Single(result.Keys);
    }

    [Fact]
    public void MigrationWithOnlyUnsupportedEntriesIsReported()
    {
        var link = MigrationLink(Entry([7, 7], "Counter:eve", "Counter", 1, 1, 1));

        var error = Assert.Throws<IdentityAccessException>(() => UseCase(null).ParseLinkForKeys(link));

        Assert.Equal(IdentityAccessError.MigrationNoSupportedKeys, error.Error);
    }

    [Theory]
    [InlineData("otpauth-migration://offline")]
    [InlineData("otpauth-migration://offline?data=")]
    [InlineData("otpauth-migration://offline?data=CgM")]
    [InlineData("otpauth-migration://offline?data=AAAA")]
    public void MalformedMigrationDataIsReported(string link)
    {
        var error = Assert.Throws<IdentityAccessException>(() => UseCase(null).ParseLinkForKeys(link));

        Assert.Equal(IdentityAccessError.MigrationInvalidData, error.Error);
    }

    [Fact]
    public void UnknownProtobufFieldsAreSkipped()
    {
        byte[] extra = [.. Varint(9 << 3), 7, .. Bytes(10, [1, 2, 3]), .. Varint((11 << 3) | 5), 1, 2, 3, 4];
        var entry = Entry([1, 2, 3, 4, 5], "Acme:ana", "Acme", 1, 1, 2, extra);
        var link = MigrationLink(entry);

        Assert.Single(UseCase(null).ParseLinkForKeys(link).Keys);
    }

    private static byte[] Varint(long value)
    {
        var bytes = new List<byte>();
        do
        {
            var current = (byte)(value & 0x7F);
            value >>= 7;
            bytes.Add(value > 0 ? (byte)(current | 0x80) : current);
        } while (value > 0);
        return [.. bytes];
    }

    private static byte[] Bytes(int field, byte[] value) =>
        [.. Varint((field << 3) | 2), .. Varint(value.Length), .. value];

    private static byte[] Entry(byte[] secret, string name, string issuer, int algorithm, int digits, int type, byte[]? extra = null) =>
    [
        .. Bytes(1, secret),
        .. Bytes(2, Encoding.UTF8.GetBytes(name)),
        .. Bytes(3, Encoding.UTF8.GetBytes(issuer)),
        .. Varint(4 << 3), .. Varint(algorithm),
        .. Varint(5 << 3), .. Varint(digits),
        .. Varint(6 << 3), .. Varint(type),
        .. extra ?? []
    ];

    private static string MigrationLink(params byte[][] entries)
    {
        var payload = entries.SelectMany(entry => Bytes(1, entry)).Concat([.. Varint(2 << 3), (byte)1]).ToArray();
        return "otpauth-migration://offline?data=" + Uri.EscapeDataString(Convert.ToBase64String(payload));
    }

    [Theory]
    [InlineData("https://example.com", IdentityAccessError.QrNotOtpAuth)]
    [InlineData("otpauth://totp/A:b?secret=", IdentityAccessError.QrInvalidSecret)]
    [InlineData("otpauth://totp/A:b?secret=JBSWY3DPEHPK3PXP&digits=9", IdentityAccessError.QrUnsupportedParameters)]
    [InlineData("otpauth://totp/A:b?secret=JBSWY3DPEHPK3PXP&algorithm=MD5", IdentityAccessError.QrUnsupportedParameters)]
    [InlineData("otpauth-migration://offline?data=%%%", IdentityAccessError.MigrationInvalidData)]
    public void ParseLinkMapsEachProblemToAnError(string text, IdentityAccessError expected)
    {
        var error = Assert.Throws<IdentityAccessException>(() => UseCase(null).ParseLink(text));

        Assert.Equal(expected, error.Error);
    }
}
