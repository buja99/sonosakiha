using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class TutorialPauseMenuController : MonoBehaviour
{
    [Header("UI")]
    public GameObject pausePanel;
    public TextMeshProUGUI[] menuTexts;
    public RectTransform arrowText;

    [Header("Font Size")]
    public float normalFontSize = 48f;
    public float selectedFontSize = 60f;

    [Header("Arrow")]
    public float arrowOffsetX = -60f;

    [Header("Scene Names")]
    public string tutorialSceneName = "TutorialScene";
    public string difficultySceneName = "SelectDifficultyScene";
    public string titleSceneName = "TitleScene";

    [Header("Tutorial Guide")]
    public TutorialGuideController tutorialGuide;

    [Header("Disable While Pause Open")]
    public Behaviour[] controlsToDisable;

    private int selectedIndex = 0;
    private bool isOpen = false;

    void Start()
    {
        if (pausePanel != null)
        {
            pausePanel.SetActive(false);
        }

        if (tutorialGuide == null)
        {
            tutorialGuide = FindFirstObjectByType<TutorialGuideController>(FindObjectsInactive.Include);
        }
    }

    void Update()
    {
        if (tutorialGuide != null && tutorialGuide.IsOpen)
        {
            return;
        }

        if (!isOpen)
        {
            if (Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.R))
            {
                if (AudioManager.Instance != null)
                {
                    AudioManager.Instance.PlayDecideSE();
                }
                OpenPauseMenu();
            }

            return;
        }

        if (Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.R))
        {
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlayDecideSE();
            }
            ClosePauseMenu();
            return;
        }

        if (Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.UpArrow))
        {
            selectedIndex--;

            if (selectedIndex < 0)
            {
                selectedIndex = menuTexts.Length - 1;
            }
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlayUIMoveSE();
            }
            UpdateMenuVisual();
        }

        if (Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.DownArrow))
        {
            selectedIndex++;

            if (selectedIndex >= menuTexts.Length)
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
            Decide();
        }
    }

    void OpenPauseMenu()
    {
        isOpen = true;
        selectedIndex = 0;

        Time.timeScale = 0f;
        SetControlsActive(false);

        if (pausePanel != null)
        {
            pausePanel.SetActive(true);
            pausePanel.transform.SetAsLastSibling();
        }

        UpdateMenuVisual();
    }

    void ClosePauseMenu()
    {
        isOpen = false;

        if (pausePanel != null)
        {
            pausePanel.SetActive(false);
        }

        Time.timeScale = 1f;
        SetControlsActive(true);
    }

    void UpdateMenuVisual()
    {
        if (menuTexts == null || menuTexts.Length == 0) return;

        for (int i = 0; i < menuTexts.Length; i++)
        {
            if (menuTexts[i] == null) continue;

            if (i == selectedIndex)
            {
                menuTexts[i].fontSize = selectedFontSize;
            }
            else
            {
                menuTexts[i].fontSize = normalFontSize;
            }
        }

        if (selectedIndex < 0 || selectedIndex >= menuTexts.Length) return;
        if (arrowText == null) return;
        if (menuTexts[selectedIndex] == null) return;

        Vector3 pos = menuTexts[selectedIndex].rectTransform.localPosition;
        arrowText.localPosition = new Vector3(pos.x + arrowOffsetX, pos.y, pos.z);
    }

    void Decide()
    {
        Time.timeScale = 1f;

        if (selectedIndex == 0)
        {
            // 説明
            isOpen = false;

            if (pausePanel != null)
            {
                pausePanel.SetActive(false);
            }

            if (tutorialGuide != null)
            {
                tutorialGuide.OpenGuide(0);
            }
        }
        else if (selectedIndex == 1)
        {
            // もう一度
            SceneManager.LoadScene(tutorialSceneName);
        }
        else if (selectedIndex == 2)
        {
            // 難易度選択へ
            SceneManager.LoadScene(difficultySceneName);
        }
        else if (selectedIndex == 3)
        {
            // タイトルへ
            SceneManager.LoadScene(titleSceneName);
        }
    }

    void SetControlsActive(bool active)
    {
        if (controlsToDisable == null) return;

        for (int i = 0; i < controlsToDisable.Length; i++)
        {
            if (controlsToDisable[i] != null)
            {
                controlsToDisable[i].enabled = active;
            }
        }
    }
}