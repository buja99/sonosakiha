using UnityEngine;

public class GoalController : MonoBehaviour
{
    public StageClearUIController stageClearUI;
    public string playerTag = "Player";

    private bool isCleared = false;

    void Awake()
    {
        Collider col = GetComponent<Collider>();

        if (col == null)
        {
            Debug.LogWarning("The Goal does not have a Collider: " + gameObject.name);
        }
        else
        {
            col.isTrigger = true;
            Debug.Log("Goal Collider Trigger ON: " + gameObject.name);
        }

        Rigidbody rb = GetComponent<Rigidbody>();

        if (rb == null)
        {
            rb = gameObject.AddComponent<Rigidbody>();
        }

        rb.useGravity = false;
        rb.isKinematic = true;
    }

    void Start()
    {
        if (stageClearUI == null)
        {
            stageClearUI = FindFirstObjectByType<StageClearUIController>(FindObjectsInactive.Include);
        }

        if (stageClearUI == null)
        {
            Debug.LogWarning("StageClearUIController was not found.");
        }
        else
        {
            Debug.Log("StageClearUIController found: " + stageClearUI.name);
        }
    }

    void OnTriggerEnter(Collider other)
    {
        Debug.Log("Goal Trigger Enter: " + other.name);
        CheckGoal(other);
    }

    void OnTriggerStay(Collider other)
    {
        CheckGoal(other);
    }

    void CheckGoal(Collider other)
    {
        if (isCleared) return;

        bool isPlayer =
            other.CompareTag(playerTag) ||
            other.GetComponent<PlayerController>() != null ||
            other.GetComponentInParent<PlayerController>() != null ||
            other.GetComponentInChildren<PlayerController>() != null;

        if (!isPlayer)
        {
            Debug.Log("Reached the goal but is not a player: " + other.name);
            return;
        }

        Debug.Log("GOAL CLEAR!");

        isCleared = true;

        if (stageClearUI != null)
        {
            stageClearUI.ShowStageClear();
        }
        else
        {
            Debug.LogWarning("Cannot display the UI because StageClearUI is null.");
        }
    }
}