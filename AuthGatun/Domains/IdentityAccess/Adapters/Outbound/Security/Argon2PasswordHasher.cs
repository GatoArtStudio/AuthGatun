using System;
using System.Security.Cryptography;
using System.Text;
using AuthGatun.Domains.IdentityAccess.Application.Ports.Out;
using Konscious.Security.Cryptography;

namespace AuthGatun.Domains.IdentityAccess.Infrastructure.Services;

public class Argon2PasswordHasher : IPasswordHasher
{
    private const int SaltSize = 16;
    private const int HashSize = 36;
    private const int Iterations = 4;
    private const int MemorySize = 65536;
    private const int DegreeOfParallelism = 1;
    
    public string Hash(string password)
    {
        byte[] salt = GenerateSalt(SaltSize);
        byte[] passwordBytes = Encoding.UTF8.GetBytes(password);
        
        var argon2 = new Argon2id(passwordBytes)
        {
            Salt = salt,
            DegreeOfParallelism = DegreeOfParallelism,
            MemorySize = MemorySize,
            Iterations = Iterations
        };

        byte[] hashBytes = argon2.GetBytes(HashSize);
        return Convert.ToBase64String(Combine(salt, hashBytes));
    }

    public bool Verify(string password, string hashedPassword)
    {
        byte[] FullHash = Convert.FromBase64String(hashedPassword);
        byte[] salt = FullHash[..SaltSize];
        byte[] originalHash = FullHash[SaltSize..];

        var argon2 = new Argon2id(Encoding.UTF8.GetBytes(password))
        {
            Salt = salt,
            DegreeOfParallelism = DegreeOfParallelism,
            MemorySize = MemorySize,
            Iterations = Iterations
        };

        byte[] newHash = argon2.GetBytes(HashSize);
        return CryptographicOperations.FixedTimeEquals(newHash, originalHash);
    }

    private static byte[] GenerateSalt(int length)
    {
        byte[] salt = new byte[length];
        RandomNumberGenerator.Fill(salt);
        return salt;
    }

    private static byte[] Combine(byte[] first, byte[] second)
    {
        byte[] result = new byte[first.Length + second.Length];
        Buffer.BlockCopy(first, 0, result, 0, first.Length);
        Buffer.BlockCopy(second, 0, result, first.Length, second.Length);
        return result;
    }
}