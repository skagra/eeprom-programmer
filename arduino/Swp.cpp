
// This code is taken from TommyPROM https://github.com/TomNisbet/TommyPROM/tree/master/unlock-ben-eater-hardware
// and slightly adapted

#include <Arduino.h>

#include "Swp.h"
#include "Pins.h"

using namespace EEPROMProgrammer;

void Swp::enableWrite()
{
    digitalWrite(EEPROM_WE_PIN, LOW);
}

void Swp::disableWrite()
{
    digitalWrite(EEPROM_WE_PIN, HIGH);
}

void Swp::setAddress(int addr, bool outputEnable)
{
    if (outputEnable)
    {
        // Modified for my HW design
        digitalWrite(EEPROM_OE_PIN, LOW);
    }
    else
    {
        // Modified for my HW design
        digitalWrite(EEPROM_OE_PIN, HIGH);
    }

    byte dataMask = 0x04;
    byte clkMask = 0x08;
    byte latchMask = 0x10;

    PORTD &= ~clkMask;

    for (uint16_t ix = 0; (ix < 16); ix++)
    {
        if (addr & 0x8000)
        {
            PORTD |= dataMask;
        }
        else
        {
            PORTD &= ~dataMask;
        }

        PORTD |= clkMask;
        delayMicroseconds(3);
        PORTD &= ~clkMask;
        addr <<= 1;
    }

    PORTD &= ~latchMask;
    delayMicroseconds(1);
    PORTD |= latchMask;
    delayMicroseconds(1);
    PORTD &= ~latchMask;
}

void Swp::disableSoftwareWriteProtect()
{
    disableWrite();
    setDataBusMode(OUTPUT);

    // Modified for AT28HC64B EEPROM
    setByte(0xaa, 0x1555);
    setByte(0x55, 0x0aaa);
    setByte(0x80, 0x1555);
    setByte(0xaa, 0x1555);
    setByte(0x55, 0x0aaa);
    setByte(0x20, 0x1555);

    setDataBusMode(INPUT);
    delay(10);
}

void Swp::enableSoftwareWriteProtect()
{
    disableWrite();
    setDataBusMode(OUTPUT);

    // Modified for AT28HC64B EEPROM
    setByte(0xaa, 0x1555);
    setByte(0x55, 0x0aaa);
    setByte(0xa0, 0x1555);

    setDataBusMode(INPUT);
    delay(10);
}

void Swp::setDataBusMode(uint8_t mode)
{
    if (mode == OUTPUT)
    {
        DDRB |= 0x1f;
        DDRD |= 0xe0;
    }
    else
    {
        DDRB &= 0xe0;
        DDRD &= 0x1f;
    }
}

void Swp::writeDataBus(byte data)
{
    PORTB = (PORTB & 0xe0) | (data >> 3);
    PORTD = (PORTD & 0x1f) | (data << 5);
}

void Swp::setByte(byte value, word address)
{
    setAddress(address, false);
    writeDataBus(value);

    delayMicroseconds(1);
    enableWrite();
    delayMicroseconds(1);
    disableWrite();
}
