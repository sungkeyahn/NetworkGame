using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GroundCheck : MonoBehaviour
{
    [SerializeField] private Vector3 boxSize;
    [SerializeField] private float maxDistance;
    public void SetBoxSize(Vector3 size) { boxSize = size; }
    public void SetMaxDistance(float dis) { maxDistance = dis; }
    public bool IsGrounded()
    {
        return Physics.BoxCast(transform.position, boxSize, -transform.up, transform.rotation, maxDistance, (int)Define.Layer.Ground);
    }
    [Header("Debug")]
    [SerializeField] private bool drawGizmo;

    private void OnDrawGizmos()
    {
        if (!drawGizmo) return;

        Gizmos.color = Color.cyan;
        Gizmos.DrawCube(transform.position - transform.up * maxDistance, boxSize);
    }
}
