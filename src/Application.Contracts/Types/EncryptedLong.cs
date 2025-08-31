namespace Engrslan.Types;

public struct EncryptedLong
{
    private readonly long _value;
    
    public static implicit operator long(EncryptedLong encryptedLong) => encryptedLong._value;
    public static implicit operator EncryptedLong(long val) => new (val);

    public EncryptedLong(long value)
    {
        _value = value;
    }
}