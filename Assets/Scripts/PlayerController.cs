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

    [Header("Wall Check Settings")]
    public LayerMask wallLayer;
    public float wallCheckDistance = 1f;
    public float wallCheckHeight = 0.2f;

    [Header("Surface Change Settings")]
    public float frontGroundCheckDistance = 2.1f;
    public float surfaceMoveOffset = 0.8f;
    private Vector3 currentUp;

    [Header("Cube Center")]
    public Transform cubeCenter;

    void Start()
    {
        currentUp = transform.up;
    }

    void Update()
    {

        if (isMoving) return;

        CheckGround();

        if (Input.GetKeyDown(KeyCode.W))
        {
            StartCoroutine(MoveForward());
        }
        else if (Input.GetKeyDown(KeyCode.A) && !hasRotated)
        {
            StartCoroutine(Rotate(-90f));
        }
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
            Vector3 newUp = GetInsideUp(hit.point);

            currentUp = newUp;

            Quaternion targetRotation = Quaternion.FromToRotation(transform.up, currentUp) * transform.rotation;
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, 10f * Time.deltaTime);

            transform.position = hit.point + currentUp * playerHeightOffset;
        }
    }

    IEnumerator MoveForward()
    {
        isMoving = true;

        if (IsWallInFront())
        {
            hasRotated = false;
            isMoving = false;
            yield break;
        }

        RaycastHit frontGroundHit;
        if (TryGetFrontGround(out frontGroundHit))
        {
            yield return MoveToNewSurface(frontGroundHit);
            hasRotated = false;
            isMoving = false;
            yield break;
        }

        Vector3 startPos = transform.position;
        Vector3 targetPos = startPos + transform.forward * moveDistance;
        float elapsedTime = 0;

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
    IEnumerator MoveToNewSurface(RaycastHit hit)
    {
        Vector3 startPos = transform.position;
        Quaternion startRot = transform.rotation;

        Vector3 newUp = GetInsideUp(hit.point);

        Vector3 targetPos = hit.point + newUp * surfaceMoveOffset;

        Quaternion surfaceTurn = Quaternion.FromToRotation(currentUp, newUp);
        Quaternion targetRot = surfaceTurn * startRot;

        float elapsedTime = 0;

        while (elapsedTime < moveDuration)
        {
            float t = elapsedTime / moveDuration;

            transform.position = Vector3.Lerp(startPos, targetPos, t);
            transform.rotation = Quaternion.Slerp(startRot, targetRot, t);

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        transform.position = targetPos;
        transform.rotation = targetRot;
        currentUp = newUp;
    }

    Vector3 GetInsideUp(Vector3 surfacePoint)
    {
        Vector3 centerPos = Vector3.zero;

        if (cubeCenter != null)
        {
            centerPos = cubeCenter.position;
        }

        Vector3 dir = centerPos - surfacePoint;

        float absX = Mathf.Abs(dir.x);
        float absY = Mathf.Abs(dir.y);
        float absZ = Mathf.Abs(dir.z);

        if (absX > absY && absX > absZ)
        {
            return new Vector3(Mathf.Sign(dir.x), 0, 0);
        }
        else if (absY > absX && absY > absZ)
        {
            return new Vector3(0, Mathf.Sign(dir.y), 0);
        }
        else
        {
            return new Vector3(0, 0, Mathf.Sign(dir.z));
        }
    }
    bool IsWallInFront()
    {
        RaycastHit hit;

        Vector3 origin = transform.position + currentUp * wallCheckHeight;
        Vector3 direction = transform.forward;

        if (Physics.Raycast(origin, direction, out hit, wallCheckDistance, wallLayer))
        {
            return true;
        }

        return false;
    }
    bool TryGetFrontGround(out RaycastHit hit)
    {
        Vector3 origin = transform.position;
        Vector3 direction = transform.forward;

        return Physics.Raycast(origin, direction, out hit, frontGroundCheckDistance, groundLayer);
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