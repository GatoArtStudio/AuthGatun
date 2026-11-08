using System;
using System.Collections.Generic;
using System.Text;

namespace AuthGatun.Domains.IdentityAccess.Domain.ValueObject;

public enum OtpMigrationProblem
{
    None,
    NotMigration,
    InvalidData
}

public sealed record OtpMigrationEntry(string ServiceName, string SecretKey, TotpParameters Parameters);

public sealed record OtpMigrationUri(IReadOnlyList<OtpMigrationEntry> Entries, int Unsupported)
{
    private const string Scheme = "otpauth-migration://";
    private const string DefaultServiceName = "TOTP";
    private const string Base32Alphabet = "ABCDEFGHIJKLMNOPQRSTUVWXYZ234567";

    private const int AlgorithmSha1 = 1;
    private const int AlgorithmSha256 = 2;
    private const int AlgorithmSha512 = 3;
    private const int DigitsSix = 1;
    private const int DigitsEight = 2;
    private const int TypeTotp = 2;

    public static bool IsMigration(string? text) =>
        text is not null && text.TrimStart().StartsWith(Scheme, StringComparison.OrdinalIgnoreCase);

    public static OtpMigrationProblem TryParse(string? text, out OtpMigrationUri? migration)
    {
        migration = null;
        var value = text?.Trim();
        if (string.IsNullOrEmpty(value) || !value.StartsWith(Scheme, StringComparison.OrdinalIgnoreCase))
            return OtpMigrationProblem.NotMigration;

        var queryStart = value.IndexOf('?');
        if (queryStart < 0)
            return OtpMigrationProblem.InvalidData;

        string? data = null;
        foreach (var pair in value[(queryStart + 1)..].Split('&', StringSplitOptions.RemoveEmptyEntries))
        {
            var separator = pair.IndexOf('=');
            if (separator > 0 && pair[..separator].Equals("data", StringComparison.OrdinalIgnoreCase))
                data = pair[(separator + 1)..];
        }

        var bytes = DecodeBase64(data);
        if (bytes is null)
            return OtpMigrationProblem.InvalidData;

        var entries = new List<OtpMigrationEntry>();
        var unsupported = 0;
        try
        {
            var payload = new ProtobufReader(bytes);
            while (payload.TryReadTag(out var field, out var wireType))
            {
                if (field != 1 || wireType != ProtobufReader.LengthDelimited)
                {
                    payload.Skip(wireType);
                    continue;
                }

                var entry = ReadEntry(payload.ReadBytes());
                if (entry is null)
                    unsupported++;
                else
                    entries.Add(entry);
            }
        }
        catch (FormatException)
        {
            return OtpMigrationProblem.InvalidData;
        }

        if (entries.Count == 0 && unsupported == 0)
            return OtpMigrationProblem.InvalidData;

        migration = new OtpMigrationUri(entries, unsupported);
        return OtpMigrationProblem.None;
    }

    private static OtpMigrationEntry? ReadEntry(byte[] bytes)
    {
        var reader = new ProtobufReader(bytes);
        byte[] secret = [];
        string name = "", issuer = "";
        long algorithm = 0, digits = 0, type = 0;

        while (reader.TryReadTag(out var field, out var wireType))
        {
            switch (field)
            {
                case 1 when wireType == ProtobufReader.LengthDelimited: secret = reader.ReadBytes(); break;
                case 2 when wireType == ProtobufReader.LengthDelimited: name = Encoding.UTF8.GetString(reader.ReadBytes()); break;
                case 3 when wireType == ProtobufReader.LengthDelimited: issuer = Encoding.UTF8.GetString(reader.ReadBytes()); break;
                case 4 when wireType == ProtobufReader.Varint: algorithm = reader.ReadVarint(); break;
                case 5 when wireType == ProtobufReader.Varint: digits = reader.ReadVarint(); break;
                case 6 when wireType == ProtobufReader.Varint: type = reader.ReadVarint(); break;
                default: reader.Skip(wireType); break;
            }
        }

        if (secret.Length == 0 || type != TypeTotp)
            return null;

        var parsedAlgorithm = algorithm switch
        {
            0 or AlgorithmSha1 => TotpAlgorithm.Sha1,
            AlgorithmSha256 => TotpAlgorithm.Sha256,
            AlgorithmSha512 => TotpAlgorithm.Sha512,
            _ => (TotpAlgorithm?)null
        };
        var parsedDigits = digits switch
        {
            0 or DigitsSix => TotpParameters.MinDigits,
            DigitsEight => 8,
            _ => 0
        };
        if (parsedAlgorithm is null || parsedDigits == 0)
            return null;

        var serviceName = string.IsNullOrWhiteSpace(issuer)
            ? ResolveFromName(name)
            : issuer.Trim();

        return new OtpMigrationEntry(
            serviceName,
            EncodeBase32(secret),
            new TotpParameters(parsedAlgorithm.Value, parsedDigits, TotpParameters.Default.PeriodSeconds));
    }

    private static string ResolveFromName(string name)
    {
        var separator = name.IndexOf(':');
        var service = separator > 0 ? name[..separator] : name;
        return string.IsNullOrWhiteSpace(service) ? DefaultServiceName : service.Trim();
    }

    private static byte[]? DecodeBase64(string? data)
    {
        if (string.IsNullOrWhiteSpace(data))
            return null;

        string unescaped;
        try
        {
            unescaped = Uri.UnescapeDataString(data);
        }
        catch (UriFormatException)
        {
            return null;
        }

        var normalized = unescaped.Replace(' ', '+').Replace('-', '+').Replace('_', '/').TrimEnd('=');
        normalized = normalized.PadRight(normalized.Length + (4 - normalized.Length % 4) % 4, '=');
        try
        {
            return Convert.FromBase64String(normalized);
        }
        catch (FormatException)
        {
            return null;
        }
    }

    internal static string EncodeBase32(byte[] bytes)
    {
        var result = new StringBuilder((bytes.Length * 8 + 4) / 5);
        var buffer = 0;
        var bits = 0;
        foreach (var current in bytes)
        {
            buffer = (buffer << 8) | current;
            bits += 8;
            while (bits >= 5)
            {
                result.Append(Base32Alphabet[(buffer >> (bits - 5)) & 31]);
                bits -= 5;
            }

            buffer &= (1 << bits) - 1;
        }

        if (bits > 0)
            result.Append(Base32Alphabet[(buffer << (5 - bits)) & 31]);

        return result.ToString();
    }

    private sealed class ProtobufReader(byte[] data)
    {
        public const int Varint = 0;
        public const int Fixed64 = 1;
        public const int LengthDelimited = 2;
        public const int Fixed32 = 5;

        private int _position;

        public bool TryReadTag(out int field, out int wireType)
        {
            field = 0;
            wireType = 0;
            if (_position >= data.Length)
                return false;

            var tag = ReadVarint();
            field = (int)(tag >> 3);
            wireType = (int)(tag & 7);
            if (field <= 0)
                throw new FormatException("Invalid protobuf field number.");
            return true;
        }

        public long ReadVarint()
        {
            long result = 0;
            for (var shift = 0; shift < 64; shift += 7)
            {
                if (_position >= data.Length)
                    throw new FormatException("Truncated protobuf varint.");

                var current = data[_position++];
                result |= (long)(current & 0x7F) << shift;
                if ((current & 0x80) == 0)
                    return result;
            }

            throw new FormatException("Protobuf varint is too long.");
        }

        public byte[] ReadBytes()
        {
            var length = ReadVarint();
            if (length < 0 || length > data.Length - _position)
                throw new FormatException("Truncated protobuf field.");

            var result = data[_position..(_position + (int)length)];
            _position += (int)length;
            return result;
        }

        public void Skip(int wireType)
        {
            switch (wireType)
            {
                case Varint: ReadVarint(); break;
                case Fixed64: Advance(8); break;
                case LengthDelimited: ReadBytes(); break;
                case Fixed32: Advance(4); break;
                default: throw new FormatException("Unsupported protobuf wire type.");
            }
        }

        private void Advance(int count)
        {
            if (count > data.Length - _position)
                throw new FormatException("Truncated protobuf field.");
            _position += count;
        }
    }
}
