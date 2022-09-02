#include "Protocol.h"

using namespace EEPROMProgrammer;

Protocol *protocol;

void setup()
{
    Serial.begin(9600);
    protocol = new Protocol();
}

void loop()
{
    protocol->tick();
}