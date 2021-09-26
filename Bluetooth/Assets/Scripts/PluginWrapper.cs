using UnityEngine;

public class PluginWrapper : MonoBehaviour
{
	//Send the message as a regual string
	public static void SendBluetoothMessage(string id)
	{
		try
        {
			AndroidJavaClass jc = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
			AndroidJavaObject jo = jc.GetStatic<AndroidJavaObject>("currentActivity");
			jo.Call("SendBluetoothMessage", id);
        }
		catch (System.Exception e)
		{
			Debug.Log(e);
		}
	}

	public static void TurnOffBluetooth()
	{
		try
		{
			AndroidJavaClass jc = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
			AndroidJavaObject jo = jc.GetStatic<AndroidJavaObject>("currentActivity");
			jo.Call("TurnOffBluetooth");
		}
		catch (System.Exception e)
		{
			Debug.Log(e);
		}
	}


	public static string GetPairedDevices()
	{
		try
		{
			AndroidJavaClass jc = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
			AndroidJavaObject jo = jc.GetStatic<AndroidJavaObject>("currentActivity");
			return jo.Call<string>("GetPairedDevices");
		}
		catch (System.Exception e)
		{
			Debug.Log(e);
			return "";
		}
	}

	public static void ConnectToDevice(string address)
	{
		try
		{
			AndroidJavaClass jc = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
			AndroidJavaObject jo = jc.GetStatic<AndroidJavaObject>("currentActivity");
			jo.Call("ConnectToDevice", address);
		}
		catch (System.Exception e)
		{
			Debug.Log(e);
		}
	}

	public static string SetCaptorValue()
	{
		try
		{
			AndroidJavaClass jc = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
			AndroidJavaObject jo = jc.GetStatic<AndroidJavaObject>("currentActivity");
			return jo.Call<string>("SetValue");
		}
		catch (System.Exception e)
		{
			Debug.Log(e);
			return "";
		}
	}

	public static void SetNavigationBarColor(string id)
	{
		try
		{
			AndroidJavaClass jc = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
			AndroidJavaObject jo = jc.GetStatic<AndroidJavaObject>("currentActivity");
			jo.Call("SetNavigationBarColor", id);
		}
		catch (System.Exception e)
		{
			Debug.Log(e);
		}
	}

	public static void StatusBarState(string id)
	{
		try
		{
			AndroidJavaClass jc = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
			AndroidJavaObject jo = jc.GetStatic<AndroidJavaObject>("currentActivity");
			jo.Call("StatusBarState", id);
		}
		catch (System.Exception e)
		{
			Debug.Log(e);
		}
	}
}	
