using System;
using System.Collections.Generic;
using MmGraph.Database.Records;
using MmGraph.Database.Repositories;

namespace MmGraph
{
    public class PropertyManager : IDisposable
    {
        private readonly StringRepository _stringRepository;
        private readonly IndexRepository _indexRepository;
        private readonly PropertyRepository _propertyRepository;
        private readonly StringRepository _keysRepository;

        private readonly Dictionary<string, long> _propertyKeys = new Dictionary<string, long>();
        private readonly Dictionary<long, IndexRecord> _propertyIndices = new Dictionary<long, IndexRecord>();
        
        private bool _disposed;

        public PropertyManager(string baseName, StringRepository stringRepository)
        {
            _stringRepository = stringRepository;
            _indexRepository = new IndexRepository(baseName + ".index.store");
            _propertyRepository = new PropertyRepository(baseName + ".property.store");
            _keysRepository = new StringRepository(baseName + ".keys.store");

            Initialize();
        }

        private void Initialize()
        {
            foreach (var index in _indexRepository.Scan())
            {
                index.Key = _keysRepository.Read(index.KeyBlockId).Value;
                _propertyKeys.Add(index.Key, index.KeyBlockId);
                _propertyIndices.Add(index.KeyBlockId, index);
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        ~PropertyManager()
        {
            Dispose(false);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (_disposed)
                return;

            if (disposing)
            {
                WritePropertyIndices();
                _indexRepository?.Dispose();
                _propertyRepository?.Dispose();
                _keysRepository?.Dispose();
            }

            _disposed = true;
        }

        public IEnumerable<string> GetPropertyKeys(long nextPropertyId)
        {
            while (nextPropertyId != -1)
            {
                var property = _propertyRepository.Read((int)nextPropertyId);
                if (!_propertyIndices.TryGetValue(property.KeyIndexId, out var index))
                    throw new InvalidOperationException($"No index found for keyId '{property.KeyIndexId}'.");
                
                nextPropertyId = property.NextPropertyId;
                yield return index.Key;
            }
        }

        public void SetProperty(long nextPropertyId, string key, object value, out long newNextPropertyId)
        {
            newNextPropertyId = nextPropertyId;
            IndexRecord index;
            if (!_propertyKeys.TryGetValue(key, out var keyId))
            {
                keyId = _keysRepository.Create(new StringBlockRecord { Value = key });
                index = new IndexRecord {KeyBlockId = (int) keyId, Key = key};
                index.Id = _indexRepository.Create(index);
                _propertyKeys.Add(key, keyId);
                _propertyIndices.Add(keyId, index);
            }

            if (!_propertyIndices.TryGetValue(keyId, out index))
                throw new InvalidOperationException($"No index found for key '{key}'.");

            PropertyRecord property = null;
            var found = false;
            long lastPropertyId = -1;

            while (nextPropertyId != -1)
            {
                lastPropertyId = nextPropertyId;
                property = _propertyRepository.Read((int)nextPropertyId);
                if (property.KeyIndexId == keyId)
                {
                    found = true;
                    break;
                }

                nextPropertyId = property.NextPropertyId;
            }

            var newProperty = new PropertyRecord
            {
                Value = value,
                KeyIndexId = (int)keyId,
                NextPropertyId = -1,
                PreviousPropertyId = (int)lastPropertyId
            };

            if (found)
            {
                DeletePropertyBlock(property);
                CreatePropertyBlock(newProperty);
                newProperty.NextPropertyId = property.NextPropertyId;
                _propertyRepository.Update(property.Id, newProperty);
            }
            else
            {
                CreatePropertyBlock(newProperty);
                var id = _propertyRepository.Create(newProperty);
                if (newProperty.PreviousPropertyId == -1)
                    newNextPropertyId = id;
                else if (property != null)
                {
                    property.NextPropertyId = id;
                    _propertyRepository.Update(property.Id, property);
                }

                index.IncrementCount();
            }
        }

        public object GetProperty(long nextPropertyId, string key)
        {
            if (!_propertyKeys.TryGetValue(key, out var keyId))
                throw new InvalidOperationException($"No keyIndex found for key '{key}'.");
            
            while (nextPropertyId != -1)
            {
                var property = _propertyRepository.Read((int)nextPropertyId);
                if (property.KeyIndexId == keyId)
                {
                    ReadPropertyBlock(property);
                    return property.Value;
                }

                nextPropertyId = property.NextPropertyId;
            }

            return null;
        }

        public object RemoveProperty(long nextPropertyId, string key, out long newNextPropertyId)
        {
            if (!_propertyKeys.TryGetValue(key, out var keyId))
                throw new InvalidOperationException($"No keyIndex found for key '{key}'.");

            object result = null;
            newNextPropertyId = nextPropertyId;

            while (nextPropertyId != -1)
            {
                var property = _propertyRepository.Read((int)nextPropertyId);
                if (property.KeyIndexId == keyId)
                {
                    ReadPropertyBlock(property);
                    result = property.Value;

                    if (property.NextPropertyId != -1)
                    {
                        var nextProperty = _propertyRepository.Read(property.NextPropertyId);
                        nextProperty.PreviousPropertyId = property.PreviousPropertyId;
                        _propertyRepository.Update(property.NextPropertyId, nextProperty);
                        if (property.PreviousPropertyId == -1)
                            newNextPropertyId = property.NextPropertyId;
                    }
                    else
                    {
                        if (property.PreviousPropertyId == -1)
                            newNextPropertyId = -1;
                    }

                    if (property.PreviousPropertyId != -1)
                    {
                        var previousProperty = _propertyRepository.Read(property.PreviousPropertyId);
                        previousProperty.NextPropertyId = property.NextPropertyId;
                        _propertyRepository.Update(property.PreviousPropertyId, previousProperty);
                    }

                    DeletePropertyBlock(property);
                    _propertyRepository.Delete((int)nextPropertyId);
                    break;
                }

                nextPropertyId = property.NextPropertyId;
            }

            return result;
        }

        public void DeletePropertyList(long nextPropertyId)
        {
            _propertyRepository.DeleteList((int)nextPropertyId, DeletePropertyBlock);
        }

        public void WritePropertyIndices()
        {
            foreach (var propertyIndex in _propertyIndices)
            {
                _indexRepository.Update(propertyIndex.Value.Id, propertyIndex.Value);
            }
        }

        private void ReadPropertyBlock(PropertyRecord record)
        {
            if (record.PropertyBlockId == -1) return;

            switch (record.PropertyType)
            {
                case PropertyType.Boolean:
                case PropertyType.Byte:
                case PropertyType.Sbyte:
                case PropertyType.Char:
                case PropertyType.Double:
                case PropertyType.Float:
                case PropertyType.Int:
                case PropertyType.Uint:
                case PropertyType.Long:
                case PropertyType.Ulong:
                case PropertyType.Short:
                case PropertyType.Ushort:
                    break;

                case PropertyType.String:
                    var stringRecord = _stringRepository.Read((int)record.PropertyBlockId);
                    record.Value = stringRecord.Value;
                    break;

                case PropertyType.Decimal:
                case PropertyType.BooleanArray:
                case PropertyType.ByteArray:
                case PropertyType.SbyteArray:
                case PropertyType.CharArray:
                case PropertyType.DoubleArray:
                case PropertyType.FloatArray:
                case PropertyType.IntArray:
                case PropertyType.UintArray:
                case PropertyType.LongArray:
                case PropertyType.UlongArray:
                case PropertyType.ShortArray:
                case PropertyType.UshortArray:
                case PropertyType.StringArray:
                    break;

                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private void DeletePropertyBlock(PropertyRecord property)
        {
            if (!_propertyIndices.TryGetValue(property.KeyIndexId, out var index))
                throw new InvalidOperationException($"Cannot find index for keyIndexId: {property.KeyIndexId}.");

            index.DecrementCount();

            if (property.PropertyBlockId != -1)
            {
                switch (property.PropertyType)
                {
                    case PropertyType.Boolean:
                    case PropertyType.Byte:
                    case PropertyType.Sbyte:
                    case PropertyType.Char:
                    case PropertyType.Double:
                    case PropertyType.Float:
                    case PropertyType.Int:
                    case PropertyType.Uint:
                    case PropertyType.Long:
                    case PropertyType.Ulong:
                    case PropertyType.Short:
                    case PropertyType.Ushort:
                        break;

                    case PropertyType.String:
                        _stringRepository.Delete((int)property.PropertyBlockId);
                        break;

                    case PropertyType.Decimal:
                    case PropertyType.BooleanArray:
                    case PropertyType.ByteArray:
                    case PropertyType.SbyteArray:
                    case PropertyType.CharArray:
                    case PropertyType.DoubleArray:
                    case PropertyType.FloatArray:
                    case PropertyType.IntArray:
                    case PropertyType.UintArray:
                    case PropertyType.LongArray:
                    case PropertyType.UlongArray:
                    case PropertyType.ShortArray:
                    case PropertyType.UshortArray:
                    case PropertyType.StringArray:
                        break;

                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
        }

        private void CreatePropertyBlock(PropertyRecord property)
        {
            property.PropertyType = PropertyTypeHelpers.GetPropertyType(property.Value);
            property.PropertyBlockId = -1;

            switch (property.PropertyType)
            {
                case PropertyType.Null:
                case PropertyType.Boolean:
                case PropertyType.Byte:
                case PropertyType.Sbyte:
                case PropertyType.Char:
                case PropertyType.Double:
                case PropertyType.Float:
                case PropertyType.Int:
                case PropertyType.Uint:
                case PropertyType.Long:
                case PropertyType.Ulong:
                case PropertyType.Short:
                case PropertyType.Ushort:
                    break;

                case PropertyType.String:
                    property.PropertyBlockId = _stringRepository.Create(new StringBlockRecord
                    {
                        Value = (string)property.Value
                    });
                    break;

                case PropertyType.Decimal:
                case PropertyType.BooleanArray:
                case PropertyType.ByteArray:
                case PropertyType.SbyteArray:
                case PropertyType.CharArray:
                case PropertyType.DoubleArray:
                case PropertyType.FloatArray:
                case PropertyType.IntArray:
                case PropertyType.UintArray:
                case PropertyType.LongArray:
                case PropertyType.UlongArray:
                case PropertyType.ShortArray:
                case PropertyType.UshortArray:
                case PropertyType.StringArray:
                    break;

                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}