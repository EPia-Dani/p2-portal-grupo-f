using System;
using Core.EventBus;
using Player;
using UnityEngine;

public class GravityGun : MonoBehaviour
{
    public float pickRange = 5f;
    public float holdSmooth = 10f;
    public float hoverAmplitude = 0.15f;
    public float hoverSpeed = 2f;
    public float holdDistance;

    [Header("Ground Collision")]
    public LayerMask groundMask = ~0;
    public float groundClearance = 0.05f;

    [Header("Smoothing")]
    public float positionSmoothTime = 0.05f;

    private Camera playerCamera;
    private Rigidbody heldRb;
    private float originalDrag;
    private float hoverOffset;

    private Vector3 moveVelocity = Vector3.zero;

    void Start()
    {
        playerCamera = Camera.main;
        EventBusVoid<PlayerEventsEnum>.Subscribe(PlayerEventsEnum.Interact, HandleInteract);
    }

    void OnDisable()
    {
        EventBusVoid<PlayerEventsEnum>.Unsubscribe(PlayerEventsEnum.Interact, HandleInteract);
    }

    void HandleInteract()
    {
        if (heldRb == null)
            TryPick();
        else
            Drop();
    }

    void TryPick()
    {
        if (playerCamera == null) return;

        Ray ray = playerCamera.ScreenPointToRay(new Vector2(Screen.width / 2f, Screen.height / 2f));
        if (Physics.Raycast(ray, out RaycastHit hit, pickRange))
        {
            var pickable = hit.collider.GetComponent<PickableCube>();
            if (pickable != null)
            {
                var rb = pickable.GetComponent<Rigidbody>();
                if (rb != null)
                {
                    heldRb = rb;
                    originalDrag = heldRb.linearDamping;
                    holdDistance = Vector3.Distance(playerCamera.transform.position, heldRb.position);

                    heldRb.useGravity = false;
                    heldRb.linearDamping = 10f;
                    heldRb.angularDamping = 10f;

                    moveVelocity = Vector3.zero;
                }
            }
        }
    }

    void Drop()
    {
        if (heldRb == null) return;

        heldRb.useGravity = true;
        heldRb.linearDamping = originalDrag;
        heldRb.angularDamping = 0.05f;

        heldRb = null;
        moveVelocity = Vector3.zero;
    }

    void FixedUpdate()
    {
        if (heldRb == null || playerCamera == null) return;

        Vector3 targetPos = playerCamera.transform.position + playerCamera.transform.forward * holdDistance;
        hoverOffset = Mathf.Sin(Time.time * hoverSpeed) * hoverAmplitude;
        targetPos += playerCamera.transform.up * hoverOffset;

        Collider heldCol = heldRb.GetComponent<Collider>();
        if (heldCol != null)
        {
            float bottomOffset = heldCol.bounds.extents.y;

            float checkDistance = holdDistance + bottomOffset + 1f;
            if (Physics.Raycast(targetPos, Vector3.down, out RaycastHit groundHit, checkDistance, groundMask))
            {
                float groundY = groundHit.point.y;
                float minCenterY = groundY + bottomOffset + groundClearance;

                if (targetPos.y - bottomOffset < groundY + groundClearance)
                {
                    targetPos.y = minCenterY;
                }
            }
        }

        Vector3 direction = targetPos - heldRb.position;
        float distance = direction.magnitude;

        float forceMagnitude = distance * holdSmooth;
        heldRb.AddForce(direction.normalized * forceMagnitude, ForceMode.VelocityChange);
        heldRb.linearVelocity *= 0.9f;

        Quaternion targetRot = Quaternion.LookRotation(playerCamera.transform.forward, playerCamera.transform.up);
        heldRb.MoveRotation(Quaternion.Slerp(heldRb.rotation, targetRot, Time.fixedDeltaTime * holdSmooth));
    }
}