using System.Security.Cryptography;

namespace Nixvale.Core.Network.SecureChannel;

/// <summary>
/// Client implementation of secure channel communication
/// </summary>
public class SecureChannelClientStream : SecureChannelStream
{
    private readonly ICryptoTransform _encryptor;
    private readonly ICryptoTransform _decryptor;
    private readonly byte[] _buffer = new byte[4096];
    private int _bufferOffset;
    private int _bufferCount;

    public SecureChannelClientStream(Stream baseStream, byte[] privateKey, byte[] serverPublicKey)
        : base(baseStream, ComputeSharedSecret(privateKey, serverPublicKey))
    {
        // Send IV to server
        _baseStream.Write(_iv, 0, _iv.Length);

        // Create encryptor/decryptor
        _encryptor = _aes.CreateEncryptor();
        _decryptor = _aes.CreateDecryptor();
    }

    private static byte[] ComputeSharedSecret(byte[] privateKey, byte[] serverPublicKey)
    {
        using var ecdh = ECDiffieHellman.Create();
        ecdh.ImportPkcs8PrivateKey(privateKey, out _);
        
        using var serverKey = ECDiffieHellman.Create();
        serverKey.ImportSubjectPublicKeyInfo(serverPublicKey, out _);
        
        return ecdh.DeriveKeyMaterial(serverKey.PublicKey);
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