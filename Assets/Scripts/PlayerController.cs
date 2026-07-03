using System.Collections;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveDistance = 1f; // Distance to move per tile
    public float moveDuration = 0.25f; // Duration of movement
    public float rotationDuration = 0.2f; // Duration of rotation

    private bool isMoving = false;
    private bool hasRotated = false;

    [Header("Cube Surface Settings")]
    public LayerMask groundLayer;
    public float groundCheckDistance = 1.5f;
    public float playerHeightOffset = 0.8f;

    private Vector3 currentUp;

    void Start()
    {
        currentUp = transform.up;
    }

    void Update()
    {

        CheckGround();

        // Disable input during movement
        if (isMoving) return;

        // Move forward (W key)
        if (Input.GetKeyDown(KeyCode.W))
        {
            StartCoroutine(MoveForward());
        }
        // Rotate 90 degrees left (A key)
        else if (Input.GetKeyDown(KeyCode.A) && !hasRotated)
        {
            StartCoroutine(Rotate(-90f));
        }
        // Rotate 90 degrees right (D key)
        else if (Input.GetKeyDown(KeyCode.D) && !hasRotated)
        {
            StartCoroutine(Rotate(90f));
        }


    }

    void CheckGround()
    {
        RaycastHit hit;

        Vector3 down = -currentUp;

        if (Physics.Raycast(transform.position, down, out hit, groundCheckDistance, groundLayer))
        {
           
            Vector3 newUp = hit.transform.up;

            currentUp = newUp;

            Quaternion targetRotation = Quaternion.FromToRotation(transform.up, currentUp) * transform.rotation;
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, 10f * Time.deltaTime);

            transform.position = hit.point + currentUp * playerHeightOffset;
        }
    }

    IEnumerator MoveForward()
    {
        isMoving = true;
        Vector3 startPos = transform.position;
        // Set target position forward by moveDistance
        Vector3 targetPos = startPos + transform.forward * moveDistance;
        float elapsedTime = 0;

        // Smooth movement (Lerp)
        while (elapsedTime < moveDuration)
        {
            transform.position = Vector3.Lerp(startPos, targetPos, elapsedTime / moveDuration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        transform.position = targetPos;
        hasRotated = false;
        isMoving = false;
    }

    IEnumerator Rotate(float angle)
    {
        isMoving = true;
        Quaternion startRot = transform.rotation;
        // Set target rotation by adding the specified angle
        Quaternion turnRot = Quaternion.AngleAxis(angle, currentUp);
        Quaternion targetRot = turnRot * startRot;
        float elapsedTime = 0;

        // Smooth rotation (Lerp)
        while (elapsedTime < rotationDuration)
        {
            transform.rotation = Quaternion.Slerp(startRot, targetRot, elapsedTime / rotationDuration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        transform.rotation = targetRot;
        hasRotated = true;
        isMoving = false;
    }
}