namespace Grave.Indexing
{
    public interface IDocumentFactory
    {
        IDocument Create(object document);
    }
}
