//THIS WAS USED FOR THE DEMO LAST NIGHT
#include <Servo.h>
Servo panServo;
Servo rotationServo;
Servo tiltServo;

const byte numChars = 32;
String receivedChars;   // an array to store the received data
char targetServo;
bool gotTargetServo = false;
boolean newData = false;

void setup() {
    Serial.begin(9600);
    Serial.println("<Arduino is ready>");
    panServo.attach(9);
    rotationServo.attach(10);
    tiltServo.attach(11);
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
        if(!gotTargetServo)
        {
          targetServo = receivedChar;
          gotTargetServo=true;
        }
        else
        {
          if (receivedChar != endMarker) 
          {
            receivedChars+= receivedChar;
            nextIndex++;
            if (nextIndex >= numChars) {
                nextIndex = numChars - 1;
            }
          }
          else 
          {
            //receivedChars[nextIndex] = '\0'; // terminate the string
            nextIndex = 0;
            newData = true;
          }
        }      
    }
}

void showNewData() {
    if (newData == true) {
        
        int servoAngle = receivedChars.toInt();
        switch(targetServo){
        case 'P':
        panServo.write(servoAngle);
        Serial.print("P");
        break;

        case 'R':
        rotationServo.write(servoAngle);
        Serial.print("R");       
        break;

        case 'T':
        tiltServo.write(servoAngle);
        Serial.print("T");       
        break;
        }
        
        gotTargetServo=false;
        delay(45);
        Serial.print("Angle:");
        Serial.println(servoAngle);
        receivedChars = "";
        newData = false;
    }
}
