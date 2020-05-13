// Example 2 - Receive with an end-marker
#include <Servo.h>
Servo testServo;
const byte numChars = 32;
String receivedChars;   // an array to store the received data

boolean newData = false;

void setup() {
    Serial.begin(9600);
    Serial.println("<Arduino is ready>");
    testServo.attach(9);
}

void loop() {
    recvWithEndMarker();
    showNewData();
}

void recvWithEndMarker() {
    static byte nextIndex = 0;
    char endMarker = '\n';
    char receivedChar;
   
    while (Serial.available() > 0 && newData == false) {
        receivedChar = Serial.read();

        if (receivedChar != endMarker) {
            receivedChars+= receivedChar;
            nextIndex++;
            if (nextIndex >= numChars) {
                nextIndex = numChars - 1;
            }
        }
        else {
            //receivedChars[nextIndex] = '\0'; // terminate the string
            nextIndex = 0;
            newData = true;
        }
    }
}

void showNewData() {
    if (newData == true) {
        Serial.print("Angle:");
        int servoAngle = receivedChars.toInt();
        testServo.write(servoAngle);
        delay(15);
        Serial.println(servoAngle);
        receivedChars = "";
        newData = false;
    }
}
