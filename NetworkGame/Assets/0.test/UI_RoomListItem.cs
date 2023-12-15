using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Photon.Realtime;

public class UI_RoomListItem : MonoBehaviour
{
    [SerializeField] TMP_Text text;
    public RoomInfo info=null;
    public void SetUp(RoomInfo i)
    {
        info = i;
        text.text = i.Name;
    }
    public void OnClick()
    {
        if (info!=null)
            PhotonInit.Instance.JoinRoom(info);
    }
}
