#include <avr/pgmspace.h>
#include <Wire.h>

const PROGMEM unsigned char binary[] = {
  #include "dest.h"
};


void setup() {
  // put your setup code here, to run once:
  Wire.begin();
  Serial.begin(115200, SERIAL_8N1);
  Serial.println(F("Run-length decoding"));
  Serial.println(F("(c)2021 tmct"));
  Serial.println(F("https://ss1.xrea.com/tmct.s1009.xrea.com/"));
  Serial.println("");
}

void loop() {
  // put your main code here, to run repeatedly:
  
  unsigned int dataPointer;
  unsigned int i;
  unsigned char runLength;
  unsigned char runData;

  dataPointer = 0;

  while (sizeof(binary) > dataPointer)
  {
    runLength = pgm_read_byte_near(binary + dataPointer);
    dataPointer++;
    if (sizeof(binary) == dataPointer)
    {
      // Invalid data
      Serial.println(F("!!Invalid!!"));
    }
    else
    {
      runData = pgm_read_byte_near(binary + dataPointer);
      dataPointer++;
      for (i = 0; i < runLength; i++)
      {
        Serial.write(runData);
      }
    }
  }
  
  Serial.println(F("Bye!"));
  
  while(1);
}
