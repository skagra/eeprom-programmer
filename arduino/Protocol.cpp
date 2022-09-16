#include "Protocol.h"

using namespace EEPROMProgrammer;

Protocol::Protocol(Programmer *programmer)
{
   _programmer = programmer;
}

unsigned short demarshallUshort(byte *buffer)
{
   // Little endian
   return (unsigned short)(buffer[0] + (buffer[1] << 8));
}

void Protocol::readBlock()
{
   // Skip the op code byte - then content is the requested block number
   unsigned short blockNumber = demarshallUshort(serialBuffer + 1);

   // Protocol packet size
   Serial.write((byte)(_BLOCK_SIZE + 1));

   // Protocol op code
   Serial.write((byte)_OPCODE_OUT_READ_BLOCK_RESPONSE);

   // Packet content - a EEPROM block
   unsigned short addressBase = blockNumber * _BLOCK_SIZE;
   for (int addressOffset = 0; addressOffset < _BLOCK_SIZE; addressOffset++)
   {
      serialBuffer[addressOffset] = _programmer->readByte(addressBase + addressOffset);
   }

   // Write the result
   Serial.write(serialBuffer, _BLOCK_SIZE);
   Serial.flush();
}

void Protocol::writeBlock()
{
   // Skip the op code byte - the get the block number to write
   unsigned short blockNumber = demarshallUshort(serialBuffer + 1);

   // Write the block of data to the EEPROM
   unsigned short addressBase = blockNumber * _BLOCK_SIZE;
   for (int addressOffset = 0; addressOffset < _BLOCK_SIZE; addressOffset++)
   {
      // +3 offset skipping op code byte and block number short
      _programmer->writeByte(serialBuffer[addressOffset + 3], addressBase + addressOffset);
   }

   // Write the response protocol size (just an op code)
   Serial.write((byte)1);

   // Write the response
   Serial.write((byte)_OPCODE_OUT_WRITE_BLOCK_RESPONSE);
   Serial.flush();
}

void Protocol::tick()
{
   byte currentByte;
   if (Serial.available())
   {
      // Read a protocol byte
      currentByte = Serial.read();

      // Are we in the middle of reading a packet?
      if (!_currentlyReading)
      {
         // No - so this is new packet
         _serialBufferIndex = 0;
         if (currentByte > 0)
         {
            // And the first byte is the size of the remainder of the packet
            _totalBytesToRead = currentByte;
            _currentlyReading = true;
         }
      }
      else
      {
         // Store the current byte
         serialBuffer[_serialBufferIndex] = currentByte;
         _serialBufferIndex++;
         if (_serialBufferIndex == _totalBytesToRead)
         {
            // Done reading - so use the op code byte call the
            // associated routine
            _currentlyReading = false;
            switch (serialBuffer[0])
            {
            case _OPCODE_IN_READ_BLOCK_REQUEST:
               readBlock();
               break;
            case _OPCODE_IN_WRITE_BLOCK_REQUEST:
               writeBlock();
               break;
            default:
               // ERROR
               break;
            }
         }
      }
   }
}