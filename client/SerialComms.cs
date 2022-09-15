using System.IO.Ports;
using System.Management;
using System.Text.RegularExpressions;
using NLog;

namespace EEPROMProgrammer
{
    public class SerialComms
    {
        private const int READ_BUFFER_SIZE = 1024;
        private const int WRITE_BUFFER_SIZE = 1024;
        private const int READ_TIMEOUT_MILLIS = 5000;
        private const int WRITE_TIMEOUT_MILLIS = 100;

        private readonly Logger _logger = LogManager.GetCurrentClassLogger();
        private readonly SerialPort _serialPort;
        private readonly string _portName;

        public SerialComms(string portName, int baudRate)
        {
            _portName = portName ?? throw new ArgumentNullException(nameof(portName));

            _serialPort = new SerialPort(portName, baudRate, Parity.None, 8, StopBits.One)
            {
                ReadBufferSize = READ_BUFFER_SIZE,
                WriteBufferSize = WRITE_BUFFER_SIZE,
                ReadTimeout = READ_TIMEOUT_MILLIS,
                WriteTimeout = WRITE_TIMEOUT_MILLIS
            };

            _serialPort.Open();
        }

        public int Read(byte[] buffer, int offset, int count)
        {
            int result = 0;
            while (result == 0)
            {
                try
                {
                    result = _serialPort.Read(buffer, offset, count);
                }
                catch (TimeoutException)
                {
                    // Ignore
                }
            }
            return result;
        }

        public void Write(byte[] buffer, int offset, int count)
        {
            _serialPort.Write(buffer, offset, count);
        }

        public static string? FindArduinoComPort(string[] arduinoNames)
        {
            string? result = null;

            var regex = new Regex($"({string.Join("|", arduinoNames)}) \\((.+)\\)");

            var scope = new ManagementScope();
            var query = new SelectQuery("SELECT * FROM Win32_PnPEntity");
            var searcher = new ManagementObjectSearcher(scope, query);

            foreach (var item in searcher.Get())
            {
                if (((string)item["PNPClass"]) == "Ports")
                {
                    var name = (string)item["Name"];
                    if (name != null)
                    {
                        var parts = regex.Match(name);
                        if (parts.Success)
                        {
                            result = parts.Groups[2].Value;
                        }
                    }
                }
            }

            return result;
        }
    }
}