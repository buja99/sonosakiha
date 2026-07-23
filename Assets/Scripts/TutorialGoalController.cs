using UnityEngine;
using UnityEngine.SceneManagement;

public class TutorialGoalController : MonoBehaviour
{
    public string difficultySceneName = "SelectDifficultyScene";
    public string playerTag = "Player";

    private bool isCleared = false;

    void Awake()
    {
        Collider col = GetComponent<Collider>();

        if (col != null)
        {
            col.isTrigger = true;
        }

        Rigidbody rb = GetComponent<Rigidbody>();

        if (rb == null)
        {
            rb = gameObject.AddComponent<Rigidbody>();
        }

        rb.useGravity = false;
        rb.isKinematic = true;
    }

    void OnTriggerEnter(Collider other)
    {
        if (isCleared) return;

        bool isPlayer =
            other.CompareTag(playerTag) ||
            other.GetComponentInParent<PlayerController>() != null;

        if (!isPlayer) return;

        isCleared = true;
        Time.timeScale = 1f;

        SceneManager.LoadScene(difficultySceneName);
    }
}