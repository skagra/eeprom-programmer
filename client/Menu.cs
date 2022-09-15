namespace EEPROMProgrammer
{
    using NLog.LayoutRenderers.Wrappers;
    using NLog.Targets;
    using static ColorConsole;
    using static ColourValues;
    using static EEPROMDefinition;

    public class Menu
    {
        private readonly string _version;
        private readonly string _serialPort;

        private readonly Action<ushort, ushort> _readRomCallback;
        private readonly Action<string> _writeRomCallback;
        private readonly Action _eraseRomCallback;
        private readonly Action _isRomEmptyCallback;

        public Menu(string serialPort, string version, Action<ushort, ushort> readRomCallback,
            Action<string> writeRomCallback, Action isRomEmptyCallback,
            Action eraseRomCallback)
        {
            _serialPort = serialPort ?? throw new ArgumentNullException(nameof(serialPort));
            _version = version ?? throw new ArgumentNullException(nameof(version));
            _readRomCallback = readRomCallback ?? throw new ArgumentNullException(nameof(readRomCallback));
            _writeRomCallback = writeRomCallback ?? throw new ArgumentNullException(nameof(readRomCallback));
            _isRomEmptyCallback = isRomEmptyCallback ?? throw new ArgumentNullException(nameof(isRomEmptyCallback));
            _eraseRomCallback = eraseRomCallback ?? throw new ArgumentNullException(nameof(eraseRomCallback));
        }

        private void ShowMenu()
        {
            ConsoleWriteln("EEPROM Programmer", COLOUR_TITLE);
            Console.WriteLine();
            ConsoleWriteln("1) Read EEPROM", COLOUR_BODY);
            ConsoleWriteln("2) Write EEPROM", COLOUR_BODY);
            ConsoleWriteln("3) Erase EEPROM", COLOUR_BODY);
            ConsoleWriteln("4) Check EEPROM is empty", COLOUR_BODY);
            ConsoleWriteln("5) Show configuration", COLOUR_BODY);
            ConsoleWriteln("6) Exit", COLOUR_BODY);
            Console.WriteLine();
            ConsoleWrite("Enter choice> ", COLOUR_PROMPT);
        }

        private void ShowVersion()
        {
            ConsoleClear();

            const int boxWidth = 40;
            var border = new string('#', boxWidth);
            var wrapper = $"#{new string(' ', boxWidth - 2)}#";

            Console.CursorTop = VerticalCentreCursor(3);
            ConsoleWritelnMiddle(border, COLOUR_TITLE);
            ConsoleWritelnMiddle(wrapper, COLOUR_TITLE);
            ConsoleWriteMiddle(wrapper, COLOUR_TITLE);
            ConsoleWritelnMiddle($"EEPROM Programmer version {_version}", COLOUR_BODY);
            ConsoleWriteMiddle(wrapper, COLOUR_TITLE);
            ConsoleWritelnMiddle($"Arduino Port: {_serialPort}", COLOUR_BODY);
            ConsoleWriteMiddle(wrapper, COLOUR_TITLE);
            ConsoleWritelnMiddle($"EEPROM Size: {_ROM_SIZE_BYTES} bytes", COLOUR_BODY);
            ConsoleWritelnMiddle(wrapper, COLOUR_TITLE);
            ConsoleWritelnMiddle(border, COLOUR_TITLE);
        }

        private void ShowError(string message)
        {
            ConsoleWriteln(message, COLOUR_ERROR);
        }

        private void ShowSuccess(string message)
        {
            ConsoleWriteln(message, COLOUR_OK);
        }

        private ushort GetUInt16(string prompt, ushort defaultValue)
        {
            ushort result = defaultValue;

            var ok = false;

            while (!ok)
            {
                ConsoleWrite($"{prompt} [{defaultValue}]> ", COLOUR_PROMPT);
                var valueString = Console.ReadLine();
                if (valueString == null || valueString.Length == 0)
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

            ConsoleWrite($"{prompt} [{defaultValue}]> ", COLOUR_PROMPT);
            var valueString = Console.ReadLine();
            if (valueString.Length > 0)
            {
                result = valueString;
            }
            return result;
        }

        private void ReadRom()
        {
            ConsoleClear();
            ConsoleWriteln("Read EEPROM", COLOUR_TITLE);
            Console.WriteLine();

            var startBlock = GetUInt16("Start block", 0);
            var numBlocks = GetUInt16("Number of blocks", _ROM_SIZE_BLOCKS); // TODO

            Console.WriteLine();
            _readRomCallback(startBlock, numBlocks);
        }

        private void WriteRom()
        {
            ConsoleClear();
            ConsoleWriteln("Write EEPROM", COLOUR_TITLE);
            Console.WriteLine();

            var fileName = GetString("ROM filename", "ROM.bin");

            _writeRomCallback(fileName);
        }

        private void EraseRom()
        {
            ConsoleClear();
            ConsoleWriteln("Erase EEPROM", COLOUR_TITLE);
            Console.WriteLine();

            _eraseRomCallback();
        }

        private void CheckRomIsEmpty()
        {
            ConsoleClear();
            ConsoleWriteln("Check whether EEPROM is empty", COLOUR_TITLE);
            Console.WriteLine();

            _isRomEmptyCallback();
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
                    CheckRomIsEmpty();
                    break;
                case ConsoleKey.D5:
                    ShowVersion();
                    break;
                case ConsoleKey.D6:
                    quit = true;
                    break;
                default:
                    Console.WriteLine();
                    ShowError("Please enter a choice between 1 and 5");
                    break;

            }

            if (!quit)
            {
                Console.WriteLine();
                ConsoleWriteln("Press a key", COLOUR_PROGRESS);
                Console.ReadKey();
            }

            return quit;
        }
    }
}