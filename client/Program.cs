using static EEPROMProgrammer.ColorConsole;

namespace EEPROMProgrammer
{
    public class EEPROMProgrammerMain
    {
        private const string _VERSION = "0.1";
        private static Protocol _protocol;

        private static readonly string[] ARDUINO_NAMES = { "CH340", "Arduino Uno" };

        private static bool ReadEEPROM(UInt16 start, UInt16 length)
        {
            ConsoleWrite("READING", ConsoleColor.Cyan);
            return true;
        }

        private static bool WriteEEPROM(string fileName)
        {
            ConsoleWrite("WRITING", ConsoleColor.Cyan);
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
                var _protocol = new Protocol(serialComms);

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