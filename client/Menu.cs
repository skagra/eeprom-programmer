namespace EEPROMProgrammer
{
    using NLog.LayoutRenderers.Wrappers;
    using NLog.Targets;
    using static ColorConsole;
    using static ColourValues;
    using static EEPROMDefinition;

    public class Menu
    {
        private readonly Action<ushort, ushort> _readRomCallback;
        private readonly Action<string> _writeRomCallback;
        private readonly Action _eraseRomCallback;
        private readonly Action _isRomEmptyCallback;
        private readonly Action _showConfigurationCallback;

        public Menu(Action<ushort, ushort> readRomCallback,
            Action<string> writeRomCallback, Action isRomEmptyCallback,
            Action eraseRomCallback, Action showConfigurationCallback)
        {
            _readRomCallback = readRomCallback ?? throw new ArgumentNullException(nameof(readRomCallback));
            _writeRomCallback = writeRomCallback ?? throw new ArgumentNullException(nameof(readRomCallback));
            _isRomEmptyCallback = isRomEmptyCallback ?? throw new ArgumentNullException(nameof(isRomEmptyCallback));
            _eraseRomCallback = eraseRomCallback ?? throw new ArgumentNullException(nameof(eraseRomCallback));
            _showConfigurationCallback = showConfigurationCallback ?? throw new ArgumentNullException(nameof(showConfigurationCallback));
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

            _showConfigurationCallback();
        }

        private void ShowError(string message)
        {
            ConsoleWriteln(message, COLOUR_ERROR);
        }

        private void ShowSuccess(string message)
        {
            ConsoleWriteln(message, COLOUR_OK);
        }

        // TODO - validation min/max
        private ushort GetUInt16(string prompt, ushort defaultValue, ushort minValue, ushort maxValue)
        {
            ushort result = defaultValue;

            var done = false;

            while (!done)
            {
                ConsoleWrite($"{prompt} [{defaultValue}]> ", COLOUR_PROMPT);
                var valueString = Console.ReadLine();
                if (valueString == null || valueString.Length == 0)
                {
                    done = true;
                }
                else
                {
                    if (ushort.TryParse(valueString, out result))
                    {
                        if (result >= minValue && result <= maxValue)
                        {
                            done = true;
                        }
                        else
                        {
                            ShowError($"Enter an integer value between {minValue} and {maxValue}");
                        }
                    }
                    else
                    {
                        ShowError("Invalid integer value");
                    }
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

            var startBlock = GetUInt16("Start block", 0, 0, _ROM_SIZE_BLOCKS - 1);
            var numBlocks = GetUInt16("Number of blocks", 1, 1, (ushort)(_ROM_SIZE_BLOCKS - startBlock));

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
                    ShowError("Please enter a choice between 1 and 6");
                    break;

            }

            if (!quit)
            {
                Console.WriteLine();
                ConsoleWrite("Press a key", COLOUR_PROMPT);
                Console.CursorVisible = false;
                Console.ReadKey();
                Console.CursorVisible = true;
            }

            return quit;
        }
    }
}