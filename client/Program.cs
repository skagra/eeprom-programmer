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

        private static bool Compare(byte[] expected, byte[] got)
        {
            var same = true;

            for (int index = 0; index < _BLOCK_SIZE && same; index++)
            {
                same = expected[index] == got[index];
            }

            return same;
        }

        private static bool WriteEEPROM(string fileName)
        {
            ConsoleWriteln($"Writing from file '{fileName}'", ConsoleColor.Cyan);
            Console.WriteLine();

            var fileInfo = new FileInfo(fileName);
            ushort numBlocks = (ushort)(((fileInfo.Length) / _BLOCK_SIZE) + ((fileInfo.Length % _BLOCK_SIZE != 0) ? 1 : 0));

            ConsoleWriteln($"File size: {fileInfo.Length} bytes ({numBlocks} blocks)");
            Console.WriteLine();

            ConsoleWriteln("Writing");
            var fileBytes = File.ReadAllBytes(fileName);
            for (ushort blockNum = 0; blockNum < numBlocks; blockNum++)
            {
                ConsoleWrite($"Writing block '{blockNum}'...", ConsoleColor.Gray);
                _protocol.WriteBlock(blockNum, fileBytes[(blockNum * _BLOCK_SIZE)..((blockNum + 1) * _BLOCK_SIZE)]);  // TODO - partials!
                ConsoleWriteln("Done", ConsoleColor.Green);
            }
            ConsoleWriteln("Written", ConsoleColor.Green);

            Console.WriteLine();
            Console.WriteLine("Verifying");
            var same = true;
            for (ushort blockNum = 0; blockNum < numBlocks && same; blockNum++)
            {
                var block = _protocol.ReadBlock(blockNum);
                ConsoleWrite($"Verifying block '{blockNum}'", ConsoleColor.Gray);
                same = Compare(fileBytes[(blockNum * _BLOCK_SIZE)..((blockNum + 1) * _BLOCK_SIZE)], block); // TODO - partials!
                if (same)
                {
                    ConsoleWriteln("OK", ConsoleColor.Green);
                }
                else
                {
                    ConsoleWriteln("Error", ConsoleColor.Red);
                }
            }

            if (same)
            {
                ConsoleWriteln("Verified", ConsoleColor.Green);
            }
            else
            {
                // TODO
            }

            return true;
        }

        private static bool EraseEEPROM()
        {
            var fillPattern = new byte[_BLOCK_SIZE];
            Array.Fill<byte>(fillPattern, 0xFF);

            ConsoleWriteln("Erasing");
            for (ushort blockNum = 0; blockNum < _ROM_SIZE_BLOCKS; blockNum++)
            {
                ConsoleWrite($"Erasing block '{blockNum}'...", ConsoleColor.Gray);
                _protocol.WriteBlock(blockNum, fillPattern);  // TODO - partials!
                ConsoleWriteln("Done", ConsoleColor.Green);
            }
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