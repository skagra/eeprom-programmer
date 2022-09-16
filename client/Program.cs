namespace EEPROMProgrammer
{
    using static ColorConsole;
    using static ColourValues;
    using static EEPROMDefinition;

    public class EEPROMProgrammerMain
    {
        private const string _VERSION = "0.1";
        private const string _EEPROM_TYPE = "AT28HC64";

        private const int BAUD_RATE = 57600;

        private static readonly string[] ARDUINO_NAMES = { "CH340", "Arduino Uno" };

        private static Protocol _protocol;

        private static string _serialPort;

        private static void PrintBlock(ushort blockNumber, byte[] block)
        {
            ConsoleWriteln($"Block number: {blockNumber}", COLOUR_PROGRESS);
            ConsoleWriteln($"Start address: 0x{(blockNumber * _BLOCK_SIZE):X4}", COLOUR_PROGRESS);

            for (var byteIndex = 0; byteIndex < _BLOCK_SIZE; byteIndex++)
            {
                if (byteIndex % 16 == 0)
                {
                    Console.WriteLine();
                    ConsoleWrite($"0x{((blockNumber * _BLOCK_SIZE) + byteIndex):X4}\t", COLOUR_PROGRESS);
                }
                ConsoleWrite($"{(block[byteIndex]):X2} ", COLOUR_BODY);

            }
            Console.WriteLine();
        }

        private static void ReadEEPROM(ushort startBlockNumber, ushort numBlocks)
        {
            ConsoleWriteln($"Reading '{numBlocks}' blocks starting at block '{startBlockNumber}'", COLOUR_BODY);
            Console.WriteLine();

            for (ushort blockNum = 0; blockNum < numBlocks; blockNum++)
            {
                ConsoleWrite($"Reading block '{blockNum}'...", COLOUR_PROGRESS);
                var block = _protocol.ReadBlock((ushort)(startBlockNumber + blockNum));
                ConsoleWriteln("Done", COLOUR_OK);
                Console.WriteLine();
                PrintBlock((ushort)(startBlockNumber + blockNum), block);
                Console.WriteLine();
            }

            ConsoleWriteln("Done reading EEPROM", COLOUR_OK);
        }

        private static bool Compare(byte[] expected, byte[] got)
        {
            var same = true;

            for (int index = 0; index < _BLOCK_SIZE && same; index++)
            {
                same = expected[index] == got[index];
            }

            return same;
        }

        private static void WriteEEPROM(string fileName)
        {
            ConsoleWriteln($"Writing from file '{fileName}'", COLOUR_PROGRESS);
            Console.WriteLine();

            var fileInfo = new FileInfo(fileName);
            ushort numBlocks = (ushort)(((fileInfo.Length) / _BLOCK_SIZE) + ((fileInfo.Length % _BLOCK_SIZE != 0) ? 1 : 0));

            ConsoleWriteln($"File size: {fileInfo.Length} bytes ({numBlocks} blocks)", COLOUR_BODY);
            Console.WriteLine();

            ConsoleWriteln("Writing", COLOUR_BODY);
            var fileBytes = File.ReadAllBytes(fileName);
            for (ushort blockNum = 0; blockNum < numBlocks; blockNum++)
            {
                ConsoleWrite($"Writing block '{blockNum}'...", COLOUR_PROGRESS);
                _protocol.WriteBlock(blockNum, fileBytes[(blockNum * _BLOCK_SIZE)..((blockNum + 1) * _BLOCK_SIZE)]);  // TODO - partials!
                ConsoleWriteln("Done", COLOUR_OK);
            }
            ConsoleWriteln("Written", COLOUR_OK);

            Console.WriteLine();
            Console.WriteLine("Verifying", COLOUR_BODY);
            var same = true;
            for (ushort blockNum = 0; blockNum < numBlocks && same; blockNum++)
            {
                var block = _protocol.ReadBlock(blockNum);
                ConsoleWrite($"Verifying block '{blockNum}'...", COLOUR_PROGRESS);
                same = Compare(fileBytes[(blockNum * _BLOCK_SIZE)..((blockNum + 1) * _BLOCK_SIZE)], block); // TODO - partials!
                if (same)
                {
                    ConsoleWriteln("OK", COLOUR_OK);
                }
                else
                {
                    ConsoleWriteln("Error", COLOUR_ERROR);
                }
            }

            if (same)
            {
                ConsoleWriteln("Verified", COLOUR_OK);
            }
            else
            {
                // TODO - Display expected and actual block
            }

            Console.WriteLine();
            ConsoleWriteln("Done writing EEPROM", COLOUR_OK);
        }

        private static void IsEEPROMEmpty()
        {
            ConsoleWrite("Checking", COLOUR_BODY);
            var empty = true;
            for (ushort blockNum = 0; blockNum < _ROM_SIZE_BLOCKS && empty; blockNum++)
            {
                var block = _protocol.ReadBlock(blockNum);
                ConsoleWrite(".", COLOUR_PROGRESS);
                empty = Compare(fillPattern, block); // TODO - partials!
            }
            ConsoleWriteln("Done", COLOUR_OK);
            Console.WriteLine();
            if (empty)
            {
                ConsoleWriteln("EEPROM is empty.", COLOUR_OK);
            }
            else
            {
                ConsoleWriteln("EEPROM is NOT empty.", COLOUR_ERROR);
            }
        }

        private static void ShowConfiguration()
        {
            const int boxWidth = 40;
            var border = new string('#', boxWidth);
            var wrapper = $"#{new string(' ', boxWidth - 2)}#";

            Console.CursorTop = VerticalCentreCursor(8);
            ConsoleWritelnMiddle(border, COLOUR_TITLE);
            ConsoleWritelnMiddle(wrapper, COLOUR_TITLE);
            ConsoleWriteMiddle(wrapper, COLOUR_TITLE);
            ConsoleWritelnMiddle($"EEPROM Programmer version {_VERSION}", COLOUR_BODY);
            ConsoleWriteMiddle(wrapper, COLOUR_TITLE);
            ConsoleWritelnMiddle($"Arduino Port: {_serialPort}", COLOUR_BODY);
            ConsoleWriteMiddle(wrapper, COLOUR_TITLE);
            ConsoleWritelnMiddle($"EEPROM Type: {_EEPROM_TYPE}", COLOUR_BODY);
            ConsoleWriteMiddle(wrapper, COLOUR_TITLE);
            ConsoleWritelnMiddle($"EEPROM Size: {_ROM_SIZE_BYTES} bytes", COLOUR_BODY);
            ConsoleWritelnMiddle(wrapper, COLOUR_TITLE);
            ConsoleWritelnMiddle(border, COLOUR_TITLE);
        }

        private readonly static byte[] fillPattern;
        static EEPROMProgrammerMain()
        {
            fillPattern = new byte[_BLOCK_SIZE];
            Array.Fill<byte>(fillPattern, 0xFF);
        }

        private static void EraseEEPROM()
        {
            ConsoleWriteln("Erasing");
            for (ushort blockNum = 0; blockNum < _ROM_SIZE_BLOCKS; blockNum++)
            {
                ConsoleWrite($"Erasing block '{blockNum}'...", COLOUR_PROGRESS);
                _protocol.WriteBlock(blockNum, fillPattern);  // TODO - partials!
                ConsoleWriteln("Done", COLOUR_OK);
            }

            Console.WriteLine();
            ConsoleWriteln("Done erasing EEPROM", COLOUR_OK);
        }

        public static void Main(string[] args)
        {
            ConsoleClear();
            ConsoleCentreMessage("Searching for Arduino...", ConsoleColor.Yellow);

            _serialPort = SerialComms.FindArduinoComPort(ARDUINO_NAMES);
            if (_serialPort != null)
            {
                ConsoleClear();
                ConsoleCentreMessage($"Arduino found on port {_serialPort}", COLOUR_OK);
                var menu = new Menu(ReadEEPROM, WriteEEPROM, IsEEPROMEmpty, EraseEEPROM, ShowConfiguration);
                var serialComms = new SerialComms(_serialPort, BAUD_RATE);
                _protocol = new Protocol(serialComms);

                Thread.Sleep(1000);
                while (!menu.Process()) ;
            }
            else
            {
                ConsoleClear();
                ConsoleCentreMessage("Arduino not found", COLOUR_ERROR);
            }
        }
    }

}