namespace Grave.Indexing.Indexers
{
    public abstract class Indexer
    {
        protected readonly IDocument Document;

        protected Indexer(IDocument document)
        {
            Document = document;
        }

        public abstract void Index(string documentName);
    }
}
