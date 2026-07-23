using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

public class DifficultyMenuController : MonoBehaviour
{
    [Header("Menu Texts")]
    public TextMeshProUGUI tutorialText;
    public TextMeshProUGUI easyText;
    public TextMeshProUGUI normalText;
    public TextMeshProUGUI hardText;
    public TextMeshProUGUI hellText;
    public TextMeshProUGUI backText;

    [Header("Arrow")]
    public RectTransform arrowText;
    public float arrowOffsetX = -70f;

    [Header("Text Size")]
    public float normalFontSize = 50f;
    public float selectedFontSize = 62f;

    [Header("Description Position")]
    public Vector2 descriptionOffset = new Vector2(160f, -25f);

    [Header("Description")]
    public TextMeshProUGUI descriptionText;

    [Header("Scene Names")]
    public string tutorialSceneName = "TutorialScene";
    public string easyStageSelectSceneName = "EasyStageScene";
    public string normalStageSelectSceneName = "NormalStageScene";
    public string hardStageSelectSceneName = "HardStageScene";
    public string mainSceneName = "MainScene";
    public string titleSceneName = "TitleScene";

    private TextMeshProUGUI[] menuTexts;
    private int selectedIndex = 0;

    private string[] descriptions =
   {
        "基本操作を学びます。",
        "6×6の基本ステージ。",
        "7×7の標準ステージ。",
        "8×8の高難度ステージ。",
        "10×10ランダム生成モード。",
        "タイトルへ戻ります。"
    };

    void Start()
    {
        menuTexts = new TextMeshProUGUI[]
        {
            tutorialText,
            easyText,
            normalText,
            hardText,
            hellText,
            backText,
        };

        selectedIndex = 0;
        UpdateMenuVisual();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.W))
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

        if (Input.GetKeyDown(KeyCode.S))
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
            DecideMenu();
        }
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

        MoveArrowTo(menuTexts[selectedIndex].rectTransform);
        UpdateDescription();
    }

    void MoveArrowTo(RectTransform targetText)
    {
        if (arrowText == null) return;

        Vector2 targetPos = targetText.anchoredPosition;

        arrowText.anchoredPosition = new Vector2(
            targetPos.x + arrowOffsetX,
            targetPos.y
        );
    }

    void UpdateDescription()
    {
        if (descriptionText == null) return;

        descriptionText.text = descriptions[selectedIndex];

        RectTransform selectedTextRect = menuTexts[selectedIndex].rectTransform;
        RectTransform descriptionRect = descriptionText.rectTransform;

        descriptionRect.anchoredPosition = selectedTextRect.anchoredPosition + descriptionOffset;
    }

    void DecideMenu()
    {
        switch (selectedIndex)
        {
            case 0:
                SceneManager.LoadScene(tutorialSceneName);
                break;

            case 1:
                GameData.SelectedDifficulty = DifficultyType.Easy;
                SceneManager.LoadScene(easyStageSelectSceneName);
                break;

            case 2:
                GameData.SelectedDifficulty = DifficultyType.Normal;
                SceneManager.LoadScene(normalStageSelectSceneName);
                break;

            case 3:
                GameData.SelectedDifficulty = DifficultyType.Hard;
                SceneManager.LoadScene(hardStageSelectSceneName);
                break;

            case 4:
                GameData.SelectedDifficulty = DifficultyType.RandomHell;
                GameData.SelectedStage = 0;
                SceneManager.LoadScene(mainSceneName);
                break;

            case 5:
                SceneManager.LoadScene(titleSceneName);
                break;
        }
    }
}