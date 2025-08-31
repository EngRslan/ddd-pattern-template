using System.Text.Json;
using System.Text.Json.Serialization;
using Engrslan.Services;
using Engrslan.Types;

namespace Engrslan.Binders;

public class EncryptedLongJsonConverter : JsonConverter<EncryptedLong>
{
    private readonly IEncryptionService _encryptionService;

    public EncryptedLongJsonConverter(IEncryptionService encryptionService)
    {
        _encryptionService = encryptionService ?? throw new ArgumentNullException(nameof(encryptionService));
    }
    public override EncryptedLong Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        try
        {
            var encrypted = reader.GetString();
            if (string.IsNullOrWhiteSpace(encrypted))
            {
                return new EncryptedLong(0);
            }

            var base64Decoded = encrypted.FromBase64UrlSafe();
            var decryptedValue = _encryptionService.DecryptAESToLong(base64Decoded);
            return new EncryptedLong(decryptedValue);
        }
        catch (FormatException ex)
        {
            throw new JsonException("Invalid encrypted long format", ex);
        }
        catch (ArgumentException ex)
        {
            throw new JsonException("Failed to decrypt long value", ex);
        }
        catch (Exception ex)
        {
            throw new JsonException("Failed to deserialize encrypted long", ex);
        }
    }

    public override void Write(Utf8JsonWriter writer, EncryptedLong value, JsonSerializerOptions options)
    {
        try
        {
            var encrypted = _encryptionService.EncryptAES(value);
            var base64Encoded = encrypted.ToUrlSafeBase64();
            writer.WriteStringValue(base64Encoded);
        }
        catch (ArgumentException ex)
        {
            throw new JsonException("Failed to encrypt long value", ex);
        }
        catch (Exception ex)
        {
            throw new JsonException("Failed to serialize encrypted long", ex);
        }
    }
}