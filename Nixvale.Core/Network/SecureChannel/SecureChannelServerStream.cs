using System.Security.Cryptography;

namespace Nixvale.Core.Network.SecureChannel;

/// <summary>
/// Server implementation of secure channel communication
/// </summary>
public class SecureChannelServerStream : SecureChannelStream
{
    private readonly ICryptoTransform _encryptor;
    private readonly ICryptoTransform _decryptor;
    private readonly byte[] _buffer = new byte[4096];
    private int _bufferOffset;
    private int _bufferCount;

    public SecureChannelServerStream(Stream baseStream, byte[] privateKey, byte[] clientPublicKey)
        : base(baseStream, ComputeSharedSecret(privateKey, clientPublicKey))
    {
        // Read IV from client
        var bytesRead = _baseStream.Read(_iv, 0, _iv.Length);
        if (bytesRead != _iv.Length)
            throw new SecureChannelException("Failed to read initialization vector");

        // Create encryptor/decryptor
        _encryptor = _aes.CreateEncryptor();
        _decryptor = _aes.CreateDecryptor();
    }

    private static byte[] ComputeSharedSecret(byte[] privateKey, byte[] clientPublicKey)
    {
        using var ecdh = ECDiffieHellman.Create();
        ecdh.ImportPkcs8PrivateKey(privateKey, out _);
        
        using var clientKey = ECDiffieHellman.Create();
        clientKey.ImportSubjectPublicKeyInfo(clientPublicKey, out _);
        
        return ecdh.DeriveKeyMaterial(clientKey.PublicKey);
    }

    public override int Read(byte[] buffer, int offset, int count)
    {
        if (_bufferCount == 0)
        {
            // Read encrypted block size
            var sizeBuffer = new byte[4];
            var bytesRead = _baseStream.Read(sizeBuffer, 0, 4);
            if (bytesRead == 0) return 0;
            if (bytesRead != 4) throw new SecureChannelException("Invalid encrypted block");

            var blockSize = BitConverter.ToInt32(sizeBuffer);
            if (blockSize <= 0 || blockSize > 1024 * 1024) // Max 1MB
                throw new SecureChannelException("Invalid block size");

            // Read encrypted block
            var encryptedBlock = new byte[blockSize];
            bytesRead = _baseStream.Read(encryptedBlock, 0, blockSize);
            if (bytesRead != blockSize)
                throw new SecureChannelException("Incomplete encrypted block");

            // Decrypt
            _bufferCount = _decryptor.TransformBlock(
                encryptedBlock, 0, blockSize,
                _buffer, 0);
            _bufferOffset = 0;
        }

        var toCopy = Math.Min(count, _bufferCount - _bufferOffset);
        Buffer.BlockCopy(_buffer, _bufferOffset, buffer, offset, toCopy);
        _bufferOffset += toCopy;
        _bufferCount -= toCopy;
        return toCopy;
    }

    public override void Write(byte[] buffer, int offset, int count)
    {
        // Encrypt data
        var encrypted = new byte[count + 32]; // Extra space for padding
        var encryptedCount = _encryptor.TransformBlock(
            buffer, offset, count,
            encrypted, 0);

        // Write size and encrypted data
        var sizeBuffer = BitConverter.GetBytes(encryptedCount);
        _baseStream.Write(sizeBuffer, 0, 4);
        _baseStream.Write(encrypted, 0, encryptedCount);
    }
} 