#include <SoftwareSerial.h> 
SoftwareSerial HC05(0, 1); // RX | TX 

int potPin = A0;
String tempPotValue;

int redLedPin = 2;
int greenLedPin = 3;
int blueLedPin = 4;
int whiteLedPin = 5;

bool isConnectedToBluetooth = false;
bool isLoggedIn = false;

int tempBatteryValue = 0;

void setup() 
{   
   HC05.begin(9600); 
   
   pinMode(13, OUTPUT); 
   pinMode(redLedPin, OUTPUT); 
   pinMode(greenLedPin, OUTPUT); 
   pinMode(blueLedPin, OUTPUT); 
   pinMode(whiteLedPin, OUTPUT); 
   
   pinMode(potPin, INPUT);
} 

void loop() 
{   
  //Check if the bluetooth module received something
  if(HC05.available())
  {
    //Get the received value
    char flag = HC05.read();

    CheckForBluetoothMessage(flag);
  }
} 

void CheckForBluetoothMessage(char flag)
{
  // Never use SendMessageToApplication method with delay, 
  // otherwise the buffer gets filled really fast
  switch(flag)
  {
    case '0':
      //Connected To Bluetooth
      isConnectedToBluetooth = true;
      break;
    case '1':
      //Not Connected To Bluetooth
      isConnectedToBluetooth = false;
      break;
    case '2':
      //Log in signal
      isLoggedIn = true;
      digitalWrite(whiteLedPin, HIGH);
      break;
    case '3':
      //Log off signal
      isLoggedIn = false;
      digitalWrite(whiteLedPin, LOW);
      break;
    case '4':
      //Logged In
      
      String potValueText = String(analogRead(potPin));
      
      //Check if the value sent before is the same as the current
      if(tempPotValue != potValueText)
      {
        //Send the value
        SendMessageToApplication("2" , potValueText);
      }
      tempPotValue = potValueText;

      if(tempBatteryValue < 1023)
      {
        tempBatteryValue++;
        SendMessageToApplication("3" , String(tempBatteryValue));
      }
      else
      {
        tempBatteryValue = 0;
      }
      break;
  }

  if(isConnectedToBluetooth)
  {
    TurnOnLeds(0, 0, 1);
    
    if(isLoggedIn)
    {
      TurnOnLeds(0, 1, 1);
    }
    else
    {
      TurnOnLeds(1, 0, 1);
    }
  }
  else
  {
    TurnOnLeds(0, 0, 0);
  }
}

void TurnOnLeds(int redVal, int greenVal, int blueVal)
{
    digitalWrite(redLedPin, redVal);
    digitalWrite(greenLedPin, greenVal);
    digitalWrite(blueLedPin, blueVal);
}

void SendMessageToApplication(String messageId,String messageContent)
{
  //Send message to the phone using separators to parse messages -> "$", ":"
  HC05.print(messageId + "$" + messageContent + ":");
}

  
