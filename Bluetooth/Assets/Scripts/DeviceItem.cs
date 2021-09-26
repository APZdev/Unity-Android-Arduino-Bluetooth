using System.Collections;
using UnityEngine;
using TMPro;

public class DeviceItem : MonoBehaviour
{
    public string deviceAdress;

    private TextMeshProUGUI deviceNameText;
    private MenuManager loginScript;

    public void SetItem(MenuManager instance, string name, string address)
    {
        deviceNameText = GetComponentInChildren<TextMeshProUGUI>();
        deviceNameText.text = name;
        deviceAdress = address;
        loginScript = instance;
    }

    public void OnClick_ConnectToDevice()
    {
        PluginWrapper.ConnectToDevice(deviceAdress);
        loginScript.CameraPosition(1);
    }
}
