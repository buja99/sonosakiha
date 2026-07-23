using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class StageClearUIController : MonoBehaviour
{
    [Header("UI")]
    public GameObject clearPanel;
    public TextMeshProUGUI clearTitleText;
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
    public string difficultySceneName = "SelectDifficultyScene";

    private int selectedIndex = 0;
    private bool isOpen = false;

    public bool IsOpen
    {
        get { return isOpen; }
    }

    void Start()
    {
        if (clearPanel != null)
        {
            clearPanel.SetActive(false);
        }
    }

    void Update()
    {
        if (!isOpen) return;

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

    public void ShowStageClear()
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayClearSE();
        }

        isOpen = true;
        selectedIndex = 0;

        if (clearPanel != null)
        {
            clearPanel.SetActive(true);
        }

        Time.timeScale = 0f;

        UpdateMenuVisual();
    }

    void UpdateMenuVisual()
    {
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

        if (selectedIndex == 0)
        {
            SceneManager.LoadScene(titleSceneName);
        }
        else if (selectedIndex == 1)
        {
            SceneManager.LoadScene(mainSceneName);
        }
        else if (selectedIndex == 2)
        {
            SceneManager.LoadScene(GetStageSelectSceneName());
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