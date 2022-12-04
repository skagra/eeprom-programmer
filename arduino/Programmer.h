#ifndef _PROGRAMMER_DOT_H_
#define _PROGRAMMER_DOT_H_

#include <Arduino.h>

namespace EEPROMProgrammer
{
    class Programmer
    {
    private:
        void setAddress(unsigned short address);
        void outputEnable(bool enable);

    public:
        Programmer();
        bool writeByte(byte data, unsigned short address);
        byte readByte(unsigned short address);
        void disableSoftwareWriteProtect();
        void enableSoftwareWriteProtect();
        void readBlock(unsigned short blockNumber, byte *buffer);
    };
}

#endif