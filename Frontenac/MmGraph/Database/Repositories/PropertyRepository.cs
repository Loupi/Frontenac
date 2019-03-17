using System;
using MmGraph.Database.Records;

namespace MmGraph.Database.Repositories
{
    public class PropertyRepository : MemoryMappedRepository<PropertyRecord>
    {
        private const int RecordSize = 25;
        private const int OffsetType = 1;
        private const int OffsetKeyIndexId = 5;
        private const int OffsetPropertyBlockId = 9;
        private const int OffsetPreviousPropertyId = 17;
        private const int OffsetNextPropertyId = 21;

        public PropertyRepository(string storeName)
            : base(storeName, RecordSize)
        {
        }

        public unsafe void DeleteList(int id, Action<PropertyRecord> deleteAction, bool useIn = false)
        {
            var pointer = GetPointer(id);
            if (*pointer == 0)
                throw new InvalidOperationException($"Cannot delete entry '{id}' because it is not in use.");

            var entry = Read(id, pointer);
            if (entry.PreviousPropertyId != -1)
                throw new InvalidOperationException("Property lists can only be deleted from their first block.");
            
            deleteAction(entry);
            *pointer = 0;
            IdGenerator.FreeId(id);

            while (entry.NextPropertyId != -1)
            {
                pointer = GetPointer(entry.NextPropertyId);
                if (*pointer == 0)
                    throw new InvalidOperationException(
                        $"Cannot delete next entry '{entry.NextPropertyId}' of '{id}' because it is not in use.");

                id = entry.NextPropertyId;
                entry = Read(id, pointer);
                deleteAction(entry);
                *pointer = 0;
                IdGenerator.FreeId(id);
            }
        }

        protected override unsafe PropertyRecord Read(int id, byte* pointer)
        {
            var record = new PropertyRecord
            {
                Id = id,
                PropertyType = (PropertyType) (*(int*)(pointer + OffsetType)),
                KeyIndexId = *(int*)(pointer + OffsetKeyIndexId),
                PreviousPropertyId = *(int*)(pointer + OffsetPreviousPropertyId),
                NextPropertyId = *(int*)(pointer + OffsetNextPropertyId)
            };
            ReadPropertyValue(pointer, record);
            return record;
        }

        protected override unsafe void Write(int id, byte* pointer, PropertyRecord record)
        {
            *(int*)(pointer + OffsetType) = (int) record.PropertyType;
            *(int*)(pointer + OffsetKeyIndexId) = record.KeyIndexId;
            *(int*)(pointer + OffsetPreviousPropertyId) = record.PreviousPropertyId;
            *(int*)(pointer + OffsetNextPropertyId) = record.NextPropertyId;
            WritePropertyValue(pointer, record);
        }

        private static unsafe void ReadPropertyValue(byte* pointer, PropertyRecord record)
        {
            switch (record.PropertyType)
            {
                case PropertyType.Null:
                    record.Value = null;
                    break;

                case PropertyType.Boolean:
                    record.Value = *(bool*) (pointer + OffsetPropertyBlockId);
                    break;

                case PropertyType.Byte:
                    record.Value = *pointer + OffsetPropertyBlockId;
                    break;

                case PropertyType.Sbyte:
                    record.Value = *(sbyte*)(pointer + OffsetPropertyBlockId);
                    break;

                case PropertyType.Char:
                    record.Value = *(char*)(pointer + OffsetPropertyBlockId);
                    break;
                    
                case PropertyType.Double:
                    record.Value = *(double*)(pointer + OffsetPropertyBlockId);
                    break;

                case PropertyType.Float:
                    record.Value = *(float*)(pointer + OffsetPropertyBlockId);
                    break;

                case PropertyType.Int:
                    record.Value = *(int*)(pointer + OffsetPropertyBlockId);
                    break;

                case PropertyType.Uint:
                    record.Value = *(uint*)(pointer + OffsetPropertyBlockId);
                    break;

                case PropertyType.Long:
                    record.Value = *(long*)(pointer + OffsetPropertyBlockId);
                    break;

                case PropertyType.Ulong:
                    record.Value = *(ulong*)(pointer + OffsetPropertyBlockId);
                    break;

                case PropertyType.Short:
                    record.Value = *(short*)(pointer + OffsetPropertyBlockId);
                    break;

                case PropertyType.Ushort:
                    record.Value = *(ushort*)(pointer + OffsetPropertyBlockId);
                    break;

                case PropertyType.String:
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
                    record.PropertyBlockId = *(long*)(pointer + OffsetPropertyBlockId);
                    break;

                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        

        private static unsafe void WritePropertyValue(byte* pointer, PropertyRecord record)
        {
            switch (record.PropertyType)
            {
                case PropertyType.Null:
                    break;

                case PropertyType.Boolean:
                    *(bool*)(pointer + OffsetPropertyBlockId) = (bool)record.Value;
                    break;

                case PropertyType.Byte:
                    *(pointer + OffsetPropertyBlockId) = (byte)record.Value;
                    break;

                case PropertyType.Sbyte:
                    record.Value = *(sbyte*)(pointer + OffsetPropertyBlockId) = (sbyte)record.Value;
                    break;

                case PropertyType.Char:
                    record.Value = *(char*)(pointer + OffsetPropertyBlockId) = (char)record.Value;
                    break;

                case PropertyType.Double:
                    record.Value = *(double*)(pointer + OffsetPropertyBlockId) = (double)record.Value;
                    break;

                case PropertyType.Float:
                    record.Value = *(float*)(pointer + OffsetPropertyBlockId) = (float)record.Value;
                    break;

                case PropertyType.Int:
                    record.Value = *(int*)(pointer + OffsetPropertyBlockId) = (int)record.Value;
                    break;

                case PropertyType.Uint:
                    record.Value = *(uint*)(pointer + OffsetPropertyBlockId) = (uint)record.Value;
                    break;

                case PropertyType.Long:
                    record.Value = *(long*)(pointer + OffsetPropertyBlockId) = (long)record.Value;
                    break;

                case PropertyType.Ulong:
                    record.Value = *(ulong*)(pointer + OffsetPropertyBlockId) = (ulong)record.Value;
                    break;

                case PropertyType.Short:
                    record.Value = *(short*)(pointer + OffsetPropertyBlockId) = (short)record.Value;
                    break;

                case PropertyType.Ushort:
                    record.Value = *(ushort*)(pointer + OffsetPropertyBlockId) = (ushort)record.Value;
                    break;

                case PropertyType.String:
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
                    *(long*)(pointer + OffsetPropertyBlockId) = record.PropertyBlockId;
                    break;

                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}