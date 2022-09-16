#ifndef _PROTOCOL_DOT_H_
#define _PROTOCOL_DOT_H_

#include <Arduino.h>
#include "Programmer.h"

namespace EEPROMProgrammer
{
      class Protocol
      {
      private:
            // The size of EEPROM blocks to read/write
            static const int _BLOCK_SIZE = 64;

            // Size of protocol buffer
            static const int _SERIAL_BUFFER_SIZE = 0xFF;

            // Protocol op codes

            // Read a EEPROM block
            static const byte _OPCODE_IN_READ_BLOCK_REQUEST = 0x01;
            static const byte _OPCODE_OUT_READ_BLOCK_RESPONSE = 0x02;

            // Write an EEPROM block
            static const byte _OPCODE_IN_WRITE_BLOCK_REQUEST = 0x03;
            static const byte _OPCODE_OUT_WRITE_BLOCK_RESPONSE = 0x04;

            // Flag an error
            static const byte _OPCODE_OUT_OP_ERROR_RESPONSE = 0xF0;

            // Low level EEPROM programmer
            Programmer *_programmer;

            // Protocol buffer
            byte serialBuffer[_SERIAL_BUFFER_SIZE];

            // Protocol buffer read index
            int _serialBufferIndex = 0;

            // Are we reading mid packet?
            bool _currentlyReading = false;

            // Size of the current protocol pack,
            // not including the size byte itself
            byte _totalBytesToRead = 0;

            // Read a block from the EEPROM
            void readBlock();

            // Write a block to the EEPROM
            void writeBlock();

      public:
            Protocol(Programmer *programmer);

            // Process incoming protocol
            void tick();
      };
}

#endif
