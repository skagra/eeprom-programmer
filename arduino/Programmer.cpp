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
}

// Adapted from https://github.com/skirienkopanea/eeprom-programmer/blob/master/eeprom-programmer/eeprom-programmer.ino
// This is also doing the output endable disbale - seems like a hack ... make it explicit
void Programmer::setAddress(unsigned short address, bool outputEnable)
{
    shiftOut(SHIFT_DATA, SHIFT_CLK, MSBFIRST, (address >> 8) | (outputEnable ? 0 : 128));
    shiftOut(SHIFT_DATA, SHIFT_CLK, MSBFIRST, address);

    // Triggers a clock pulse for the shift register output clock signal
    digitalWrite(SHIFT_LATCH, !digitalRead(SHIFT_LATCH));
    digitalWrite(SHIFT_LATCH, !digitalRead(SHIFT_LATCH));
}

void Programmer::writeByte(byte data, unsigned short address)
{
    setAddress(address, false);

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

    digitalWrite(EEPROM_WE, LOW);
    delayMicroseconds(1);
    digitalWrite(EEPROM_WE, HIGH);

    do
    {
        delay(1);
    } while ((readByte(address) & 0x80) != msb);
}

byte Programmer::readByte(unsigned short address)
{
    for (int pin = EEPROM_D0; pin <= EEPROM_D7; pin++)
    {
        pinMode(pin, INPUT);
    }

    setAddress(address, true);

    byte data = 0;
    for (int pin = EEPROM_D7; pin >= EEPROM_D0; pin--)
    {
        data = (data << 1) + digitalRead(pin);
    }

    return data;
}
