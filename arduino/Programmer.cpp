#include "Programmer.h"

using namespace EEPROMProgrammer;

#include "Pins.h"
#include "Swp.h"

#define MAX_WRITE_CYCLES 100

Programmer::Programmer()
{
    pinMode(SHIFT_DATA_PIN, OUTPUT);
    pinMode(SHIFT_CLK_PIN, OUTPUT);
    pinMode(SHIFT_LATCH_PIN, OUTPUT);
    digitalWrite(EEPROM_WE_PIN, HIGH);
    pinMode(EEPROM_WE_PIN, OUTPUT);
    pinMode(EEPROM_OE_PIN, OUTPUT);
}

void Programmer::setAddress(unsigned short address)
{
    // for (int pin = EEPROM_D0_PIN; pin <= EEPROM_D7_PIN; pin++)
    // {
    //     pinMode(pin, OUTPUT);
    // }

    // Write the address to the shift registers
    shiftOut(SHIFT_DATA_PIN, SHIFT_CLK_PIN, MSBFIRST, (address >> 8));
    shiftOut(SHIFT_DATA_PIN, SHIFT_CLK_PIN, MSBFIRST, address);

    // Pulse to latch the shift registers
    digitalWrite(SHIFT_LATCH_PIN, LOW);
    digitalWrite(SHIFT_LATCH_PIN, HIGH);
    digitalWrite(SHIFT_LATCH_PIN, LOW);
}

void Programmer::outputEnable(bool enable)
{
    if (enable)
    {
        digitalWrite(EEPROM_OE_PIN, LOW);
    }
    else
    {
        digitalWrite(EEPROM_OE_PIN, HIGH);
    }
}

bool Programmer::writeByte(byte data, unsigned short address)
{
    bool result = false;

    // Set EEPROM address to write to
    setAddress(address);

    // We are writing
    outputEnable(false);

    // Set data pins to output
    for (int pin = EEPROM_D0_PIN; pin <= EEPROM_D7_PIN; pin++)
    {
        pinMode(pin, OUTPUT);
    }

    // To poll for completion of the write operation
    byte msb = data & 0x80;

    // Set the data to write to the EEPROM
    for (int pin = EEPROM_D0_PIN; pin <= EEPROM_D7_PIN; pin++)
    {
        digitalWrite(pin, data & 1);
        data >>= 1;
    }

    // Trigger write
    delayMicroseconds(1);
    digitalWrite(EEPROM_WE_PIN, LOW);
    delayMicroseconds(1);
    digitalWrite(EEPROM_WE_PIN, HIGH);

    // A completed write is flagged as complete when
    // the MS-bit read back is equal to the value written.
    int writeCycle = 0;
    while ((!result) && writeCycle < MAX_WRITE_CYCLES)
    {
        if ((readByte(address) & 0x80) == msb)
        {
            result = true;
        }
        else
        {
            delayMicroseconds(1);
            writeCycle++;
        }
    }
    return result;
}

byte Programmer::readByte(unsigned short address)
{
    // Set EEPROM address to write to
    setAddress(address);

    // We are reading
    outputEnable(true);

    for (int pin = EEPROM_D0_PIN; pin <= EEPROM_D7_PIN; pin++)
    {
        pinMode(pin, INPUT);
    }

    // Read the byte from the EEPROM
    byte data = 0;
    for (int pin = EEPROM_D7_PIN; pin >= EEPROM_D0_PIN; pin--)
    {
        data = (data << 1) + digitalRead(pin);
    }

    return data;
}

#define _BLOCK_SIZE 64

void Programmer::readBlock(unsigned short blockNumber, byte *buffer)
{
    DDRB &= 0b11100000; // Port B maps to pins D6 -> D13 (bits 6 and 7 and crystal pins) - reading pins 8->12
    DDRD &= 0b00011111; // Port D maps to pins D0 -> D7 -> reading pins 5->7

    outputEnable(true);

    unsigned short addressBase = blockNumber * _BLOCK_SIZE;
    for (int addressOffset = 0; addressOffset < _BLOCK_SIZE; addressOffset++)
    {
        setAddress(addressBase + addressOffset);
        buffer[addressOffset] = (PINB << 3) | (PIND & 0b11100000) >> 5;
    }
}

void Programmer::disableSoftwareWriteProtect()
{
    Swp::disableSoftwareWriteProtect();
}

void Programmer::enableSoftwareWriteProtect()
{
    Swp::enableSoftwareWriteProtect();
}
