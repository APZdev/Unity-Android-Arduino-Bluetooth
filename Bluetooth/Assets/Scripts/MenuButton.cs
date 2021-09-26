using UnityEngine;
using UnityEngine.UI;

public class MenuButton : MonoBehaviour
{
    private float[] predefiedHeight = { 220, 270 };
    private bool isSelected;
    private Button buttonBackgroundBtn;
    private RectTransform rect;


    void Start()
    {
        buttonBackgroundBtn = GetComponent<Button>();
        rect = GetComponent<RectTransform>();

        isSelected = false;
    }

    void Update()
    {
        if (isSelected)
        {   
            rect.sizeDelta = Vector2.Lerp(rect.sizeDelta, new Vector2(rect.sizeDelta.x, predefiedHeight[1]), 0.2f);
        }
        else
        {
            rect.sizeDelta = Vector2.Lerp(rect.sizeDelta, new Vector2(rect.sizeDelta.x, predefiedHeight[0]), 0.2f);
        }
    }

    public void ChangeButtonState(bool state)
    {
        isSelected = state;

        if(isSelected)
            buttonBackgroundBtn.Select();
    }
}
