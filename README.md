# Arduino EEPROM Programmer

This project uses an *Arduino* as a `parallel EEPROM` programmer.

The circuit draws heavily on [Ben Eater's work](https://github.com/beneater/eeprom-programmer).  The approach to the software component though is rather different.

Ben included the data to be written to the Arduino in the actual sketch.  This project increases flexibility by adding a client-side application which drives an Arduino sketch via a protocol.

The client-side:

* Provides a simple UI.
* Drives the Arduino to:
  * Write a selected binary file to an `EEPROM`.
  * Verify the contents of a written `EEPROM`.
  * Read selected data from an `EEPROM`.
  * Erase an `EEPROM`.
* (De)marshalls the protocol to drive the Arduino.

The Arduino-side side:

* (De)marshalls the protocol and schedules operations.
* Writes data to an `EEPROM` as instructed via the protocol.
* Reads data from an `EEPROM` requested via the protocol and returns the result.

The project was created to write the decoder and microcode EEPROMs used in the [diy-cpu](https://github.com/skagra/diy-cpu) project.
