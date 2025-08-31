using Engrslan.Services;
using Microsoft.Extensions.Primitives;

namespace Engrslan.Types;

public struct EncryptedInt
{
    private readonly int _value;
    
    public static implicit operator int(EncryptedInt encryptedInt) => encryptedInt._value;
    public static implicit operator EncryptedInt(int val) => new (val);

    public EncryptedInt(int value)
    {
        _value = value;
    }
}
