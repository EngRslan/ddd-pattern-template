using Engrslan.Services;
using FastEndpoints;
using Microsoft.Extensions.Primitives;

namespace Engrslan.Binders;

public struct EncryptedInt
{
    private int _value;
    
    public static implicit operator int(EncryptedInt encryptedInt) => encryptedInt._value;

    
    public static Func<StringValues,ParseResult> Parser(IEncryptionService encryptionService)
    {
        return (c) =>
        {
            if (c.Count == 0 || string.IsNullOrWhiteSpace(c[0]))
                return new ParseResult(false, "Value is required");

            try
            {
                var decrypted = encryptionService.DecryptAESToInt(c[0]!.FromBase64UrlSafe());
                return new ParseResult(true, new EncryptedInt { _value = decrypted });
            }
            catch
            {
                return new ParseResult(false, "Invalid encrypted value");
            }
        };
    }
}
