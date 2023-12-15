using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

public class PhotonManagers : MonoBehaviourPunCallbacks
{
    readonly string version = "1.0f"; //포톤 버전
    string userName = "None"; //사용자 식별 변수
    private void Awake()
    {
        //같은 룸에 있는 유저들에게 자동으로 씬을 로딩할지 여부
        PhotonNetwork.AutomaticallySyncScene = true;
        //같은 버전의 유저끼리의 접속 허용을 위한 게임버전 세팅
        PhotonNetwork.GameVersion = version;
        //유저에게 아이디 할당 
        PhotonNetwork.NickName = userName;
        //포톤서버의 통신 횟수 설정,기본은 초당30
        Debug.Log(PhotonNetwork.SendRate);
        //서버 접속 메서드(중요)
        PhotonNetwork.ConnectUsingSettings();
    }
    public override void OnConnectedToMaster() //포톤 서버 입장시 호출되는 콜백함수
    {
        base.OnConnectedToMaster();
        Debug.Log("ConnectedToMaster");
        Debug.Log($"InLobby : { PhotonNetwork.InLobby}");
        PhotonNetwork.JoinLobby(); //로비 입장 메서드
    }
    public override void OnJoinedLobby() //로비에 입장시 호출되는 콜백 함수
    {
        base.OnJoinedLobby();
        Debug.Log("JoinedLobby");
        Debug.Log($"InLobby : { PhotonNetwork.InLobby}");
        PhotonNetwork.JoinRandomRoom(); //랜덤 룸 매칭 기능 제공 메서드 
    }
    public override void OnJoinRandomFailed(short returnCode, string message) //룸입장에 실패했을때 호출되는 콜백함수
    {
        base.OnJoinRandomFailed(returnCode, message);
        Debug.Log($"JoinRandomFailed{returnCode}:{message}");

        //룸 생성을 위한 룸 속성 정의
        RoomOptions op = new RoomOptions();
        op.MaxPlayers = 20;
        op.IsOpen = true;
        op.IsVisible = true;//로비에서 해당 룸 조회 가능 여부

        PhotonNetwork.CreateRoom("Room",op);//룸생성 메서드
    }
    public override void OnCreatedRoom()//룸 생성 이후 호출되는 콜백함수
    {
        base.OnCreatedRoom();
        Debug.Log("CreatedRoom");
        Debug.Log($"RoomNam={PhotonNetwork.CurrentRoom.Name}");
    }
    public override void OnJoinedRoom() //룸 입장 이후 호출되는 콜백함수 
    {
        base.OnJoinedRoom();
        Debug.Log($"InRoom={PhotonNetwork.InRoom}");
        Debug.Log($"RoomNam={PhotonNetwork.CurrentRoom.PlayerCount}");

        //룸에 입장한 모든 유저의 정보 확인 
        foreach (var item in PhotonNetwork.CurrentRoom.Players)
        {
            Debug.Log($"{item.Value.NickName},{item.Value.ActorNumber}");
        }
    }

}
