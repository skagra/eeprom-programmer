#ifndef _PROTOCOL_DOT_H_
#define _PROTOCOL_DOT_H_

#include <Arduino.h>
#include "Programmer.h"

namespace EEPROMProgrammer
{
      class Protocol
      {
      private:
            static const int _BLOCK_SIZE = 64;

            static const int _SERIAL_INF_READ_BUFFER_SIZE = 0xFF;

            static const byte _OPCODE_IN_READ_BLOCK_REQUEST = 0x01;
            static const byte _OPCODE_OUT_READ_BLOCK_RESPONSE = 0x02;
            static const byte _OPCODE_IN_WRITE_BLOCK_REQUEST = 0x03;
            static const byte _OPCODE_OUT_WRITE_BLOCK_RESPONSE = 0x04;
            static const byte _OPCODE_OUT_OP_ERROR_RESPONSE = 0xF0;

            Programmer *_programmer;

            byte serialBuffer[_SERIAL_INF_READ_BUFFER_SIZE];

            int _serialBufferIndex = 0;
            bool _currentlyReading = false;
            byte _totalBytesToRead = 0;

            void readBlock(byte *buffer);
            void writeBlock(byte *buffer);

      public:
            Protocol(Programmer *programmer);
            void tick();
      };
}

#endif
