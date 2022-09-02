namespace EEPROMProgrammer
{
    using static ColorConsole;
    using static EEPROMDefinition;

    public class Menu
    {
        private readonly string _version;
        private readonly string _serialPort;

        private readonly Func<ushort, ushort, bool> _readRomCallback;
        private readonly Func<string, bool> _writeRomCallback;
        private readonly Func<bool> _eraseRomCallback;

        public Menu(string serialPort, string version, Func<ushort, ushort, bool> readRomCallback,
            Func<string, bool> writeRomCallback,
            Func<bool> eraseRomCallback)
        {
            _serialPort = serialPort ?? throw new ArgumentNullException(nameof(serialPort));
            _version = version ?? throw new ArgumentNullException(nameof(version));
            _readRomCallback = readRomCallback ?? throw new ArgumentNullException(nameof(readRomCallback));
            _writeRomCallback = writeRomCallback ?? throw new ArgumentNullException(nameof(readRomCallback));
            _eraseRomCallback = eraseRomCallback ?? throw new ArgumentNullException(nameof(eraseRomCallback));
        }

        private void ShowMenu()
        {
            ConsoleWriteln("EEPROM Programmer", ConsoleColor.Yellow);
            Console.WriteLine();
            ConsoleWriteln("1) Read EEPROM");
            ConsoleWriteln("2) Write EEPROM");
            ConsoleWriteln("3) Erase EEPROM");
            ConsoleWriteln("4) Show configuration");
            ConsoleWriteln("5) Exit");
            Console.WriteLine();
            ConsoleWrite("Enter choice> ", ConsoleColor.Yellow);
        }

        private void ShowVersion()
        {
            ConsoleClear();

            const int boxWidth = 40;
            var border = new string('#', boxWidth);
            var wrapper = $"#{new string(' ', boxWidth - 2)}#";

            Console.CursorTop = VerticalCentreCursor(3);
            ConsoleWritelnMiddle(border, ConsoleColor.Yellow);
            ConsoleWritelnMiddle(wrapper, ConsoleColor.Yellow);
            ConsoleWriteMiddle(wrapper, ConsoleColor.Yellow);
            ConsoleWritelnMiddle($"EEPROM Programmer version {_version}");
            ConsoleWriteMiddle(wrapper, ConsoleColor.Yellow);
            ConsoleWritelnMiddle($"Arduino Port: {_serialPort}");
            ConsoleWriteMiddle(wrapper, ConsoleColor.Yellow);
            ConsoleWritelnMiddle($"EEPROM Size: {_ROM_SIZE_BYTES} bytes");
            ConsoleWritelnMiddle(wrapper, ConsoleColor.Yellow);
            ConsoleWritelnMiddle(border, ConsoleColor.Yellow);

            Console.CursorVisible = false;
            Console.ReadKey();
            ConsoleClear();
            Console.CursorVisible = true;
        }

        private void ShowError(string message)
        {
            ConsoleWriteln(message, ConsoleColor.Red);
        }

        private void ShowSuccess(string message)
        {
            ConsoleWriteln(message, ConsoleColor.Green);
        }

        private ushort GetUInt16(string prompt, ushort defaultValue)
        {
            ushort result = defaultValue;

            var ok = false;

            while (!ok)
            {
                ConsoleWrite($"{prompt} [{defaultValue}]> ", ConsoleColor.Yellow);
                var valueString = Console.ReadLine();
                if (valueString.Length == 0)
                {
                    ok = true;
                }
                else
                {
                    ok = ushort.TryParse(valueString, out result);
                }
                if (!ok)
                {
                    ShowError("Invalid integer value");
                }
            }

            return result;

        }

        private string GetString(string prompt, string defaultValue)
        {
            var result = defaultValue;

            ConsoleWrite($"{prompt} [{defaultValue}]> ", ConsoleColor.Yellow);
            var valueString = Console.ReadLine();
            if (valueString.Length > 0)
            {
                result = valueString;
            }
            return result;
        }

        // TODO Validation
        private void ReadRom()
        {
            ConsoleClear();
            ConsoleWriteln("Read EEPROM");
            Console.WriteLine();

            var startBlock = GetUInt16("Start block", 0);
            var numBlocks = GetUInt16("Number of blocks", _ROM_SIZE_BLOCKS); // TODO

            Console.WriteLine();
            var ok = _readRomCallback(startBlock, numBlocks);

            if (ok)
            {
                Console.WriteLine();
                ShowSuccess("EEPROM read successful");
            }
            else
            {
                Console.WriteLine();
                ShowError("Error while read EEPROM");
            }

            Console.ReadKey();
        }

        private void WriteRom()
        {
            ConsoleClear();
            Console.WriteLine("Write EEPROM");
            Console.WriteLine();

            var fileName = GetString("ROM filename", "ROM.bin");
            ConsoleWriteln($"Writing EEPRROM from '{fileName}'", ConsoleColor.Yellow);

            var ok = _writeRomCallback(fileName);
            if (ok)
            {
                Console.WriteLine();
                ShowSuccess("EEPROM write successful");
            }
            else
            {
                Console.WriteLine();
                ShowError("Error while writing EEPROM");
            }

            Console.ReadKey();

        }

        private void EraseRom()
        {
            ConsoleClear();
            Console.WriteLine("Erase EEPROM");
            Console.WriteLine();

            var ok = _eraseRomCallback();

            if (ok)
            {
                Console.WriteLine();
                ShowSuccess("EEPROM erase successful");
            }
            else
            {
                Console.WriteLine();
                ShowError("Error while erasing EEPROM");
            }

            Console.ReadKey();
        }

        public bool Process()
        {
            var quit = false;

            ConsoleClear();
            ShowMenu();
            var key = Console.ReadKey();
            switch (key.Key)
            {
                case ConsoleKey.D1:
                    ReadRom();
                    break;
                case ConsoleKey.D2:
                    WriteRom();
                    break;
                case ConsoleKey.D3:
                    EraseRom();
                    break;
                case ConsoleKey.D4:
                    ShowVersion();
                    break;
                case ConsoleKey.D5:
                    quit = true;
                    break;
                default:
                    Console.WriteLine();
                    ShowError("Please enter a choice between 1 and 5");
                    Console.ReadKey();
                    break;

            }

            return quit;
        }
    }
}