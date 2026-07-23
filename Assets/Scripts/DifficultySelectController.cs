using UnityEngine;
using UnityEngine.SceneManagement;

public class DifficultySelectController : MonoBehaviour
{
    public void SelectTutorial()
    {
        SceneManager.LoadScene("TutorialScene");
    }

    public void SelectEasy()
    {
        GameData.SelectedDifficulty = DifficultyType.Easy;
        SceneManager.LoadScene("EasyStageScene");
    }

    public void SelectNormal()
    {
        GameData.SelectedDifficulty = DifficultyType.Normal;
        SceneManager.LoadScene("NormalStageScene");
    }

    public void SelectHard()
    {
        GameData.SelectedDifficulty = DifficultyType.Hard;
        SceneManager.LoadScene("HardStageScene");
    }

    public void SelectRandomHell()
    {
        GameData.SelectedDifficulty = DifficultyType.RandomHell;
        GameData.SelectedStage = 0;
        SceneManager.LoadScene("MainScene");
    }

    public void SelectBack()
    {
        SceneManager.LoadScene("TitleScene");
    }
}