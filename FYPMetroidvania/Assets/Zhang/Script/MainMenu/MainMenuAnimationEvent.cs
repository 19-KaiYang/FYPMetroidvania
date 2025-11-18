using UnityEngine;

public class MainMenuAnimationEvent : MonoBehaviour
{
    public MainMenuUI mainMenuUI;
    public GameObject[] image;

    private void Start()
    {
        //DontDestroyOnLoad(gameObject);
        mainMenuUI = MainMenuUI.instance;
    }
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

    public void PlayTruckHorn()
    {
        AudioManager.PlaySFX(SFXTYPE.REVVING);
    }

    public void PlayTruckCrash()
    {
        AudioManager.PlaySFX(SFXTYPE.TRUCK_CRASH);
    }
}
