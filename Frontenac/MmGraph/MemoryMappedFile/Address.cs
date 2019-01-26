namespace MmGraph.MemoryMappedFile
{
    public unsafe class Address
    {
        public byte* MemoryLocation { get; }

        public Address(byte* memoryLocation)
        {
            MemoryLocation = memoryLocation;
        }
    }
}
