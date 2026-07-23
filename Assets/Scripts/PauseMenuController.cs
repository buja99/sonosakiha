using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class PauseMenuController : MonoBehaviour
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
    public string titleSceneName = "TitleScene";
    public string mainSceneName = "MainScene";

    [Header("Stage Clear")]
    public StageClearUIController stageClearUI;

    private int selectedIndex = 0;
    private bool isOpen = false;

    void Start()
    {
        if (pausePanel != null)
        {
            pausePanel.SetActive(false);
        }

        if (stageClearUI == null)
        {
            stageClearUI = FindFirstObjectByType<StageClearUIController>(FindObjectsInactive.Include);
        }
    }

    void Update()
    {
        
        if (stageClearUI != null && stageClearUI.IsOpen)
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
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlayDecideSE();
            }

            Decide();
        }
    }

    void OpenPauseMenu()
    {
        PlayerController playerController = FindFirstObjectByType<PlayerController>();

        if (playerController != null)
        {
            playerController.HideCannotMoveMessage();
        }

        if (playerController != null)
        {
            playerController.SetControlEnabled(false);
        }

        isOpen = true;
        selectedIndex = 0;

        Time.timeScale = 0f;

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

        Time.timeScale = 1f;

        pausePanel.SetActive(false);

        PlayerController playerController = FindFirstObjectByType<PlayerController>();

        if (playerController != null)
        {
            playerController.SetControlEnabled(true);
        }
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

        if (arrowText != null && menuTexts[selectedIndex] != null)
        {
            Vector3 pos = menuTexts[selectedIndex].rectTransform.localPosition;
            arrowText.localPosition = new Vector3(pos.x + arrowOffsetX, pos.y, pos.z);
        }
    }

    void Decide()
    {
        Time.timeScale = 1f;

        // 0 = retry
        if (selectedIndex == 0)
        {
            SceneManager.LoadScene(mainSceneName);
        }
        // 1 = select
        else if (selectedIndex == 1)
        {
            SceneManager.LoadScene(GetStageSelectSceneName());
        }
        // 2 = title
        else if (selectedIndex == 2)
        {
            SceneManager.LoadScene(titleSceneName);
        }
    }

    string GetStageSelectSceneName()
    {
        switch (GameData.SelectedDifficulty)
        {
            case DifficultyType.Easy:
                return "EasyStageScene";

            case DifficultyType.Normal:
                return "NormalStageScene";

            case DifficultyType.Hard:
                return "HardStageScene";

            case DifficultyType.RandomHell:
                return "SelectDifficultyScene";

            default:
                return "SelectDifficultyScene";
        }
    }
}