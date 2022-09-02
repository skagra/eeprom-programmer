#ifndef _PROGRAMMER_DOT_H_
#define _PROGRAMMER_DOT_H_

#include <Arduino.h>

namespace EEPROMProgrammer
{
    class Programmer
    {
    private:
        void setAddress(unsigned short address, bool outputEnable);

    public:
        Programmer();
        void writeByte(byte data, unsigned short address);
        byte readByte(unsigned short address);
    };
}

#endif