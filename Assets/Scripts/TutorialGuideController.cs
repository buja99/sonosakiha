using UnityEngine;
using TMPro;
using System.Collections;
public class TutorialGuideController : MonoBehaviour
{
    [Header("UI")]
    public GameObject guidePanel;
    public TextMeshProUGUI titleText;
    public TextMeshProUGUI bodyText;
    public TextMeshProUGUI pageText;
    public TextMeshProUGUI arrowText;
    public TextMeshProUGUI closeHintText;

    [Header("Page Texts")]
    public string[] pageTitles = new string[4];
    [TextArea(3, 8)]
    public string[] pageBodies = new string[4];

    [Header("Settings")]
    public bool openOnStart = true;
    public bool pauseWhileOpen = true;

    [Header("Disable While Guide Open")]
    public Behaviour[] controlsToDisable;

    [Header("Extra UI")]
    public GameObject cannotMoveTextObject;

    private int currentPage = 0;
    private bool isOpen = false;

    public bool IsOpen
    {
        get { return isOpen; }
    }

    void Start()
    {
        SetDefaultTexts();

        if (guidePanel != null)
        {
            guidePanel.SetActive(false);
        }

        if (openOnStart)
        {
            StartCoroutine(OpenGuideAfterOneFrame());
        }
    }

    IEnumerator OpenGuideAfterOneFrame()
    {
        yield return null;

        OpenGuide(0);
    }
    void Update()
    {
        if (!isOpen) return;

        if (Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.LeftArrow))
        {
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlayUIMoveSE();
            }
            PreviousPage();
        }

        if (Input.GetKeyDown(KeyCode.D) || Input.GetKeyDown(KeyCode.RightArrow))
        {
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlayUIMoveSE();
            }
            NextPage();
        }

        if (Input.GetKeyDown(KeyCode.Space))
        {
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlayDecideSE();
            }
            if (currentPage == pageTitles.Length - 1)
            {
                CloseGuide();
            }
        }
    }

    public void OpenGuide(int startPage)
    {
        currentPage = Mathf.Clamp(startPage, 0, pageTitles.Length - 1);
        isOpen = true;

        if (guidePanel != null)
        {
            guidePanel.SetActive(true);
            guidePanel.transform.SetAsLastSibling();
        }

        if (pauseWhileOpen)
        {
            Time.timeScale = 0f;
        }

        SetControlsActive(false);
        UpdatePage();
    }

    public void CloseGuide()
    {
        isOpen = false;

        if (guidePanel != null)
        {
            guidePanel.SetActive(false);
        }

        if (cannotMoveTextObject != null)
        {
            cannotMoveTextObject.SetActive(false);
        }

        if (pauseWhileOpen)
        {
            Time.timeScale = 1f;
        }

        SetControlsActive(true);
    }

    void PreviousPage()
    {
        currentPage--;

        if (currentPage < 0)
        {
            currentPage = 0;
        }

        UpdatePage();
    }

    void NextPage()
    {
        currentPage++;

        if (currentPage >= pageTitles.Length)
        {
            currentPage = pageTitles.Length - 1;
        }

        UpdatePage();
    }

    void UpdatePage()
    {
        if (titleText != null)
        {
            titleText.text = pageTitles[currentPage];
        }

        if (bodyText != null)
        {
            bodyText.text = pageBodies[currentPage];
        }

        if (pageText != null)
        {
            pageText.text = (currentPage + 1).ToString() + " / " + pageTitles.Length.ToString();
        }

        if (arrowText != null)
        {
            arrowText.text = "A ◀    ▶D";
        }

        if (closeHintText != null)
        {
            if (currentPage == pageTitles.Length - 1)
            {
                closeHintText.gameObject.SetActive(true);
                closeHintText.text = "<b>Space </b>キーで説明を閉じます。";
            }
            else
            {
                closeHintText.gameObject.SetActive(false);
            }
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

    void SetDefaultTexts()
    {
        if (pageTitles == null || pageTitles.Length != 4)
        {
            pageTitles = new string[4];
        }

        if (pageBodies == null || pageBodies.Length != 4)
        {
            pageBodies = new string[4];
        }

        if (string.IsNullOrEmpty(pageTitles[0]))
        {
            pageTitles[0] = "ゲーム説明";
            pageTitles[1] = "UI操作説明";
            pageTitles[2] = "操作説明";
            pageTitles[3] = "移動不可説明";
        }

        if (string.IsNullOrEmpty(pageBodies[0]))
        {
            pageBodies[0] =
                "このゲームは、立方体の内側を進み、光るポータルを目指す迷路ゲームです。\n" +
                "一度進むと後戻りはできません。\n" +
                "分かれ道で進む方向を選び、ゴールを目指してください。";

            pageBodies[1] =
                "説明ページは A / D キーで切り替えます。\n" +
                "メニューでは W / S キーで項目を選びます。\n" +
                "Enter または Space キーで決定します。";

            pageBodies[2] =
                "A / D キーで進む方向を選択します。\n" +
                "W / Enter / Space キーで選択した方向へ進みます。\n" +
                "進んだ後は、その方向が新しい正面になります。";

            pageBodies[3] =
                "壁や進めない方向を選ぶと「移動不可」と表示されます。\n" +
                "その場合は、別の方向を選んでください。\n" +
                "最後のページでは Space キーで説明を閉じます。";
        }
    }
}