using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
public class UI_Dead : MonoBehaviour
{
    public void RespanwPlayerCharacter()
    {
        Manager_Player.Instance.CreatePalyer();
    }
    public void QuitGame()
    {
        Application.Quit();
    }
}
