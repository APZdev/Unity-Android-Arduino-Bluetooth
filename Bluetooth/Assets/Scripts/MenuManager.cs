using System.Collections;
using UnityEngine.UI;
using UnityEngine;
using TMPro;

public class MenuManager : MonoBehaviour
{
    #region UI Elements Variables

    [Header("Global")]
    [SerializeField] private GameObject uiPannel = null;
    [SerializeField] private float cameraLerpTime = 10f;
    private float finalPosition = -6f;

    [Header("Search Device")]
    [SerializeField] private GameObject deviceItemPrefab = null;
    [SerializeField] private Transform deviceItemHolder = null;

    [Header("Login")]
    [SerializeField] private GameObject loginPanel;
    [SerializeField] private InputField passwordInput = null;
    public string password = "";
    [SerializeField] private TextMeshProUGUI loginStatusText = null;
    private bool isConnected;

    [Header("Change Password")]
    [SerializeField] private InputField currentPasswordInput = null;
    [SerializeField] private InputField newPasswordInput = null;
    [SerializeField] private TextMeshProUGUI changePasswordStatusText = null;


    [SerializeField] private GameObject[] menuBtn = null;
    [SerializeField] private GameObject[] pannelObjects = null;

    [SerializeField] private Image batteryPercentageCircle = null;
    [SerializeField] private Text batteryPercentageTxt = null;
    [SerializeField] private Text batteryTxt = null;

    #endregion


    #region Private Variables

    private string hexColor = "#222222";
    private float refreshSensorValueTime;

    #endregion

    #region Main Methods

    private void Start()
    {
        PluginWrapper.SetNavigationBarColor(hexColor);
        PluginWrapper.StatusBarState(hexColor);

        //Tell the arudino to disconnect bluetooth
        PluginWrapper.SendBluetoothMessage("1");

        //Tell the arudino to log off
        PluginWrapper.SendBluetoothMessage("3");

        CameraPosition(0);
        OnClick_SelectPannel(1);

        passwordInput.text = "";
    }

    private void Update()
    {
        GetReceivedMessages();

        if (uiPannel.transform.localPosition.x != finalPosition)
        {
            uiPannel.transform.localPosition = Vector3.Lerp(uiPannel.transform.localPosition, new Vector3(finalPosition, uiPannel.transform.localPosition.y, uiPannel.transform.localPosition.z), cameraLerpTime * Time.deltaTime);
        }
    }

    #endregion

    #region OnClick Methods

    public void OnClick_SearchDevices()
    {
        foreach (Transform child in deviceItemHolder)
        {
            Destroy(child.gameObject);
        }

        //Search for the items, instantiate the list, and setup the buttons
        //string[] deviceListInfo = "BluetoothDevice1&Address1$BluetoothDevice2&Addresse2$BluetoothDevice3&Addresse3$BluetoothDevice4&Addresse4$".Split('$');
        string[] deviceListInfo = PluginWrapper.GetPairedDevices().Split('$');
        for (int i = 0; i < deviceListInfo.Length - 1; i++)
        {
            GameObject go = Instantiate(deviceItemPrefab);
            string name = deviceListInfo[i].Split('&')[0];
            string address = deviceListInfo[i].Split('&')[1];
            go.GetComponent<DeviceItem>().SetItem(this, name, address);
            go.transform.SetParent(deviceItemHolder);
            go.transform.localScale = new Vector3(1, 1, 1);
        }
    }

    public void OnClick_CheckLogin()
    {
        if (passwordInput.text == PlayerPrefs.GetString("SavedPassword"))
        {
            isConnected = true;
            loginStatusText.color = new Color32(0, 255, 0, 150);
            StartCoroutine(DisplayStatusMessage(loginStatusText, "Connected"));

            passwordInput.text = "";
            CameraPosition(2);
            OnClick_SelectPannel(1);

            PluginWrapper.SendBluetoothMessage("2");
        }
        else
        {
            loginStatusText.color = new Color32(255, 0, 0, 150);
            StartCoroutine(DisplayStatusMessage(loginStatusText, "Password is incorrect"));
        }
    }

    public void OnClick_ChangePassword()
    {
        if(currentPasswordInput.text == PlayerPrefs.GetString("SavedPassword", "admin"))
        {
            if(newPasswordInput.text.Length > 4)
            {
                password = newPasswordInput.text;
                PlayerPrefs.SetString("SavedPassword", password);

                changePasswordStatusText.color = new Color32(0, 255, 0, 150);
                StartCoroutine(DisplayStatusMessage(changePasswordStatusText, "Password updated successfully"));
                currentPasswordInput.text = "";
                newPasswordInput.text = "";
            }
            else
            {
                changePasswordStatusText.color = new Color32(255, 0, 0, 150);
                StartCoroutine(DisplayStatusMessage(changePasswordStatusText, "Password need to be more than 4 characters"));
            }

        }
        else
        {
            changePasswordStatusText.color = new Color32(255, 0, 0, 150);
            StartCoroutine(DisplayStatusMessage(changePasswordStatusText, "Current password is incorrect"));
        }
    }

    public void OnClick_SelectPannel(int id)
    {
        for (int i = 0; i < menuBtn.Length; i++)
        {
            menuBtn[i].GetComponent<MenuButton>().ChangeButtonState(i == id);
            pannelObjects[i].SetActive(i == id);
        }
    }

    public void OnClick_LogOff()
    {
        PluginWrapper.SendBluetoothMessage("3");
        isConnected = false;
        CameraPosition(1);
        loginStatusText.text = "";
    }

    public void OnClick_TurnOffBluetooth()
    {
        //This method already inform the arduino that the phone is note connected
        PluginWrapper.TurnOffBluetooth();
        CameraPosition(0);
    }

    #endregion

    #region Public Methods

    public void CameraPosition(int positionId)
    {
        switch (positionId)
        {
            case 0:
                finalPosition = 1080;
                break;
            case 1:
                finalPosition = 0;
                break;
            case 2:
                finalPosition = -1080;
                break;
        }
    }

    #endregion

    #region Private Methods

    private IEnumerator DisplayStatusMessage(TextMeshProUGUI statusText, string message)
    {
        statusText.text = message;
        yield return new WaitForSeconds(2f);
        statusText.text = "";
    }

    private void GetReceivedMessages()
    {
        if (isConnected)
        {
            if (pannelObjects[1].activeSelf)
            {
                //Tell the arduino to send the pressure captor value
                PluginWrapper.SendBluetoothMessage("4");

                // "$" separator is set on the arduino code
                string[] receivedMessage = PluginWrapper.SetCaptorValue().Split('$');

                if (receivedMessage[1] != "")
                {
                    //If the value is above 1023 it's a unwanted spike value
                    if (float.Parse(receivedMessage[1]) > 100) return;

                    if (receivedMessage[0] == "2")
                    {
                        float finalRatio = Mathf.Lerp(float.Parse(batteryPercentageTxt.text) / 100, float.Parse(receivedMessage[1]) / 100, 0.5f);
                        batteryPercentageCircle.fillAmount = finalRatio;
                        batteryPercentageTxt.text = ((int)(finalRatio * 100)).ToString();
                        batteryTxt.text = ((int)(finalRatio * 10)).ToString() + " mAh";
                    }
                }
            }
        }
    }

    #endregion
}
