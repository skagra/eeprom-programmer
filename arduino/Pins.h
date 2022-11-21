#ifndef _PINS_DOT_H_
#define _PINS_DOT_H_

// EEPROM pin numbers for AT28C64
// Change for different sizes of EEPROM
#define SHIFT_DATA_PIN 2  // -> Shift register 1 SER pin (14)
#define SHIFT_CLK_PIN 3   // -> Shift registers SRCLCK pins (11)
#define SHIFT_LATCH_PIN 4 // -> Shift registers RCLC pins (12)
#define EEPROM_D0_PIN 5   // -> EEPROM LSB data pin
#define EEPROM_D7_PIN 12  // -> EEPROM MSB data pin
#define EEPROM_WE_PIN 13  // -> EEPROM ~WE pin (27)
#define EEPROM_OE_PIN 14  // -> EEPROM ~OE pin (22)

#endif
