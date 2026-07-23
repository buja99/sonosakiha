using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

public class TitleMenuController : MonoBehaviour
{
    [Header("Menu Texts")]
    public TextMeshProUGUI startText;
    public TextMeshProUGUI quitText;
    public RectTransform arrowText;

    [Header("Text Size")]
    public float normalFontSize = 60f;
    public float selectedFontSize = 72f;

    [Header("Arrow Position")]
    public float arrowOffsetX = -60f;

    [Header("Scene Settings")]
    public string gameSceneName = "SelectDifficultyScene";

    private int selectedIndex = 0;

    void Start()
    {
        UpdateMenuVisual();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.W))
        {
            selectedIndex--;

            if (selectedIndex < 0)
            {
                selectedIndex = 1;
            }
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlayUIMoveSE();
            }
            UpdateMenuVisual();
        }

        if (Input.GetKeyDown(KeyCode.S))
        {
            selectedIndex++;

            if (selectedIndex > 1)
            {
                selectedIndex = 0;
            }
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlayUIMoveSE();
            }
            UpdateMenuVisual();
        }

        if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.Space))
        {
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlayDecideSE();
            }
            DecideMenu();
        }
    }

    void UpdateMenuVisual()
    {
        if (selectedIndex == 0)
        {
            startText.fontSize = selectedFontSize;
            quitText.fontSize = normalFontSize;

            MoveArrowTo(startText.rectTransform);
        }
        else
        {
            startText.fontSize = normalFontSize;
            quitText.fontSize = selectedFontSize;

            MoveArrowTo(quitText.rectTransform);
        }
    }

    void MoveArrowTo(RectTransform targetText)
    {
        Vector3 targetPos = targetText.anchoredPosition;
        arrowText.anchoredPosition = new Vector2(
            targetPos.x + arrowOffsetX,
            targetPos.y
        );
    }

    void DecideMenu()
    {
        if (selectedIndex == 0)
        {
            SceneManager.LoadScene(gameSceneName);
        }
        else
        {
            QuitGame();
        }
    }

    void QuitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}