#include "Protocol.h"
#include "Programmer.h"

using namespace EEPROMProgrammer;

Protocol *protocol;

void setup()
{
    Serial.begin(57600);
    protocol = new Protocol(new Programmer());
}

void loop()
{
    protocol->tick();
}