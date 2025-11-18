using UnityEngine;
using UnityEngine.SceneManagement;

public class EndingScene : MonoBehaviour
{

    private void Start()
    {
        RoomSaveManager.ClearSaveData();
    }
    public void BackToMenu()
    {
        SceneManager.LoadScene("MainMenu");
    }
    public void Quit()
    {
        Application.Quit();
    }
}
