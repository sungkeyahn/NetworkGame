using Cinemachine; 
using Photon.Pun; 
using UnityEngine;


public class CameraController : MonoBehaviour
{
    
    void Start()
    {
        PhotonView pv = GetComponent<PhotonView>();
        if (pv.IsMine)
        {
            CinemachineVirtualCamera cam = FindObjectOfType<CinemachineVirtualCamera>();
            cam.Follow = transform;
            cam.LookAt = transform;  
        }
    }

}
