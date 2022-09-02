#include "Protocol.h"

using namespace EEPROMProgrammer;

Protocol::Protocol(Programmer *programmer)
{
   _programmer = programmer;
}

unsigned short DemarshallUshort(byte *buffer)
{
   return (unsigned short)(buffer[0] + (buffer[1] << 8));
}

void Protocol::readBlock(byte *buffer)
{
   unsigned short blockNumber = DemarshallUshort(buffer + 1);

   Serial.write((byte)(_BLOCK_SIZE + 1));
   Serial.write((byte)_OPCODE_OUT_READ_BLOCK_RESPONSE);

   unsigned short addressBase = blockNumber * _BLOCK_SIZE;
   for (int addressOffset = 0; addressOffset < _BLOCK_SIZE; addressOffset++)
   {
      serialBuffer[addressOffset] = _programmer->readByte(addressBase + addressOffset);
   }

   Serial.write(serialBuffer, _BLOCK_SIZE);

   Serial.flush();
}

void Protocol::writeBlock(byte *buffer)
{
   unsigned short blockNumber = DemarshallUshort(buffer + 1);

   unsigned short addressBase = blockNumber * _BLOCK_SIZE;
   for (int addressOffset = 0; addressOffset < _BLOCK_SIZE; addressOffset++)
   {
      _programmer->writeByte(buffer[addressOffset + 3], addressBase + addressOffset);
   }

   Serial.write((byte)1);
   Serial.write((byte)_OPCODE_OUT_WRITE_BLOCK_RESPONSE);

   Serial.flush();
}

void Protocol::tick()
{
   byte currentByte;
   if (Serial.available())
   {
      currentByte = Serial.read();
      if (!_currentlyReading)
      {
         _serialBufferIndex = 0;
         if (currentByte > 0)
         {
            _totalBytesToRead = currentByte;
            _currentlyReading = true;
         }
      }
      else
      {
         serialBuffer[_serialBufferIndex] = currentByte;
         _serialBufferIndex++;
         if (_serialBufferIndex == _totalBytesToRead)
         {
            _currentlyReading = false;
            switch (serialBuffer[0])
            {
            case _OPCODE_IN_READ_BLOCK_REQUEST:
               readBlock(serialBuffer);
               break;
            case _OPCODE_IN_WRITE_BLOCK_REQUEST:
               writeBlock(serialBuffer);
               break;
            default:
               // ERROR
               break;
            }
         }
      }
   }
}