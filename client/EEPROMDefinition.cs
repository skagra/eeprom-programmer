namespace EEPROMProgrammer
{
    public static class EEPROMDefinition
    {
        public const ushort _BLOCK_SIZE = 64;
        public const ushort _ROM_SIZE_BYTES = 8192;
        public const ushort _ROM_SIZE_BLOCKS = _ROM_SIZE_BYTES / _BLOCK_SIZE;
    }

}