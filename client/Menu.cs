using static EEPROMProgrammer.ColorConsole;

namespace EEPROMProgrammer
{
    public class Menu
    {
        private readonly string _version;
        private readonly string _serialPort;

        private readonly Func<UInt16, UInt16, bool> _readRomCallback;
        private readonly Func<string, bool> _writeRomCallback;
        private readonly Func<bool> _eraseRomCallback;

        public Menu(string serialPort, string version, Func<UInt16, UInt16, bool> readRomCallback,
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
            ConsoleWrite("EEPROM Programmer", ConsoleColor.Yellow);
            Console.WriteLine($" on port '{_serialPort}'", ConsoleColor.Gray);
            Console.WriteLine();
            ConsoleWriteln("1) Read EEPROM");
            ConsoleWriteln("2) Write EEPROM");
            ConsoleWriteln("3) Erase EEPROM");
            ConsoleWriteln("4) Show version");
            ConsoleWriteln("5) Exit");
            Console.WriteLine();
            ConsoleWrite("Enter choice> ", ConsoleColor.Yellow);
        }

        private void ShowVersion()
        {
            ConsoleClear();
            var message = $"# EEPROM Programmer version {_version} #";
            var messageLength = message.Length;
            var messageLeft = HorizontalCentreCursor(message);
            var border = new string('#', messageLength);

            Console.CursorTop = VerticalCentreCursor(3);

            Console.CursorLeft = messageLeft;
            ConsoleWriteln(border, ConsoleColor.Yellow);
            Console.CursorLeft = messageLeft;
            ConsoleWriteln(message, ConsoleColor.Yellow);
            Console.CursorLeft = messageLeft;
            ConsoleWriteln(border, ConsoleColor.Yellow);

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

        private UInt16 GetUInt16(string prompt, UInt16 defaultValue)
        {
            UInt16 result = defaultValue;

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
                    ok = UInt16.TryParse(valueString, out result);
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

        private void ReadRom()
        {
            ConsoleClear();
            ConsoleWriteln("Read EEPROM");
            Console.WriteLine();

            var start = GetUInt16("Start address", 0);
            var length = GetUInt16("Length", 0); // TODO

            Console.WriteLine();
            ConsoleWriteln($"Reading '{length}' bytes starting at '{start}'", ConsoleColor.Yellow);

            var ok = _readRomCallback(start, length);

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