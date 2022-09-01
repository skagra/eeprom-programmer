using NLog;

namespace EEPROMProgrammer
{
    public class Protocol
    {
        private const int BLOCK_SIZE = 64;

        private static readonly Logger _logger = LogManager.GetCurrentClassLogger();

        private const byte OPCODE_OUT_READ_BLOCK_REQUEST = 0x01;
        private const byte OPCODE_IN_READ_BLOCK_RESPONSE = 0x02;
        private const byte OPCODE_OUT_WRITE_BLOCK_REQUEST = 0x03;
        private const byte OPCODE_IN_WRITE_BLOCK_RESPONSE = 0x04;
        private const byte OPCODE_IN_OP_ERROR_RESPONSE = 0xF0;
        private readonly SerialComms _serialComms;

        public Protocol(SerialComms serialComms)
        {
            _serialComms = serialComms;
        }

        private void WriteMessage(byte operation, byte[] buffer)
        {
            var bufferSize = (byte)buffer.Length;
            if (bufferSize > 255)
            {
                throw new ArgumentOutOfRangeException($"Message limited to a maximum of 255 bytes");
            }
            _serialComms.Write(new byte[] { bufferSize, operation }, 0, 2);
            _serialComms.Write(buffer, 0, bufferSize);
        }

        private (byte op, byte[] buffer) ReadMessage()
        {
            var byteBuffer = new byte[1];
            _serialComms.Read(byteBuffer, 0, 1);
            byte sizeByte = byteBuffer[0];

            _logger.Debug("Incoming size byte '{0}'", sizeByte);

            // Read the rest of the packet
            byte[] buffer = new byte[sizeByte];
            var read = 0;
            while (read < sizeByte)
            {
                read += _serialComms.Read(buffer, read, sizeByte - read);
            }

            // Get the op code
            byte opCode = buffer[0];

            _logger.Debug("Incoming protocol '{0}'", (BitConverter.ToString(buffer)));

            return (buffer[0], buffer[1..]);
        }

        private (byte lsb, byte msb) ToBytes(ushort number)
        {
            return ((byte)(number & 0xFF), (byte)((number >> 8) & 0xFF));
        }

        private void ProcessPossibleError(byte op, byte[] buffer)
        {
            if (op == OPCODE_IN_OP_ERROR_RESPONSE)
            {
                throw new InvalidOperationException("Error returned from Arduino when processing request");
            }
        }

        public byte[] ReadBlock(ushort offset)
        {
            var len = ToBytes(offset);
            WriteMessage(OPCODE_OUT_READ_BLOCK_REQUEST, new byte[] { len.lsb, len.msb });

            var (op, buffer) = ReadMessage();

            ProcessPossibleError(op, buffer);

            if (op != OPCODE_IN_READ_BLOCK_RESPONSE)
            {
                throw new InvalidOperationException($"Expected response of type '{OPCODE_IN_READ_BLOCK_RESPONSE}' but got '{op}'");
            }

            if (buffer.Length != BLOCK_SIZE)
            {
                throw new InvalidOperationException($"Expected block of length '{BLOCK_SIZE}' but got '{buffer.Length}'");
            }

            return buffer;
        }

        public void WriteBlock(ushort offset, byte[] block)
        {
            if (block.Length != BLOCK_SIZE)
            {
                throw new ArgumentOutOfRangeException(nameof(block));
            }

            var offsetBytes = ToBytes(offset);
            byte[] buffer = new byte[2 + BLOCK_SIZE];
            buffer[0] = offsetBytes.lsb;
            buffer[1] = offsetBytes.msb;
            Array.Copy(block, 0, buffer, 2, BLOCK_SIZE);
            WriteMessage(OPCODE_OUT_WRITE_BLOCK_REQUEST, buffer);

            var (op, inBuffer) = ReadMessage();

            ProcessPossibleError(op, buffer);

            if (op != OPCODE_IN_WRITE_BLOCK_RESPONSE)
            {
                throw new InvalidOperationException($"Expected response of type '{OPCODE_IN_WRITE_BLOCK_RESPONSE}' but got '{op}'");
            }
        }
    }
}