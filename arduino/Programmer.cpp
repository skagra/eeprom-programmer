#include "Programmer.h"

using namespace EEPROMProgrammer;

#define SHIFT_DATA 2
#define SHIFT_CLK 3
#define SHIFT_LATCH 4
#define EEPROM_D0 5
#define EEPROM_D7 12
#define EEPROM_WE 13
#define EEPROM_OE 14

Programmer::Programmer()
{
    pinMode(SHIFT_DATA, OUTPUT);
    pinMode(SHIFT_CLK, OUTPUT);
    pinMode(SHIFT_LATCH, OUTPUT);
    digitalWrite(EEPROM_WE, HIGH);
    pinMode(EEPROM_WE, OUTPUT);
    pinMode(EEPROM_OE, OUTPUT);
}

void Programmer::setAddress(unsigned short address)
{
    shiftOut(SHIFT_DATA, SHIFT_CLK, MSBFIRST, (address >> 8));
    shiftOut(SHIFT_DATA, SHIFT_CLK, MSBFIRST, address);

    // Pulse to latch the shift registers
    digitalWrite(SHIFT_LATCH, LOW);
    digitalWrite(SHIFT_LATCH, HIGH);
    digitalWrite(SHIFT_LATCH, LOW);
}

void Programmer::outputEnable(bool enable)
{
    if (enable)
    {
        digitalWrite(EEPROM_OE, LOW);
    }
    else
    {
        digitalWrite(EEPROM_OE, HIGH);
    }
}

void Programmer::writeByte(byte data, unsigned short address)
{
    setAddress(address);
    outputEnable(false);

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
    // the MS-bit is read back is equal to the value written.
    do
    {
        delay(1);
    } while ((readByte(address) & 0x80) != msb);
}

byte Programmer::readByte(unsigned short address)
{
    // TODO can we replace this with a single register write?
    for (int pin = EEPROM_D0; pin <= EEPROM_D7; pin++)
    {
        pinMode(pin, INPUT);
    }
    setAddress(address);
    outputEnable(true);

    byte data = 0;
    for (int pin = EEPROM_D7; pin >= EEPROM_D0; pin--)
    {
        data = (data << 1) + digitalRead(pin);
    }

    return data;
}
