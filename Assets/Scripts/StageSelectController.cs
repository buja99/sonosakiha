using UnityEngine;
using UnityEngine.SceneManagement;

public class StageSelectController : MonoBehaviour
{
    public void SelectStage1()
    {
        SelectStage(1);
    }

    public void SelectStage2()
    {
        SelectStage(2);
    }

    public void SelectStage3()
    {
        SelectStage(3);
    }

    public void SelectStage4()
    {
        SelectStage(4);
    }

    public void SelectStage5()
    {
        SelectStage(5);
    }

    void SelectStage(int stageNumber)
    {
        GameData.SelectedStage = stageNumber;
        SceneManager.LoadScene("MainScene");
    }
}