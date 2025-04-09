using System.Security.Cryptography;

namespace Nixvale.Core.Network.SecureChannel;

/// <summary>
/// Base class for secure channel communication
/// </summary>
public abstract class SecureChannelStream : Stream
{
    protected readonly Stream _baseStream;
    protected readonly byte[] _sharedSecret;
    protected readonly byte[] _iv;
    protected readonly Aes _aes;
    protected bool _isDisposed;

    protected SecureChannelStream(Stream baseStream, byte[] sharedSecret)
    {
        _baseStream = baseStream;
        _sharedSecret = sharedSecret;
        
        // Initialize AES
        _aes = Aes.Create();
        _aes.KeySize = 256;
        _aes.Mode = CipherMode.CBC;
        _aes.Padding = PaddingMode.PKCS7;
        
        // Generate IV
        _iv = new byte[_aes.BlockSize / 8];
        RandomNumberGenerator.Fill(_iv);
        
        // Derive key from shared secret using PBKDF2
        using var deriveBytes = new Rfc2898DeriveBytes(_sharedSecret, _iv, 10000, HashAlgorithmName.SHA256);
        _aes.Key = deriveBytes.GetBytes(_aes.KeySize / 8);
    }

    public override bool CanRead => _baseStream.CanRead;
    public override bool CanSeek => false;
    public override bool CanWrite => _baseStream.CanWrite;
    public override long Length => throw new NotSupportedException();
    public override long Position
    {
        get => throw new NotSupportedException();
        set => throw new NotSupportedException();
    }

    public override void Flush() => _baseStream.Flush();

    public override long Seek(long offset, SeekOrigin origin) => throw new NotSupportedException();

    public override void SetLength(long value) => throw new NotSupportedException();

    protected override void Dispose(bool disposing)
    {
        if (!_isDisposed)
        {
            if (disposing)
            {
                _aes.Dispose();
                _baseStream.Dispose();
            }

            _isDisposed = true;
        }

        base.Dispose(disposing);
    }

    /// <summary>
    /// Generates a new random user ID
    /// </summary>
    public static byte[] GenerateUserId()
    {
        var id = new byte[32];
        RandomNumberGenerator.Fill(id);
        return id;
    }

    /// <summary>
    /// Derives a public key from a private key
    /// </summary>
    public static byte[] GetPublicKeyFromPrivateKey(byte[] privateKey)
    {
        using var ecdh = ECDiffieHellman.Create();
        ecdh.ImportPkcs8PrivateKey(privateKey, out _);
        return ecdh.PublicKey.ExportSubjectPublicKeyInfo();
    }

    /// <summary>
    /// Generates a new ECDH key pair
    /// </summary>
    public static (byte[] privateKey, byte[] publicKey) GenerateKeyPair()
    {
        using var ecdh = ECDiffieHellman.Create();
        return (
            ecdh.ExportPkcs8PrivateKey(),
            ecdh.PublicKey.ExportSubjectPublicKeyInfo()
        );
    }
} 