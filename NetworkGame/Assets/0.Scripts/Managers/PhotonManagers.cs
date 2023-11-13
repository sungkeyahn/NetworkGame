using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

public class PhotonManagers : MonoBehaviourPunCallbacks
{
    readonly string version = "1.0f"; //���� ����
    string userName = "None"; //����� �ĺ� ����
    private void Awake()
    {
        //���� �뿡 �ִ� �����鿡�� �ڵ����� ���� �ε����� ����
        PhotonNetwork.AutomaticallySyncScene = true;
        //���� ������ ���������� ���� ����� ���� ���ӹ��� ����
        PhotonNetwork.GameVersion = version;
        //�������� ���̵� �Ҵ� 
        PhotonNetwork.NickName = userName;
        //���漭���� ��� Ƚ�� ����,�⺻�� �ʴ�30
        Debug.Log(PhotonNetwork.SendRate);
        //���� ���� �޼���(�߿�)
        PhotonNetwork.ConnectUsingSettings();
    }
    public override void OnConnectedToMaster() //���� ���� ����� ȣ��Ǵ� �ݹ��Լ�
    {
        base.OnConnectedToMaster();
        Debug.Log("ConnectedToMaster");
        Debug.Log($"InLobby : { PhotonNetwork.InLobby}");
        PhotonNetwork.JoinLobby(); //�κ� ���� �޼���
    }
    public override void OnJoinedLobby() //�κ� ����� ȣ��Ǵ� �ݹ� �Լ�
    {
        base.OnJoinedLobby();
        Debug.Log("JoinedLobby");
        Debug.Log($"InLobby : { PhotonNetwork.InLobby}");
        PhotonNetwork.JoinRandomRoom(); //���� �� ��Ī ��� ���� �޼��� 
    }
    public override void OnJoinRandomFailed(short returnCode, string message) //�����忡 ���������� ȣ��Ǵ� �ݹ��Լ�
    {
        base.OnJoinRandomFailed(returnCode, message);
        Debug.Log($"JoinRandomFailed{returnCode}:{message}");

        //�� ������ ���� �� �Ӽ� ����
        RoomOptions op = new RoomOptions();
        op.MaxPlayers = 20;
        op.IsOpen = true;
        op.IsVisible = true;//�κ񿡼� �ش� �� ��ȸ ���� ����

        PhotonNetwork.CreateRoom("Room",op);//����� �޼���
    }
    public override void OnCreatedRoom()//�� ���� ���� ȣ��Ǵ� �ݹ��Լ�
    {
        base.OnCreatedRoom();
        Debug.Log("CreatedRoom");
        Debug.Log($"RoomNam={PhotonNetwork.CurrentRoom.Name}");
    }
    public override void OnJoinedRoom() //�� ���� ���� ȣ��Ǵ� �ݹ��Լ� 
    {
        base.OnJoinedRoom();
        Debug.Log($"InRoom={PhotonNetwork.InRoom}");
        Debug.Log($"RoomNam={PhotonNetwork.CurrentRoom.PlayerCount}");

        //�뿡 ������ ��� ������ ���� Ȯ�� 
        foreach (var item in PhotonNetwork.CurrentRoom.Players)
        {
            Debug.Log($"{item.Value.NickName},{item.Value.ActorNumber}");
        }
    }

}
