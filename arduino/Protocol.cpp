#include "Protocol.h"

using namespace EEPROMProgrammer;

byte serialOuputBuffer[64];

Protocol::Protocol()
{
}

unsigned short DemarshallUshort(byte *buffer)
{
   return (unsigned short)(buffer[0] + (buffer[1] << 8));
}

void Protocol::readBlock(byte *buffer)
{
   unsigned short blockNumber = DemarshallUshort(buffer + 1);
   Serial.write((byte)65);
   Serial.write((byte)OPCODE_OUT_READ_BLOCK_RESPONSE);

   for (int i = 0; i < 64; i++)
   {
      serialOuputBuffer[i] = (byte)blockNumber;
   }
   Serial.write(serialOuputBuffer, 64);
   Serial.flush();
}

void Protocol::writeBlock(byte *buffer)
{
   unsigned short blockNumber = DemarshallUshort(buffer + 1);

   Serial.write((byte)1);
   Serial.write((byte)OPCODE_OUT_WRITE_BLOCK_RESPONSE);
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
            case OPCODE_IN_READ_BLOCK_REQUEST:
               readBlock(serialBuffer);
               break;
            case OPCODE_IN_WRITE_BLOCK_REQUEST:
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