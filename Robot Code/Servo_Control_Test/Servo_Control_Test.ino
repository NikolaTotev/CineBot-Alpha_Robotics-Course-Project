int stepInput = 8;  // LED connected to digital pin 13
int servoControl = 9;    // pushbutton connected to digital pin 7
Servo rotateServo;
int servoPosition=0;
void setup() {
  pinMode(ledPin, OUTPUT);  
  pinMode(inPin, INPUT);
  rotateServo.attach(servoControl)
}

void loop() {
  piInput = digitalRead(stepInput);

  if(piInput == HIGH)
  {
    servoPosition++;
    rotateServo.write(servoPosition);
     delay(10);    
  }
}
