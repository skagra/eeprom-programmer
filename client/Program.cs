namespace EEPROMProgrammer
{
    using static ColorConsole;
    using static EEPROMDefinition;

    public class EEPROMProgrammerMain
    {
        private const string _VERSION = "0.1";

        private static readonly string[] ARDUINO_NAMES = { "CH340", "Arduino Uno" };

        private static Protocol _protocol;

        private static void PrintBlock(ushort blockNumber, byte[] block)
        {
            ConsoleWriteln($"Block number: {blockNumber}");
            ConsoleWriteln($"Start address: 0x{(blockNumber * _BLOCK_SIZE):X4}");

            for (var byteIndex = 0; byteIndex < _BLOCK_SIZE; byteIndex++)
            {
                if (byteIndex % 16 == 0)
                {
                    Console.WriteLine();
                    ConsoleWrite($"0x{((blockNumber * _BLOCK_SIZE) + byteIndex):X4}\t", ConsoleColor.Gray);
                }
                ConsoleWrite($"{(block[byteIndex]):X2} ");

            }
            Console.WriteLine();
        }

        private static bool ReadEEPROM(ushort startBlockNumber, ushort numBlocks)
        {
            ConsoleWriteln($"Reading '{numBlocks}' blocks starting at block '{startBlockNumber}'", ConsoleColor.Cyan);
            Console.WriteLine();

            for (ushort blockNum = 0; blockNum < numBlocks; blockNum++)
            {
                ConsoleWrite($"Reading block '{blockNum}'...", ConsoleColor.Gray);
                var block = _protocol.ReadBlock((ushort)(startBlockNumber + blockNum));
                ConsoleWriteln("Done", ConsoleColor.Green);
                Console.WriteLine();
                PrintBlock((ushort)(startBlockNumber + blockNum), block);
                Console.WriteLine();
            }
            return true;
        }

        private static bool WriteEEPROM(string fileName)
        {
            ConsoleWriteln($"Writing from file '{fileName}'", ConsoleColor.Cyan);
            Console.WriteLine();

            var fileInfo = new FileInfo(fileName);
            ushort numBlocks = (ushort)(((fileInfo.Length) / _BLOCK_SIZE) + ((fileInfo.Length % _BLOCK_SIZE != 0) ? 1 : 0));

            ConsoleWriteln($"File size: {fileInfo.Length} bytes ({numBlocks} blocks)");

            var fileBytes = File.ReadAllBytes(fileName);
            for (ushort blockNum = 0; blockNum < numBlocks; blockNum++)
            {
                ConsoleWrite($"Writing block '{blockNum}'...", ConsoleColor.Gray);
                _protocol.WriteBlock(blockNum, fileBytes[(blockNum * _BLOCK_SIZE)..((blockNum + 1) * _BLOCK_SIZE)]);  // TODO - partials!
                ConsoleWriteln("Done", ConsoleColor.Green);
            }

            return true;
        }

        private static bool EraseEEPROM()
        {
            ConsoleWrite("ERASING", ConsoleColor.Cyan);
            return true;
        }

        public static void Main(string[] args)
        {
            ConsoleClear();
            ConsoleCentreMessage("Searching for Arduino...", ConsoleColor.Yellow);

            var serialPort = SerialComms.FindAnduinoComPort(ARDUINO_NAMES);
            if (serialPort != null)
            {
                ConsoleClear();
                ConsoleCentreMessage($"Arduino found on port {serialPort}", ConsoleColor.Green);
                var menu = new Menu(serialPort, _VERSION, ReadEEPROM, WriteEEPROM, EraseEEPROM);
                var serialComms = new SerialComms(serialPort, 9600);
                _protocol = new Protocol(serialComms);

                Thread.Sleep(1000);
                while (!menu.Process()) ;
            }
            else
            {
                ConsoleClear();
                ConsoleCentreMessage("Arduino not found", ConsoleColor.Red);
            }
        }
    }

}