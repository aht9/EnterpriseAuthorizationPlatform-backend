using System.Buffers;
using System.Buffers.Text;
using System.ComponentModel.DataAnnotations;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using IdentityService.Domain.Aggregates.Session;
using IdentityService.Domain.Aggregates.User;
using IdentityService.Domain.Services;
using Microsoft.Extensions.Options;

namespace IdentityService.Infrastructure.Crypto;

public sealed class JwtOptions
{
    [Required]
    public string Issuer { get; init; } = "enterprise-auth-platform";

    [Required]
    public string Audience { get; init; } = "enterprise-services";

    [Required]
    [MinLength(32)]
    public string SigningKey { get; init; } = "development-signing-key-change-in-secret-store-minimum-32-bytes";

    [Range(1, 1440)]
    public int AccessTokenMinutes { get; init; } = 15;
}

public sealed class HmacJwtTokenGenerator : ITokenGenerator
{
    private static readonly byte[] EncodedHeader = Base64UrlEncodeToArray("""{"alg":"HS256","typ":"JWT"}"""u8);
    private readonly JwtOptions jwtOptions;
    private readonly byte[] signingKey;

    public HmacJwtTokenGenerator(IOptions<JwtOptions> options)
    {
        jwtOptions = options.Value;
        signingKey = Encoding.UTF8.GetBytes(jwtOptions.SigningKey);
    }

    public GeneratedToken GenerateAccessToken(User user, Session session, Guid correlationId)
    {
        var now = DateTimeOffset.UtcNow;
        var expiresAt = now.AddMinutes(jwtOptions.AccessTokenMinutes);
        var issuedAt = now.ToUnixTimeSeconds();
        var expiresAtUnixTime = expiresAt.ToUnixTimeSeconds();
        var jwtId = Guid.NewGuid();

        using var payloadBuffer = new PooledBufferWriter(768);
        using (var jsonWriter = new Utf8JsonWriter(payloadBuffer, new JsonWriterOptions { SkipValidation = true }))
        {
            jsonWriter.WriteStartObject();
            jsonWriter.WriteString("iss"u8, jwtOptions.Issuer);
            jsonWriter.WriteString("aud"u8, jwtOptions.Audience);
            jsonWriter.WriteString("sub"u8, user.Id);
            jsonWriter.WriteString("tenant_id"u8, user.TenantId);
            jsonWriter.WriteString("email"u8, user.Email.Value);
            jsonWriter.WriteString("session_id"u8, session.Id);
            jsonWriter.WriteString("correlation_id"u8, correlationId);
            jsonWriter.WriteNumber("iat"u8, issuedAt);
            jsonWriter.WriteNumber("nbf"u8, issuedAt);
            jsonWriter.WriteNumber("exp"u8, expiresAtUnixTime);
            jsonWriter.WriteString("jti"u8, jwtId);
            jsonWriter.WriteEndObject();
        }

        using var tokenBuffer = new PooledBufferWriter(1536);
        tokenBuffer.Write(EncodedHeader);
        tokenBuffer.WriteByte((byte)'.');
        AppendBase64Url(payloadBuffer.WrittenSpan, tokenBuffer);

        Span<byte> signature = stackalloc byte[32];
        HMACSHA256.HashData(signingKey, tokenBuffer.WrittenSpan, signature);

        tokenBuffer.WriteByte((byte)'.');
        AppendBase64Url(signature, tokenBuffer);

        return new GeneratedToken(Encoding.UTF8.GetString(tokenBuffer.WrittenSpan), expiresAt);
    }

    private static byte[] Base64UrlEncodeToArray(ReadOnlySpan<byte> bytes)
    {
        var maximumLength = Base64.GetMaxEncodedToUtf8Length(bytes.Length);
        var rented = ArrayPool<byte>.Shared.Rent(maximumLength);
        try
        {
            var encoded = rented.AsSpan(0, maximumLength);
            var status = Base64.EncodeToUtf8(bytes, encoded, out _, out var bytesWritten);
            if (status != OperationStatus.Done)
            {
                throw new InvalidOperationException("JWT header encoding failed.");
            }

            var base64UrlLength = NormalizeBase64Url(encoded[..bytesWritten]);
            return encoded[..base64UrlLength].ToArray();
        }
        finally
        {
            ArrayPool<byte>.Shared.Return(rented);
        }
    }

    private static void AppendBase64Url(ReadOnlySpan<byte> bytes, PooledBufferWriter destination)
    {
        var maximumLength = Base64.GetMaxEncodedToUtf8Length(bytes.Length);
        var encoded = destination.GetSpan(maximumLength)[..maximumLength];
        var status = Base64.EncodeToUtf8(bytes, encoded, out _, out var bytesWritten);
        if (status != OperationStatus.Done)
        {
            throw new InvalidOperationException("Base64Url encoding failed.");
        }

        destination.Advance(NormalizeBase64Url(encoded[..bytesWritten]));
    }

    private static int NormalizeBase64Url(Span<byte> encoded)
    {
        var length = encoded.Length;
        while (length > 0 && encoded[length - 1] == (byte)'=')
        {
            length--;
        }

        for (var index = 0; index < length; index++)
        {
            encoded[index] = encoded[index] switch
            {
                (byte)'+' => (byte)'-',
                (byte)'/' => (byte)'_',
                _ => encoded[index]
            };
        }

        return length;
    }

    private sealed class PooledBufferWriter : IBufferWriter<byte>, IDisposable
    {
        private byte[] buffer;
        private int index;

        public PooledBufferWriter(int initialCapacity)
        {
            buffer = ArrayPool<byte>.Shared.Rent(initialCapacity);
        }

        public ReadOnlySpan<byte> WrittenSpan => buffer.AsSpan(0, index);

        public void Advance(int count)
        {
            index += count;
        }

        public Memory<byte> GetMemory(int sizeHint = 0)
        {
            Ensure(sizeHint);
            return buffer.AsMemory(index);
        }

        public Span<byte> GetSpan(int sizeHint = 0)
        {
            Ensure(sizeHint);
            return buffer.AsSpan(index);
        }

        public void WriteByte(byte value)
        {
            Ensure(1);
            buffer[index++] = value;
        }

        public void Write(ReadOnlySpan<byte> value)
        {
            Ensure(value.Length);
            value.CopyTo(buffer.AsSpan(index));
            index += value.Length;
        }

        public void Dispose()
        {
            ArrayPool<byte>.Shared.Return(buffer);
            buffer = [];
            index = 0;
        }

        private void Ensure(int sizeHint)
        {
            if (sizeHint == 0)
            {
                sizeHint = 1;
            }

            if (buffer.Length - index >= sizeHint)
            {
                return;
            }

            Grow(sizeHint);
        }

        private void Grow(int sizeHint)
        {
            var newSize = Math.Max(buffer.Length * 2, index + sizeHint);
            var newBuffer = ArrayPool<byte>.Shared.Rent(newSize);
            buffer.AsSpan(0, index).CopyTo(newBuffer);
            ArrayPool<byte>.Shared.Return(buffer);
            buffer = newBuffer;
        }
    }
}
