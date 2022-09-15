#include "Programmer.h"

using namespace EEPROMProgrammer;

#define SHIFT_DATA 2
#define SHIFT_CLK 3
#define SHIFT_LATCH 4
#define EEPROM_D0 5
#define EEPROM_D7 12
#define EEPROM_WE 13

Programmer::Programmer()
{
    pinMode(SHIFT_DATA, OUTPUT);
    pinMode(SHIFT_CLK, OUTPUT);
    pinMode(SHIFT_LATCH, OUTPUT);
    digitalWrite(EEPROM_WE, HIGH);
    pinMode(EEPROM_WE, OUTPUT);
}

void Programmer::setAddress(unsigned short address, bool outputEnable)
{
    // Or'ing in outputEnable which is not part of the address as we've run out of GPIO pins.
    shiftOut(SHIFT_DATA, SHIFT_CLK, MSBFIRST, (address >> 8) | (outputEnable ? 0 : 0x80));
    shiftOut(SHIFT_DATA, SHIFT_CLK, MSBFIRST, address);

    // Pulse to latch the shift registers
    digitalWrite(SHIFT_LATCH, LOW);
    digitalWrite(SHIFT_LATCH, HIGH);
    digitalWrite(SHIFT_LATCH, LOW);
}

void Programmer::writeByte(byte data, unsigned short address)
{
    setAddress(address, false);

    // Set data pins to output
    // TODO can we replace this with a single register write?
    for (int pin = EEPROM_D0; pin <= EEPROM_D7; pin++)
    {
        pinMode(pin, OUTPUT);
    }

    byte msb = data & 0x80;

    for (int pin = EEPROM_D0; pin <= EEPROM_D7; pin++)
    {
        digitalWrite(pin, data & 1);
        data = data >> 1;
    }

    // Pulse W/E to trigger the EEPROM write
    digitalWrite(EEPROM_WE, LOW);
    delayMicroseconds(1);
    digitalWrite(EEPROM_WE, HIGH);

    // A completed write is flagged as complete when
    // the MS-bit is read back is equal to the valu written.
    do
    {
        delay(1);
    } while ((readByte(address) & 0x80) != msb);
}

byte Programmer::readByte(unsigned short address)
{
    // TODO can we replace this with a single register write?
    for (int pin = EEPROM_D0; pin <= EEPROM_D7; pin += 1)
    {
        pinMode(pin, INPUT);
    }
    setAddress(address, /*outputEnable*/ true);

    byte data = 0;
    for (int pin = EEPROM_D7; pin >= EEPROM_D0; pin -= 1)
    {
        data = (data << 1) + digitalRead(pin);
    }

    return data;
}
