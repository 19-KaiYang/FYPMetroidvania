using UnityEngine;

public class MainMenuAnimationEvent : MonoBehaviour
{
    public MainMenuUI mainMenuUI;
    public GameObject[] image;

    public void StartGame()
    {
        mainMenuUI.NewGame();
    }

    public void HideImage()
    {
        foreach (var item in image)
        {
            item.gameObject.SetActive(false);
        }
    }
}
