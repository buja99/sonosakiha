using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class EasyStageSelectController : MonoBehaviour
{
    [Header("Stage Texts")]
    public TextMeshProUGUI[] stageTexts;

    [Header("Back Text")]
    public TextMeshProUGUI backText;

    [Header("Scene Name")]
    public string mainSceneName = "MainScene";
    public string difficultySceneName = "SelectDifficultyScene";

    [Header("Stage Positions")]
    public Vector2[] slotPositions = new Vector2[5];

    [Header("Text Size")]
    public float selectedFontSize = 60f;
    public float normalFontSize = 34f;

    [Header("Move Animation")]
    public float moveSpeed = 12f;

    [Header("Back Position")]
    public Vector2 normalBackPosition = new Vector2(780f, -80f);
    public Vector2 selectedBackPosition = new Vector2(-520f, 280f);
    public float backMoveSpeed = 12f;

    [Header("Stage Preview")]
    public MapGenerator previewMapGenerator;
    public TextMeshProUGUI previewText;
    public TextAsset[] easyStagePreviewCsvs = new TextAsset[4];
    public int previewFaceSize = 6;

    [Header("Preview Cube Transform")]
    public Vector3 previewCubePosition = new Vector3(-4f, 0f, 8f);
    public Vector3 previewCubeScale = new Vector3(0.05f, 0.05f, 0.05f);

    [Header("Preview Text Transform")]
    public Vector2 questionTextPosition = new Vector2(-250f, 20f);
    public Vector2 backPreviewTextPosition = new Vector2(-250f, 20f);
    public float questionFontSize = 160f;
    public float backPreviewFontSize = 90f;

    private int selectedIndex = 0;
    private bool isBackSelected = false;

    void Start()
    {
        slotPositions[0] = new Vector2(350f, -60f);
        slotPositions[1] = new Vector2(555f, 125f);
        slotPositions[2] = new Vector2(785f, -40f);
        slotPositions[3] = new Vector2(715f, -295f);
        slotPositions[4] = new Vector2(400f, -395f);

        if (backText != null)
        {
            backText.rectTransform.anchoredPosition = normalBackPosition;
        }


        UpdateStageVisualInstant();
   
    }

    void Update()
    {
        HandleInput();
        MoveStageTexts();
        MoveBackText();
    }

    void HandleInput()
    {
        if (Input.GetKeyDown(KeyCode.W))
        {
            if (isBackSelected)
            {
                isBackSelected = false;
                selectedIndex = 0;
            }
            else
            {
                selectedIndex++;

                if (selectedIndex >= stageTexts.Length)
                {
                    isBackSelected = true;
                    selectedIndex = 0;
                }
            }
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlayUIMoveSE();
            }
            UpdateTextSize();
            UpdatePreview();
        }

        if (Input.GetKeyDown(KeyCode.S))
        {
            if (isBackSelected)
            {
                isBackSelected = false;
                selectedIndex = stageTexts.Length - 1;
            }
            else
            {
                selectedIndex--;

                if (selectedIndex < 0)
                {
                    isBackSelected = true;
                    selectedIndex = 0;
                }
            }
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlayUIMoveSE();
            }
            UpdateTextSize();
            UpdatePreview();
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

    void MoveStageTexts()
    {
        for (int i = 0; i < stageTexts.Length; i++)
        {
            if (stageTexts[i] == null) continue;

            int slotIndex = GetSlotIndex(i);

            RectTransform rect = stageTexts[i].rectTransform;
            Vector2 targetPos = slotPositions[slotIndex];

            rect.anchoredPosition = Vector2.Lerp(
                rect.anchoredPosition,
                targetPos,
                Time.deltaTime * moveSpeed
            );
        }
    }

    int GetSlotIndex(int stageIndex)
    {
        int slotIndex = stageIndex - selectedIndex;

        if (slotIndex < 0)
        {
            slotIndex += stageTexts.Length;
        }

        return slotIndex;
    }

    void UpdateStageVisualInstant()
    {
        for (int i = 0; i < stageTexts.Length; i++)
        {
            if (stageTexts[i] == null) continue;

            int slotIndex = GetSlotIndex(i);
            stageTexts[i].rectTransform.anchoredPosition = slotPositions[slotIndex];
        }

        UpdateTextSize();
        UpdatePreview();
    }

    void UpdateTextSize()
    {
        for (int i = 0; i < stageTexts.Length; i++)
        {
            if (stageTexts[i] == null) continue;

            if (!isBackSelected && i == selectedIndex)
            {
                stageTexts[i].fontSize = selectedFontSize;
            }
            else
            {
                stageTexts[i].fontSize = normalFontSize;
            }
        }

        if (backText != null)
        {
            if (isBackSelected)
            {
                backText.fontSize = 110f;
                backText.text = "Back";
            }
            else
            {
                backText.fontSize = 50f;
                backText.text = "Back";
            }
        }
    }

    void Decide()
    {
        if (isBackSelected)
        {
            SceneManager.LoadScene(difficultySceneName);
            return;
        }

        GameData.SelectedDifficulty = DifficultyType.Easy;
        GameData.SelectedStage = selectedIndex + 1;

        SceneManager.LoadScene(mainSceneName);
    }


    void MoveBackText()
    {
        if (backText == null) return;

        RectTransform backRect = backText.rectTransform;

        Vector2 targetPos;

        if (isBackSelected)
        {
            targetPos = selectedBackPosition;
        }
        else
        {
            targetPos = normalBackPosition;
        }

        backRect.anchoredPosition = Vector2.Lerp(
            backRect.anchoredPosition,
            targetPos,
            Time.deltaTime * backMoveSpeed
        );
    }

    void UpdatePreview()
    {
        if (previewText != null)
        {
            previewText.gameObject.SetActive(false);
        }

        if (previewMapGenerator != null)
        {
            previewMapGenerator.gameObject.SetActive(true);
        }

        // Back 
        if (isBackSelected)
        {
            if (previewMapGenerator != null)
            {
                previewMapGenerator.gameObject.SetActive(false);
            }

            if (previewText != null)
            {
                previewText.gameObject.SetActive(true);
                previewText.text = "–ß‚é";
                previewText.fontSize = backPreviewFontSize;
                previewText.rectTransform.anchoredPosition = backPreviewTextPosition;
            }

            return;
        }

        // Stage 1~4 
        if (selectedIndex >= 0 && selectedIndex <= 3)
        {
            if (previewMapGenerator != null)
            {
                previewMapGenerator.gameObject.SetActive(true);

                previewMapGenerator.previewPosition = previewCubePosition;
                previewMapGenerator.previewScale = previewCubeScale;
                previewMapGenerator.rotatePreview = true;

                if (easyStagePreviewCsvs[selectedIndex] != null)
                {
                    previewMapGenerator.GeneratePreviewMap(
                        easyStagePreviewCsvs[selectedIndex],
                        previewFaceSize
                    );
                }
            }

            return;
        }

        // Stage ? 
        if (selectedIndex == 4)
        {
            if (previewMapGenerator != null)
            {
                previewMapGenerator.gameObject.SetActive(false);
            }

            if (previewText != null)
            {
                previewText.gameObject.SetActive(true);
                previewText.text = "Random";
                previewText.fontSize = questionFontSize;
                previewText.rectTransform.anchoredPosition = questionTextPosition;
            }
        }
    }
}

