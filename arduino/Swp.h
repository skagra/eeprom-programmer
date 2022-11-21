#ifndef _SWP_DOT_H_
#define _SWP_DOT_H_

#include <Arduino.h>

namespace EEPROMProgrammer
{
    class Swp
    {
    private:
        static void enableWrite();
        static void disableWrite();
        static void setAddress(int addr, bool outputEnable);

    public:
        static void setDataBusMode(uint8_t mode);
        static void writeDataBus(byte data);
        static void setByte(byte value, word address);
        static void disableSoftwareWriteProtect();
        static void enableSoftwareWriteProtect();
    };
};

#endif