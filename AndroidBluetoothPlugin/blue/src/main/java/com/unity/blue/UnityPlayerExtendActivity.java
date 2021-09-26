package com.unity.blue;

import com.unity3d.player.UnityPlayerActivity;

import java.io.IOException;
import java.io.InputStream;
import java.io.OutputStream;
import java.util.Set;
import java.util.UUID;

import android.app.NotificationChannel;
import android.app.NotificationManager;
import android.graphics.Color;
import android.os.Build;
import android.os.Bundle;
import android.widget.Toast;
import android.bluetooth.BluetoothAdapter;
import android.bluetooth.BluetoothDevice;
import android.bluetooth.BluetoothSocket;
import android.content.Context;
import android.content.Intent;

import androidx.annotation.RequiresApi;
import androidx.core.app.NotificationCompat;
import androidx.core.app.NotificationManagerCompat;

public class UnityPlayerExtendActivity extends UnityPlayerActivity
{
    private Context mContext = null;

    // Intent request codes
    private static final int REQUEST_ENABLE_BT = 3;

    public BluetoothAdapter myBluetooth = null;
    private BluetoothSocket btSocket = null;
    private Set<BluetoothDevice> pairedDevices;
    static final UUID myUUID = UUID.fromString("00001101-0000-1000-8000-00805F9B34FB");

    private String connectedDeviceAddress;
    private Boolean alreadyConnected;

    private ConnectedThread mConnectedThread;
    private String tempReadMessage;
    private String finalReadMessage;
    private String valueToSend;

    @RequiresApi(api = Build.VERSION_CODES.P)
    @Override
    public void onCreate(Bundle savedInstanceState)
    {
        super.onCreate(savedInstanceState);
        mContext = this;

        myBluetooth = BluetoothAdapter.getDefaultAdapter();
        alreadyConnected = false;

        // If the adapter is null, then Bluetooth is not supported
        if (myBluetooth == null)
        {
            CallToast("No Bluetooth Found");
            this.finish();
        }
    }

    @Override
    public void onStart()
    {
        super.onStart();

        // If Bluetooth is not on, request that it be enabled.
        if (!myBluetooth.isEnabled())
        {
            //CallToast("Enable Bluetooth");
            Intent enabler = new Intent(BluetoothAdapter.ACTION_REQUEST_ENABLE);
            startActivityForResult(enabler, REQUEST_ENABLE_BT);
        }
    }

    @Override
    public void onResume()
    {
        super.onResume();

        //Check if we've already been connected
        if(alreadyConnected)
        {
            try
            {
                //Connect to previous connected device
                ConnectToDevice(connectedDeviceAddress);
            }
            catch (IOException e)
            {
                CallToast("Socket creation failed");
            }
        }
    }

    @Override
    public void onPause()
    {
        super.onPause();

        TurnOffBluetooth();
    }

    //https://wingoodharry.wordpress.com/2014/04/15/android-sendreceive-data-with-arduino-using-bluetooth-part-2/
    //create new class for connect thread
    private class ConnectedThread extends Thread
    {
        private final InputStream mmInStream;
        private final OutputStream mmOutStream;

        //Creation of the connect thread
        public ConnectedThread(BluetoothSocket socket)
        {
            InputStream tmpIn = null;
            OutputStream tmpOut = null;

            try
            {
                //Create I/O streams for connection
                tmpIn = socket.getInputStream();
                tmpOut = socket.getOutputStream();
            } catch (IOException e) { }

            mmInStream = tmpIn;
            mmOutStream = tmpOut;
        }

        public void run()
        {
            byte[] buffer = new byte[256];
            int bytes;

            // Keep looping to listen for received messages
            while (true)
            {
                try
                {
                    //Read bytes from input buffer
                    bytes = mmInStream.read(buffer);
                    String readMessage = new String(buffer, 0, bytes);
                    //Add the received message to a queue
                    tempReadMessage += readMessage;

                    //Check if we received a fully usable message
                    if(tempReadMessage.contains(":"))
                    {
                        //Parse the received message
                        String[] finalMessageContent = tempReadMessage.split(":");

                        //Check if the message contains something
                        if(finalMessageContent.length > 1)
                        {
                            //Store the received message
                            tempReadMessage = finalMessageContent[1];
                        }
                        else
                        {
                            tempReadMessage = "";
                        }

                        //Check if the value has changed to save useless messages
                        if(finalReadMessage != finalMessageContent[0])
                        {
                            //Set "valueToSend" value because it's the one returned to unity on the "SetValue" method
                            valueToSend = finalMessageContent[0];
                            SetValue();
                        }

                        //Store the value to know if the next message is different from the current
                        finalReadMessage = finalMessageContent[0];
                    }
                }
                catch (IOException e)
                {
                    break;
                }
            }
        }

        //write method
        public void WriteToArduino (String input)
        {
            //converts entered String into bytes
            byte[] msgBuffer = input.getBytes();

            try
            {
                //write bytes over BT connection via outstream
                mmOutStream.write(msgBuffer);
            }
            catch (IOException e)
            {
                //CallToast("Connection Failure");
            }
        }
    }

    public void CallToast(final String text)
    {
        runOnUiThread(() -> Toast.makeText(mContext, text, Toast.LENGTH_SHORT).show());
    }




    /// Unity Methods ///

    public String SetValue()
    {
        return valueToSend;
    }

    @RequiresApi(api = Build.VERSION_CODES.LOLLIPOP)
    public void StatusBarState(String rgba)
    {
        //hide 1024
        //show 2048
        mUnityPlayer.setSystemUiVisibility(2048);
        getWindow().setStatusBarColor(Color.parseColor(rgba));
    }

    @RequiresApi(api = Build.VERSION_CODES.LOLLIPOP)
    public void SetNavigationBarColor(String rgba)
    {
        getWindow().setNavigationBarColor(Color.parseColor(rgba));
    }

    public void TurnOffBluetooth()
    {
        try
        {
            //Tell the arduino the bluetooth connection is closed
            SendBluetoothMessage("1");

            connectedDeviceAddress = "";


            //Don't leave Bluetooth sockets open when leaving activity;
            btSocket.close();
        }
        catch (IOException e2)
        {
            //insert code to deal with this
        }
    }

    public void SendBluetoothMessage(String message)
    {
        try
        {
            if (btSocket != null)
            {
                mConnectedThread.WriteToArduino(message);
            }

        }
        catch (Exception e)
        {
            CallToast(e.getMessage());
        }
    }

    public String GetPairedDevices()
    {
        String listOfDevices = "";
        myBluetooth = BluetoothAdapter.getDefaultAdapter();
        pairedDevices = myBluetooth.getBondedDevices();
        if (pairedDevices.size() > 0)
        {
            for (BluetoothDevice bt : pairedDevices)
            {
                //Format the devices name and address to send to the app
                listOfDevices += bt.getName() + "&" + bt.getAddress() + "$";
            }
        }
        else
        {
            return "";
        }

        //Send the device list to the app
        return listOfDevices;
    }

    public void ConnectToDevice(String address) throws IOException
    {
        //Get the mobile bluetooth adapter
        myBluetooth = BluetoothAdapter.getDefaultAdapter();
        //Connects to the device's address and checks if it's available
        BluetoothDevice bluetoothDevice = myBluetooth.getRemoteDevice(address);
        //Create a RFCOMM (SPP) insecure connection -> https://www.btframework.com/rfcomm.htm
        btSocket = bluetoothDevice.createInsecureRfcommSocketToServiceRecord(myUUID);
        // Establish the Bluetooth socket connection.
        try
        {
            btSocket.connect();
            //Store the bluetooth address to reconnect the bluetooth when onResume method is called
            connectedDeviceAddress = address;
            //Stored to verify if we've already been connected to the bluetooth module and reconnect instantly when onResume method is called
            alreadyConnected = true;
            //CallToast("Connected to : " + address);
        }
        catch (IOException e)
        {
            try
            {
                btSocket.close();
                CallToast("Error : " + e);
            }
            catch (IOException e2)
            {
                //insert code to deal with this
            }
        }

        //Once the connection with bluetooth module is established, start the parallel thread that keep track of message comming from the Arduino
        mConnectedThread = new ConnectedThread(btSocket);
        mConnectedThread.start();

        //Send connected to bluetooth data to arduino
        SendBluetoothMessage("0");
    }
}



