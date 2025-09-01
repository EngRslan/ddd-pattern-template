using System.Text.Json;
using System.Text.Json.Serialization;
using Engrslan.Services;
using Engrslan.Types;
using FastEndpoints;
using Microsoft.Extensions.Primitives;

namespace Engrslan.Binders;

public static class Parser
{
    public static Func<StringValues,ParseResult> ParseEncryptedInt(IEncryptionService encryptionService)
    {
        return (c) =>
        {
            if (c.Count == 0 || string.IsNullOrWhiteSpace(c[0]))
                return new ParseResult(false, "Value is required");
            
            var value = c[0]!.Trim(); 

            try
            {
                var decrypted = encryptionService.DecryptAESToInt(value.FromBase64UrlSafe());
                return new ParseResult(true, new EncryptedInt(decrypted));
            }
            catch
            {
                return new ParseResult(false, "Invalid encrypted value");
            }
        };
    }

    public static Func<StringValues, ParseResult> ParseEncryptedLong(IEncryptionService encryptionService)
    {
        return (c) =>
        {
            if (c.Count == 0 || string.IsNullOrWhiteSpace(c[0]))
                return new ParseResult(false, "Value is required");
            var value = c[0]!.Trim(); 
            try
            {
                var decrypted = encryptionService.DecryptAESToLong(value.FromBase64UrlSafe());
                return new ParseResult(true, new EncryptedLong(decrypted));
            }
            catch
            {
                return new ParseResult(false, "Invalid encrypted value");
            }
        };
    }
}
public class EncryptedIntJsonConverter : JsonConverter<EncryptedInt>
{
    private readonly IEncryptionService _encryptionService;

    public EncryptedIntJsonConverter(IEncryptionService encryptionService)
    {
        _encryptionService = encryptionService ?? throw new ArgumentNullException(nameof(encryptionService));
    }
    public override EncryptedInt Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        try
        {
            var encrypted = reader.GetString();
            if (string.IsNullOrWhiteSpace(encrypted))
            {
                return new EncryptedInt(0);
            }

            var base64Decoded = encrypted.FromBase64UrlSafe();
            var decryptedValue = _encryptionService.DecryptAESToInt(base64Decoded);
            return new EncryptedInt(decryptedValue);
        }
        catch (FormatException ex)
        {
            throw new JsonException("Invalid encrypted integer format", ex);
        }
        catch (ArgumentException ex)
        {
            throw new JsonException("Failed to decrypt integer value", ex);
        }
        catch (Exception ex)
        {
            throw new JsonException("Failed to deserialize encrypted integer", ex);
        }
    }

    public override void Write(Utf8JsonWriter writer, EncryptedInt value, JsonSerializerOptions options)
    {
        try
        {
            var encrypted = _encryptionService.EncryptAES(value);
            var base64Encoded = encrypted.ToUrlSafeBase64();
            writer.WriteStringValue(base64Encoded);
        }
        catch (ArgumentException ex)
        {
            throw new JsonException("Failed to encrypt integer value", ex);
        }
        catch (Exception ex)
        {
            throw new JsonException("Failed to serialize encrypted integer", ex);
        }
    }
}