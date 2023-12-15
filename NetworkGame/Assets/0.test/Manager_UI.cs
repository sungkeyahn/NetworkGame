using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Manager_UI : MonoBehaviour
{
    public static Manager_UI Instance;

    [SerializeField]
    UI_Base[] UIElements;

    void Awake()
    {
        Instance = this;
    }

    public void OpenUI(string uiName)
    {
        for (int i = 0; i < UIElements.Length; i++)
        {
            if (UIElements[i].UI_Name == uiName)
                UIElements[i].Open();
            else if (UIElements[i].isOpen)
                CloseUI(UIElements[i]);
        }
    }
    public void OpenUI(UI_Base uiBase)
    {
        for (int i = 0; i < UIElements.Length; i++)
        {
            if (UIElements[i].isOpen)
                CloseUI(UIElements[i]);
        }
        uiBase.Open();
    }
    public void CloseUI(UI_Base uiBase)
    {
        uiBase.Close();
    }

}
