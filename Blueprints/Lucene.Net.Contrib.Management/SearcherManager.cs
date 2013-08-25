using System;
using System.Threading;
using Lucene.Net.Index;
using Lucene.Net.Search;
using Lucene.Net.Store;

namespace Lucene.Net.Contrib.Management
{
    public class SearcherManager : IDisposable
    {
        private readonly ISearcherWarmer _warmer;

        private readonly object _reopenLock = new object();


        private volatile IndexSearcher _currentSearcher;

        public SearcherManager(IndexWriter writer, bool applyAllDeletes = true, ISearcherWarmer warmer = null)
        {
            _warmer = warmer;
            _currentSearcher = new IndexSearcher(writer.GetReader());
            if (_warmer != null)
            {
                writer.MergedSegmentWarmer = new WarmerWrapper(_warmer);                
            }
        }

        public bool MaybeReopen()
        {
            EnsureOpen();
            if (Monitor.TryEnter(_reopenLock))
            {
                var currentReader = _currentSearcher.IndexReader;
                var newReader = _currentSearcher.IndexReader.Reopen();
                if (newReader != currentReader)
                {
                    var newSearcher = new IndexSearcher(newReader);
                    var success = false;
                    try
                    {
                        if (_warmer != null)
                        {
                            _warmer.Warm(newSearcher);
                        }
                        SwapSearcher(newSearcher);
                        success = true;
                    }
                    finally
                    {
                        if (!success)
                        {
                            ReleaseSearcher(newSearcher);
                        }
                    }
                }
                return true;
            }

            return false;
        }

        public bool IsSearcherCurrent
        {
            get
            {
                var searcher = AcquireSearcher();
                try
                {
                    return searcher.IndexReader.IsCurrent();
                }
                finally
                {
                    ReleaseSearcher(searcher);
                }
            }
        }

        public IndexSearcherToken Acquire()
        {
            return new IndexSearcherToken(AcquireSearcher(), this);
        }

        private IndexSearcher AcquireSearcher()
        {
            IndexSearcher searcher;

            if ((searcher = _currentSearcher) == null)
            {
                throw new AlreadyClosedException("this SearcherManager is closed");
            }
            searcher.IndexReader.IncRef();
            return searcher;
        }

        private void ReleaseSearcher(IndexSearcher searcher)
        {            
            searcher.IndexReader.DecRef();
        }

        private void EnsureOpen()
        {
            if (_currentSearcher == null)
            {
                throw new AlreadyClosedException("this SearcherManager is closed");
            }
        }

        private void SwapSearcher(IndexSearcher newSearcher)
        {
            EnsureOpen();
            var oldSearcher = _currentSearcher;
            _currentSearcher = newSearcher;
            ReleaseSearcher(oldSearcher);            
        }

        public void Dispose()
        {
            if (_currentSearcher != null)
            {
                // make sure we can call this more than once
                // closeable javadoc says:
                // if this is already closed then invoking this method has no effect.
                SwapSearcher(null);
            }
        }


        public class IndexSearcherToken : IDisposable
        {
            private readonly SearcherManager _manager;
            public IndexSearcher Searcher { get; private set; }

            public IndexSearcherToken(IndexSearcher searcher, SearcherManager manager)
            {
                _manager = manager;
                Searcher = searcher;
            }

            public void Dispose()
            {
                _manager.ReleaseSearcher(Searcher);
            }
        }

        class WarmerWrapper : IndexWriter.IndexReaderWarmer
        {
            private readonly ISearcherWarmer _searcher;

            public WarmerWrapper(ISearcherWarmer searcher)
            {
                _searcher = searcher;
            }

            public override void Warm(IndexReader reader)
            {
                _searcher.Warm(new IndexSearcher(reader));
            }
        }
    }
}