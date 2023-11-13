using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CCMovement : MonoBehaviour
{
    [Header("Transform References")]
    [SerializeField] private Transform movementOrientation;
    [SerializeField] private Transform characterMesh;

    [Header("Movement")]
    [SerializeField] private float speed;
    [SerializeField] private float gravitationalAcceleration;
    [Space(10.0f)]
    [SerializeField, Range(0.0f, 1.0f)] private float lookForwardThreshold;
    [SerializeField] private float lookForwardSpeed;

     CharacterController m_characterController;
     GroundCheck m_groundChecker;
     Vector3 velocity;
     Vector3 lastFixedPosition;
     Quaternion lastFixedRotation;
     Vector3 nextFixedPosition;
     Quaternion nextFixedRotation;

    void Start()
    {
        m_characterController = GetComponent<CharacterController>();
        m_groundChecker = GetComponent<GroundCheck>();
        m_groundChecker.SetBoxSize(new Vector3(0.7f, 0.1f, 0.7f));
        m_groundChecker.SetMaxDistance(10);

        velocity = new Vector3(0, 0, 0);
        lastFixedPosition = transform.position;
        lastFixedRotation = transform.rotation;
        nextFixedPosition = transform.position;
        nextFixedRotation = transform.rotation;

        //Managers.Input.MouseAction -= OnMouseEvent_IdleRun;
        //Managers.Input.MouseAction += OnMouseEvent_IdleRun;
    }


    Vector3 GetXZVelocity(float horizontalInput, float verticalInput)
    {
        Vector3 moveVelocity = movementOrientation.forward * verticalInput + movementOrientation.right * horizontalInput;
        Vector3 moveDirection = moveVelocity.normalized;
        float moveSpeed = Mathf.Min(moveVelocity.magnitude, 1.0f) * speed;

        return moveDirection * moveSpeed;
    }
    float GetYVelocity()
    {
        if (!m_groundChecker.IsGrounded())
           return velocity.y - gravitationalAcceleration * Time.fixedDeltaTime;
        else
            return Mathf.Max(0.0f, velocity.y);
    }

    /*
    void OnMouseEvent_IdleRun(Define.MouseEvent evt)
    {
        RaycastHit hit;
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        bool raycastHit = Physics.Raycast(ray, out hit, 100.0f, _mask);
        Debug.DrawRay(Camera.main.transform.position, ray.direction * 100.0f, Color.red, 1.0f);

        switch (evt)
        {
            case Define.MouseEvent.PointerDown:
                {
                    if (raycastHit)
                    {
                        _destPos = hit.point;
                        if (hit.collider.gameObject.layer == (int)Define.Layer.Monster)
                            _lockTarget = hit.collider.gameObject;
                        else
                            _lockTarget = null;
                    }
                }
                break;
            case Define.MouseEvent.Press:
                {
                    if (_lockTarget == null && raycastHit)
                        _destPos = hit.point;
                }
                break;
            case Define.MouseEvent.PointerUp:
                break;
        }
    }
    */

    public void MoveUpdate()
    {
        if (!gameObject) return;
        float interpolationAlpha = (Time.time - Time.fixedTime) / Time.fixedDeltaTime;
        m_characterController.Move(Vector3.Lerp(lastFixedPosition, nextFixedPosition, interpolationAlpha) - transform.position);
        characterMesh.rotation = Quaternion.Slerp(lastFixedRotation, nextFixedRotation, interpolationAlpha);
    }
    public void MoveFixedUpdate(Vector3 DestPos)
    {
        if (!gameObject) return;

        lastFixedPosition = nextFixedPosition;
        lastFixedRotation = nextFixedRotation;

        //여기부터
        Vector3 dir = DestPos - transform.position;
        //dir.y = 0;
        if (dir.magnitude < 0.1f)
        {
            Debug.Log("이동 정지");
        }
        else
        {
            Debug.DrawRay(transform.position + Vector3.up * 0.5f, dir.normalized, Color.green);
            if (Physics.Raycast(transform.position + Vector3.up * 0.5f, dir, 1.0f, LayerMask.GetMask("Block")))
            {
                if (Input.GetMouseButton(0) == false)
                    Debug.Log("이동 정지");
                return;
            }
        }
        //여기까지  수정코드

        Vector3 planeVelocity = GetXZVelocity(dir.x, dir.z);
        float yVelocity = GetYVelocity();
        velocity = new Vector3(planeVelocity.x, yVelocity, planeVelocity.z);

        if (planeVelocity.magnitude / speed >= lookForwardThreshold)
            nextFixedRotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(planeVelocity), lookForwardSpeed * Time.fixedDeltaTime);
        nextFixedPosition += velocity * Time.fixedDeltaTime;
    }
}
