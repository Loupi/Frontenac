namespace Grave.Indexing
{
    public interface IDocument
    {
        bool Write(string key, object value);
        bool Present(object value);
    }
}
