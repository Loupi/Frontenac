namespace Grave.Esent.Serializers
{
    public interface IContentSerializer
    {
        bool IsBinary { get; }
        byte[] Serialize(object value);
        object Deserialize(byte[] raw);
    }
}
