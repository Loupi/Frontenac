using System;
using System.Collections.Generic;
using MmGraph.Database.Records;
using MmGraph.Database.Repositories;

namespace MmGraph
{
    public class LabelManager : IDisposable
    {
        private readonly Dictionary<int, IndexRecord> _labelIndices = new Dictionary<int, IndexRecord>();
        private readonly Dictionary<string, int> _labelKeys = new Dictionary<string, int>();
        private readonly IndexRepository _labelRepository;
        private readonly StringRepository _keysRepository;

        private bool _disposed;

        public LabelManager()
        {
            _labelRepository = new IndexRepository("label.store");
            _keysRepository = new StringRepository("label.keys.store");

            Initialize();
        }

        private void Initialize()
        {
            // TODO: Use an index instead
            foreach (var labelRecord in _labelRepository.Scan())
            {
                labelRecord.Key = _keysRepository.Read(labelRecord.KeyBlockId).Value;
                _labelKeys.Add(labelRecord.Key, labelRecord.Id);
                _labelIndices.Add(labelRecord.Id, labelRecord);
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        ~LabelManager()
        {
            Dispose(false);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (_disposed)
                return;

            if (disposing)
            {
                _labelRepository?.Dispose();
                _keysRepository?.Dispose();
            }

            _disposed = true;
        }

        public IndexRecord GetLabel(string key)
        {
            return _labelKeys.TryGetValue(key, out var id)
                ? GetLabel(id)
                : null;
        }

        public IndexRecord GetLabel(int id)
        {
            return _labelIndices.TryGetValue(id, out var label) 
                ? label
                : null;
        }

        public IndexRecord CreateOrGet(string key)
        {
            if (_labelKeys.TryGetValue(key, out var keyIndexId) &&
                _labelIndices.TryGetValue(keyIndexId, out var labelRecord))
                return labelRecord;

            var keyRecord = _keysRepository.Create(new StringBlockRecord {Value = key});
            labelRecord = new IndexRecord { Key = key, KeyBlockId = keyRecord};
            labelRecord.Id = _labelRepository.Create(labelRecord);
            labelRecord.Key = key;
            _labelKeys.Add(key, labelRecord.Id);
            _labelIndices.Add(labelRecord.Id, labelRecord);

            return labelRecord;
        }

        public int Increment(string key)
        {
            return _labelKeys.TryGetValue(key, out var labelId)
                ? Increment(labelId)
                : 0;
        }

        private int Increment(int id)
        {
            var labelRecord = GetLabel(id);
            labelRecord.IncrementCount();
            _labelRepository.Update(id, labelRecord);
            return labelRecord.Count;
        }

        public int Decrement(string key)
        {
            return _labelKeys.TryGetValue(key, out var labelId) 
                ? Decrement(labelId) 
                : 0;
        }

        public int Decrement(int id)
        {
            var labelRecord = GetLabel(id);
            if (labelRecord == null)
                return 0;

            labelRecord.DecrementCount();
            _labelRepository.Update(id, labelRecord);

            if (labelRecord.Count == 0)
                Delete(labelRecord);

            return labelRecord.Count;
        }

        private void Delete(IndexRecord labelRecord)
        {
            _keysRepository.Delete(labelRecord.KeyBlockId);
            _labelRepository.Delete(labelRecord.Id);
            _labelKeys.Remove(labelRecord.Key);
            _labelIndices.Remove(labelRecord.Id);
        }
    }
}
