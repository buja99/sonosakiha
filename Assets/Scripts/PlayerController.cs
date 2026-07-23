using System.Collections;
using UnityEngine;
using TMPro;
using System;

public class PlayerController : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveDistance = 1f; // Distance to move per tile
    public float moveDuration = 0.25f; // Duration of movement
    public float rotationDuration = 0.2f; // Duration of rotation

    private bool isMoving = false;
    //private bool hasRotated = false;

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

    [Header("Cannot Move Debug")]
    public bool debugCannotMove = true;
    public TextMeshProUGUI cannotMoveDebugText;

    private int cannotMoveJudgeCount = 0;
    private int cannotMoveTextShowCount = 0;

    [Header("Cube Center")]
    public Transform cubeCenter;

    [Header("UI")]
    public TextMeshProUGUI cannotMoveText;
    public float messageTime = 1.0f;

    private Coroutine messageCoroutine;

    private int selectedDirection = 0; // -1 = left, 0 = forward, 1 = right

    private Vector3 baseForward;
    private Vector3 selectedMoveDirection;
    private Coroutine rotateCoroutine;
    private bool canControl = true;

    void Awake()
    {
        if (cannotMoveText != null)
        {
            cannotMoveText.gameObject.SetActive(false);
        }
    }

    void Start()
    {
        currentUp = transform.up.normalized;

        baseForward = Vector3.ProjectOnPlane(transform.forward, currentUp).normalized;

        if (baseForward.sqrMagnitude < 0.001f)
        {
            baseForward = transform.forward.normalized;
        }

        selectedDirection = 0;
        selectedMoveDirection = baseForward;

        transform.rotation = Quaternion.LookRotation(baseForward, currentUp);

        if (cannotMoveText != null)
        {
            cannotMoveText.gameObject.SetActive(false);
        }
    }

    void Update()
    {
        if (!canControl) return;
        if (Time.timeScale == 0f) return;

        if (isMoving) return;

        CheckGround();

        if (Input.GetKeyDown(KeyCode.A))
        {
            int nextDirection = selectedDirection - 1;

            if (nextDirection < -1)
            {
                nextDirection = -1;
            }

            if (nextDirection == selectedDirection)
            {
                return;
            }

            Vector3 nextMoveDirection = GetDirectionFromSelection(nextDirection);

            if (IsWallInDirection(nextMoveDirection))
            {
                ShowCannotMoveMessage();
                return;
            }

            selectedDirection = nextDirection;
            selectedMoveDirection = nextMoveDirection;

            RotateVisualToSelectedDirection();
        }

        if (Input.GetKeyDown(KeyCode.D))
        {
            int nextDirection = selectedDirection + 1;

            if (nextDirection > 1)
            {
                nextDirection = 1;
            }

            if (nextDirection == selectedDirection)
            {
                return;
            }

            Vector3 nextMoveDirection = GetDirectionFromSelection(nextDirection);

            if (IsWallInDirection(nextMoveDirection))
            {
                ShowCannotMoveMessage();
                return;
            }

            selectedDirection = nextDirection;
            selectedMoveDirection = nextMoveDirection;

            RotateVisualToSelectedDirection();
        }

        if (Input.GetKeyDown(KeyCode.W) )
        {
            StartCoroutine(MoveToSelectedDirection());
        }
    }
    public void SetControlEnabled(bool enabled)
    {
        canControl = enabled;

        if (!enabled)
        {
            HideCannotMoveMessage();
        }
    }
    public void InitializeStartDirection(Vector3 startUp, Vector3 startForward)
    {
        currentUp = startUp.normalized;

        Vector3 forward = Vector3.ProjectOnPlane(startForward, currentUp).normalized;
        Vector3 right = Quaternion.AngleAxis(90f, currentUp) * forward;
        Vector3 left = Quaternion.AngleAxis(-90f, currentUp) * forward;

        if (!IsWallInDirection(forward))
        {
            baseForward = forward;
        }
        else if (!IsWallInDirection(right))
        {
            baseForward = right;
        }
        else if (!IsWallInDirection(left))
        {
            baseForward = left;
        }
        else
        {
            baseForward = forward;
        }

        selectedDirection = 0;
        selectedMoveDirection = baseForward;

        transform.rotation = Quaternion.LookRotation(baseForward, currentUp);

        if (cannotMoveText != null)
        {
            cannotMoveText.gameObject.SetActive(false);
        }
    }
    void CheckGround()
    {
        RaycastHit hit;

        Vector3 down = -currentUp;

        if (Physics.Raycast(transform.position, down, out hit, groundCheckDistance, groundLayer))
        {
            Vector3 newUp = GetInsideUp(hit.point);

            if (Vector3.Angle(currentUp, newUp) > 1f)
            {
                Vector3 fixedForward = Vector3.ProjectOnPlane(transform.forward, newUp).normalized;

                if (fixedForward.sqrMagnitude < 0.001f)
                {
                    fixedForward = Vector3.ProjectOnPlane(baseForward, newUp).normalized;
                }

                currentUp = newUp;
                baseForward = fixedForward;

                selectedDirection = 0;
                selectedMoveDirection = baseForward;

                transform.rotation = Quaternion.LookRotation(baseForward, currentUp);
            }
            else
            {
                currentUp = newUp;
            }

            transform.position = hit.point + currentUp * playerHeightOffset;
        }
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
    bool TryGetGroundInDirection(Vector3 direction, out RaycastHit hit)
    {
        Vector3 origin = transform.position;

        return Physics.Raycast(
            origin,
            direction.normalized,
            out hit,
            frontGroundCheckDistance,
            groundLayer
        );
    }


    bool IsWallInDirection(Vector3 direction)
    {
        RaycastHit hit;

        Vector3 origin = transform.position + currentUp * wallCheckHeight;

        Debug.DrawRay(origin, direction.normalized * wallCheckDistance, Color.red, 1.0f);

        if (Physics.Raycast(origin, direction.normalized, out hit, wallCheckDistance, wallLayer))
        {
            cannotMoveJudgeCount++;

            if (debugCannotMove)
            {
                Debug.Log(
                    "[Determination of immobility made] " +
                    "Count: " + cannotMoveJudgeCount +
                    " / Hit: " + hit.collider.name +
                    " / Layer: " + LayerMask.LayerToName(hit.collider.gameObject.layer)
                );
            }

            UpdateCannotMoveDebugText("Decision triggered", hit.collider.name);

            return true;
        }

        if (debugCannotMove)
        {
            Debug.Log("[No wall collision detection]");
        }

        return false;
    }

    void ShowCannotMoveMessage()
    {
        cannotMoveTextShowCount++;

        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayCannotMoveSE();
        }

        if (debugCannotMove)
        {
            Debug.Log("[Request to display non-movable characters] Count: " + cannotMoveTextShowCount);
        }

        if (cannotMoveText == null)
        {
            Debug.LogWarning("[Failed to display non-movable characters] CannotMoveText is not connected.");
            UpdateCannotMoveDebugText("Failed to display text", "CannotMoveText None");
            return;
        }

        if (messageCoroutine != null)
        {
            StopCoroutine(messageCoroutine);
        }

        messageCoroutine = StartCoroutine(ShowCannotMoveMessageCoroutine());
    }
    public void HideCannotMoveMessage()
    {
        if (messageCoroutine != null)
        {
            StopCoroutine(messageCoroutine);
            messageCoroutine = null;
        }

        if (cannotMoveText != null)
        {
            cannotMoveText.gameObject.SetActive(false);
        }

        if (cannotMoveDebugText != null)
        {
            cannotMoveDebugText.gameObject.SetActive(false);
        }
    }
    void UpdateCannotMoveDebugText(string state, string detail)
    {
        if (cannotMoveDebugText == null)
        {
            return;
        }

        cannotMoveDebugText.text =
            "Cannot Move Debug\n" +
            "State : " + state + "\n" +
            "Detail : " + detail + "\n" +
            "Judge Count : " + cannotMoveJudgeCount + "\n" +
            "Text Count : " + cannotMoveTextShowCount;
    }

    IEnumerator ShowCannotMoveMessageCoroutine()
    {
        cannotMoveText.gameObject.SetActive(true);
        cannotMoveText.text = "ˆÚ“®•s‰Â";

        if (debugCannotMove)
        {
            Debug.Log("[Immovable characters are actually displayed]");
        }

        UpdateCannotMoveDebugText("Characters displayed", "ˆÚ“®•s‰Â");

        yield return new WaitForSecondsRealtime(messageTime);

        cannotMoveText.gameObject.SetActive(false);

        if (debugCannotMove)
        {
            Debug.Log("[Hide immovable characters]");
        }
    }


    Vector3 GetDirectionFromSelection(int directionIndex)
    {
        if (directionIndex == -1)
        {
            return Quaternion.AngleAxis(-90f, currentUp) * baseForward;
        }
        else if (directionIndex == 0)
        {
            return baseForward;
        }
        else
        {
            return Quaternion.AngleAxis(90f, currentUp) * baseForward;
        }
    }
    void RotateVisualToSelectedDirection()
    {
        Quaternion targetRotation = Quaternion.LookRotation(selectedMoveDirection, currentUp);

        if (rotateCoroutine != null)
        {
            StopCoroutine(rotateCoroutine);
        }

        rotateCoroutine = StartCoroutine(RotateVisualCoroutine(targetRotation));
    }

    IEnumerator RotateVisualCoroutine(Quaternion targetRotation)
    {
        isMoving = true;

        Quaternion startRotation = transform.rotation;
        float elapsedTime = 0f;

        while (elapsedTime < rotationDuration)
        {
            float t = elapsedTime / rotationDuration;

            transform.rotation = Quaternion.Slerp(
                startRotation,
                targetRotation,
                t
            );

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        transform.rotation = targetRotation;

        isMoving = false;
        rotateCoroutine = null;
    }
    IEnumerator MoveToSelectedDirection()
    {
        isMoving = true;

        if (IsWallInDirection(selectedMoveDirection))
        {
            ShowCannotMoveMessage();
            isMoving = false;
            yield break;
        }
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayMoveSE();
        }

        RaycastHit nextSurfaceHit;
        if (TryGetGroundInDirection(selectedMoveDirection, out nextSurfaceHit))
        {
            yield return MoveToNewSurface(nextSurfaceHit);


            baseForward = Vector3.ProjectOnPlane(transform.forward, currentUp).normalized;

            selectedDirection = 0;
            selectedMoveDirection = baseForward;

            transform.rotation = Quaternion.LookRotation(baseForward, currentUp);

            isMoving = false;
            yield break;
        }


        Vector3 startPos = transform.position;
        Vector3 targetPos = startPos + selectedMoveDirection.normalized * moveDistance;

        float elapsedTime = 0f;

        while (elapsedTime < moveDuration)
        {
            float t = elapsedTime / moveDuration;
            transform.position = Vector3.Lerp(startPos, targetPos, t);

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        transform.position = targetPos;


        baseForward = selectedMoveDirection.normalized;


        selectedDirection = 0;
        selectedMoveDirection = baseForward;

        transform.rotation = Quaternion.LookRotation(baseForward, currentUp);

        isMoving = false;
    }

}

