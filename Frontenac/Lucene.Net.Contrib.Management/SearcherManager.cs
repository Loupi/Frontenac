using System;
using System.Threading;
using Lucene.Net.Search;
using Lucene.Net.Store;

namespace Lucene.Net.Contrib.Management
{
    public class SearcherManager : IDisposable
    {
        private readonly object _reopenLock = new object();
        private readonly ISearcherWarmer _warmer;
        private volatile IndexSearcher _currentSearcher;

        public SearcherManager(IndexSearcher searcher, ISearcherWarmer warmer = null)
        {
            _warmer = warmer;
            _currentSearcher = searcher;
        }

        #region IDisposable

        private bool _disposed;

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        ~SearcherManager()
        {
            Dispose(false);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (_disposed)
                return;

            if (disposing && _currentSearcher != null)
            {
                var oldSearcher = _currentSearcher;
                SwapSearcher(null);
                oldSearcher.Dispose();
            }

            _disposed = true;
        }

        #endregion

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

        public bool MaybeReopen()
        {
            EnsureOpen();
            if (Monitor.TryEnter(_reopenLock))
            {
                var currentReader = _currentSearcher.IndexReader;
                var newReader = _currentSearcher.IndexReader.Reopen();
                if (newReader != currentReader)
                {
                    var oldSearcher = _currentSearcher;
                    var newSearcher = new IndexSearcher(newReader);
                    var success = false;
                    try
                    {
                        _warmer?.Warm(newSearcher);
                        SwapSearcher(newSearcher);
                        oldSearcher.Dispose();
                        success = true;
                    }
                    finally
                    {
                        if (!success)
                        {
                            ReleaseSearcher(newSearcher);
                            newSearcher.Dispose();
                        }
                    }
                }
                return true;
            }

            return false;
        }

        public IndexSearcherToken Acquire()
        {
            return new IndexSearcherToken(AcquireSearcher());
        }

        private IndexSearcher AcquireSearcher()
        {
            IndexSearcher searcher;

            if ((searcher = _currentSearcher) == null)
                throw new AlreadyClosedException("this SearcherManager is closed");

            searcher.IndexReader.IncRef();
            return searcher;
        }

        private static void ReleaseSearcher(IndexSearcher searcher)
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

        internal void SwapSearcher(IndexSearcher newSearcher)
        {
            EnsureOpen();
            var oldSearcher = _currentSearcher;
            _currentSearcher = newSearcher;
            ReleaseSearcher(oldSearcher);
        }

        public class IndexSearcherToken : IDisposable
        {
            public IndexSearcherToken(IndexSearcher searcher)
            {
                Searcher = searcher;
            }

            #region IDisposable

            private bool _disposed;

            public void Dispose()
            {
                Dispose(true);
                GC.SuppressFinalize(this);
            }

            ~IndexSearcherToken()
            {
                Dispose(false);
            }

            protected virtual void Dispose(bool disposing)
            {
                if (_disposed)
                    return;

                if (disposing)
                {
                    ReleaseSearcher(Searcher);
                }

                _disposed = true;
            }

            #endregion

            public IndexSearcher Searcher { get; }
        }

        
    }
}