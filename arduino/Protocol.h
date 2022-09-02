#ifndef _PROTOCOL_DOT_H_
#define _PROTOCOL_DOT_H_

#include <Arduino.h>

namespace EEPROMProgrammer
{
      class Protocol
      {
      private:
            static const int _SERIAL_INF_READ_BUFFER_SIZE = 0xFF;

            static const byte OPCODE_IN_READ_BLOCK_REQUEST = 0x01;
            static const byte OPCODE_OUT_READ_BLOCK_RESPONSE = 0x02;
            static const byte OPCODE_IN_WRITE_BLOCK_REQUEST = 0x03;
            static const byte OPCODE_OUT_WRITE_BLOCK_RESPONSE = 0x04;
            static const byte OPCODE_OUT_OP_ERROR_RESPONSE = 0xF0;

            void (*_errorCallback)(void *);
            void *_clientData;

            byte serialBuffer[_SERIAL_INF_READ_BUFFER_SIZE];
            int _serialBufferIndex = 0;
            bool _currentlyReading = false;
            byte _totalBytesToRead = 0;

            //  void sendOpCode(byte opCode);
            void readBlock(byte *buffer);
            void writeBlock(byte *buffer);

      public:
            Protocol();
            void tick();
      };
}

#endif
