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
    panServo.attach(3);
    rotationServo.attach(6);
    tiltServo.attach(5);

    panServo.write(75);
    rotationServo.write(80);
    tiltServo.write(80);
}

void loop() {
   // recvWithEndMarker();
   panServo.write(0);
   rotationServo.write(180);
   //tiltServo.write(15);
   delay(1000);

   panServo.write(175);
   rotationServo.write(0);
   //tiltServo.write(175);
   delay(1000);
   // showNewData();
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
        if(targetServo == 'P')
        {
            panServo.write(servoAngle);
            Serial.print("P"); 
        }

        if(targetServo == 'R')
        {
             rotationServo.write(servoAngle);
             Serial.print("R");
        }

        if(targetServo == 'T')
        {
              tiltServo.write(servoAngle);
              Serial.print("T"); 
        }
               
        gotTargetServo=false;
        delay(45);
        Serial.print("Angle:");
        Serial.println(servoAngle);
        receivedChars = "";
        newData = false;
    }
}
