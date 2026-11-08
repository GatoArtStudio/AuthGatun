using System;

namespace AuthGatun.Domains.IdentityAccess.Domain.ValueObject;

public enum OtpAuthUriProblem
{
    None,
    NotOtpAuth,
    InvalidSecret,
    UnsupportedParameters
}

public sealed record OtpAuthUri(string ServiceName, string SecretKey)
{
    private const string Scheme = "otpauth://";
    private const string DefaultServiceName = "TOTP";
    private const string SupportedAlgorithm = "SHA1";
    private const string SupportedDigits = "6";
    private const string SupportedPeriod = "30";

    public static OtpAuthUriProblem TryParse(string? text, out OtpAuthUri? uri)
    {
        uri = null;

        var value = text?.Trim();
        if (string.IsNullOrEmpty(value) || !value.StartsWith(Scheme, StringComparison.OrdinalIgnoreCase))
            return OtpAuthUriProblem.NotOtpAuth;

        var rest = value[Scheme.Length..];
        var queryStart = rest.IndexOf('?');
        var path = queryStart < 0 ? rest : rest[..queryStart];
        var query = queryStart < 0 ? "" : rest[(queryStart + 1)..];

        var labelStart = path.IndexOf('/');
        var type = labelStart < 0 ? path : path[..labelStart];
        var label = labelStart < 0 ? "" : Unescape(path[(labelStart + 1)..]);

        if (!type.Equals("totp", StringComparison.OrdinalIgnoreCase))
            return type.Equals("hotp", StringComparison.OrdinalIgnoreCase)
                ? OtpAuthUriProblem.UnsupportedParameters
                : OtpAuthUriProblem.NotOtpAuth;

        string? secret = null, issuer = null, algorithm = null, digits = null, period = null;
        foreach (var pair in query.Split('&', StringSplitOptions.RemoveEmptyEntries))
        {
            var separator = pair.IndexOf('=');
            if (separator < 0)
                continue;

            var name = pair[..separator].ToLowerInvariant();
            var parameter = Unescape(pair[(separator + 1)..]);
            switch (name)
            {
                case "secret": secret = parameter; break;
                case "issuer": issuer = parameter; break;
                case "algorithm": algorithm = parameter; break;
                case "digits": digits = parameter; break;
                case "period": period = parameter; break;
            }
        }

        var normalizedSecret = NormalizeSecret(secret);
        if (normalizedSecret is null)
            return OtpAuthUriProblem.InvalidSecret;

        if (!IsDefault(algorithm, SupportedAlgorithm) || !IsDefault(digits, SupportedDigits) || !IsDefault(period, SupportedPeriod))
            return OtpAuthUriProblem.UnsupportedParameters;

        uri = new OtpAuthUri(ResolveServiceName(issuer, label), normalizedSecret);
        return OtpAuthUriProblem.None;
    }

    private static bool IsDefault(string? value, string expected) =>
        string.IsNullOrWhiteSpace(value) || value.Trim().Equals(expected, StringComparison.OrdinalIgnoreCase);

    private static string ResolveServiceName(string? issuer, string label)
    {
        if (!string.IsNullOrWhiteSpace(issuer))
            return issuer.Trim();

        var separator = label.IndexOf(':');
        var name = separator > 0 ? label[..separator] : label;
        return string.IsNullOrWhiteSpace(name) ? DefaultServiceName : name.Trim();
    }

    private static string? NormalizeSecret(string? secret)
    {
        if (string.IsNullOrWhiteSpace(secret))
            return null;

        var normalized = secret.Replace(" ", "").Replace("-", "").ToUpperInvariant().TrimEnd('=');
        if (normalized.Length == 0)
            return null;

        foreach (var character in normalized)
        {
            if (character is not ((>= 'A' and <= 'Z') or (>= '2' and <= '7')))
                return null;
        }

        return normalized;
    }

    private static string Unescape(string value)
    {
        try
        {
            return Uri.UnescapeDataString(value);
        }
        catch (UriFormatException)
        {
            return value;
        }
    }
}
