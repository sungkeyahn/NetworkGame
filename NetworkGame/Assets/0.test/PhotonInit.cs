using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using TMPro;
using Photon.Realtime;

public class PhotonInit : MonoBehaviourPunCallbacks
{
    public static PhotonInit Instance;

    [SerializeField] TMP_InputField roomNameInputField;
    [SerializeField] TMP_Text errorText;
    [SerializeField] TMP_Text roomNameText;
    [SerializeField] Transform roomListTR;
    [SerializeField] GameObject roomListItemGO;
    [SerializeField] Transform playerListTR;
    [SerializeField] GameObject playerListItemGO;
    [SerializeField] GameObject joinGameBtn;

    void Awake()
    {
        Instance = this;
    }
    void Start()
    {
        PhotonNetwork.IsMessageQueueRunning = false;
        PhotonNetwork.ConnectUsingSettings();
    }
    public override void OnConnectedToMaster()
    {
        PhotonNetwork.JoinLobby();
        PhotonNetwork.AutomaticallySyncScene = true;
    }
    public override void OnMasterClientSwitched(Player newMasterClient)
    {
        joinGameBtn.SetActive(PhotonNetwork.IsMasterClient);
    }
    public override void OnJoinedLobby()
    {
        Manager_UI.Instance.OpenUI("Title");
        PhotonNetwork.NickName = "Player" + Random.Range(0, 1000).ToString("0000");
    }
    public override void OnJoinedRoom()
    {
        roomNameText.text = PhotonNetwork.CurrentRoom.Name;
        Manager_UI.Instance.OpenUI("Room");

        Player[] p = PhotonNetwork.PlayerList;

        foreach (Transform item in playerListTR)
        {
            Destroy(item.gameObject);
        }

        for (int i = 0; i < p.Length; i++)
        {
            Instantiate(playerListItemGO, playerListTR).GetComponent<UI_PlayerListItem>().SetUp(p[i]);
        }

        joinGameBtn.SetActive(PhotonNetwork.IsMasterClient);

        Manager_Room.Instance.isConnect = true;
    }
    public override void OnCreateRoomFailed(short returnCode, string message)
    {
        Manager_UI.Instance.OpenUI("Error");
    }
    public override void OnLeftRoom()
    {
        Manager_UI.Instance.OpenUI("Title");
    }
    public override void OnRoomListUpdate(List<RoomInfo> roomList)
    {
        foreach (Transform tr in roomListTR)
        {   //그냥 다 삭제하지 말고 방이 존재하는지 유무를 알아야함
            Destroy(tr.gameObject);
        }
        for (int i = 0; i < roomList.Count; i++)
        {
            if (roomList[i].RemovedFromList) continue;
            Instantiate(roomListItemGO, roomListTR).GetComponent<UI_RoomListItem>().SetUp(roomList[i]);
        }
    }
    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        Instantiate(playerListItemGO, playerListTR).GetComponent<UI_PlayerListItem>().SetUp(newPlayer);
    }
    public void Createroom()
    {
        if (string.IsNullOrEmpty(roomNameInputField.text)) return;
        PhotonNetwork.CreateRoom(roomNameInputField.text);
        Manager_UI.Instance.OpenUI("Loading");
    }
    public void LeaveRoom()
    {
        PhotonNetwork.LeaveRoom();
        //Manager_UI.Instance.OpenUI("Loading");
    }
    public void JoinRoom(RoomInfo info)
    {
        PhotonNetwork.JoinRoom(info.Name);
        //Manager_UI.Instance.OpenUI("Loading");
    }
    public void JoinGame()
    {
        PhotonNetwork.LoadLevel(1);
    }
    public void QuitGame()
    {
        Application.Quit();
    }

}
